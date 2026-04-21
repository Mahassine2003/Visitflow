from __future__ import annotations

from datetime import date, datetime
import io
import re

import pytesseract
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
from fastapi import FastAPI, File, UploadFile
from pydantic import BaseModel
from PIL import Image
import fitz  # PyMuPDF
import docx  # python-docx


class InsuranceResult(BaseModel):
    isValid: bool
    status: str | None = None  # VALID / INVALID / UNKNOWN / MANUAL
    year: int | None = None
    startDate: str | None = None
    endDate: str | None = None
    rawText: str


app = FastAPI(title="VisitFlow AI Microservice")

# Années de police RC / documents récents : au-delà, on ignore (naissance, bruit OCR, etc.).
_MIN_POLICY_YEAR = 2000
_MAX_POLICY_YEAR = 2100


def _is_plausible_insurance_date(d: date) -> bool:
    return _MIN_POLICY_YEAR <= d.year <= _MAX_POLICY_YEAR

# Mois en arabe (calendrier grégorien courant sur les documents) — clés normalisées sans diacritiques.
_AR_MONTHS: dict[str, int] = {
    "يناير": 1,
    "فبراير": 2,
    "مارس": 3,
    "أبريل": 4,
    "ابريل": 4,
    "إبريل": 4,
    "مايو": 5,
    "يونيو": 6,
    "يوليو": 7,
    "أغسطس": 8,
    "اغسطس": 8,
    "سبتمبر": 9,
    "أكتوبر": 10,
    "اكتوبر": 10,
    "نوفمبر": 11,
    "ديسمبر": 12,
}

_MONTHS: dict[str, int] = {
    # FR
    "janvier": 1, "janv": 1, "jan": 1,
    "fevrier": 2, "février": 2, "fev": 2, "fév": 2,
    "mars": 3,
    "avril": 4, "avr": 4,
    "mai": 5,
    "juin": 6,
    "juillet": 7, "juil": 7,
    "aout": 8, "août": 8,
    "septembre": 9, "sept": 9,
    "octobre": 10, "oct": 10,
    "novembre": 11, "nov": 11,
    "decembre": 12, "décembre": 12, "dec": 12, "déc": 12,
    # EN
    "january": 1, "jan.": 1,
    "february": 2, "feb": 2, "feb.": 2,
    "march": 3,
    "april": 4, "apr": 4, "apr.": 4,
    "may": 5,
    "june": 6,
    "july": 7,
    "august": 8, "aug": 8, "aug.": 8,
    "september": 9, "sep": 9, "sep.": 9,
    "october": 10, "oct.": 10,
    "november": 11, "nov.": 11,
    "december": 12, "dec.": 12,
}

_FR_UNITS: dict[str, int] = {
    "zero": 0, "zéro": 0,
    "un": 1, "une": 1, "premier": 1,
    "deux": 2,
    "trois": 3,
    "quatre": 4,
    "cinq": 5,
    "six": 6,
    "sept": 7,
    "huit": 8,
    "neuf": 9,
    "dix": 10,
    "onze": 11,
    "douze": 12,
    "treize": 13,
    "quatorze": 14,
    "quinze": 15,
    "seize": 16,
    "dix-sept": 17, "dix sept": 17,
    "dix-huit": 18, "dix huit": 18,
    "dix-neuf": 19, "dix neuf": 19,
}

_FR_TENS: dict[str, int] = {
    "vingt": 20,
    "trente": 30,
    "quarante": 40,
    "cinquante": 50,
    "soixante": 60,
}


def _arabic_digits_to_ascii(s: str) -> str:
    """Chiffres arabo-indiens (٠١٢…) et persans (۰۱۲…) → ASCII."""
    trans = str.maketrans(
        "٠١٢٣٤٥٦٧٨٩۰۱۲۳۴۵۶۷۸۹",
        "01234567890123456789",
    )
    return s.translate(trans)


def _strip_arabic_diacritics(s: str) -> str:
    """Retire tatwīl et marques de vocalisation arabes pour matcher les mois."""
    # Tatwīl + hamza combining + vocalisations courantes
    return re.sub(r"[\u0640\u0610-\u061A\u064B-\u065F\u0670\u06D6-\u06ED]", "", s)


def _normalize_arabic_month_key(mon: str) -> str:
    m = _strip_arabic_diacritics(mon.strip())
    m = m.replace("أ", "ا").replace("إ", "ا").replace("آ", "ا")
    m = m.replace("ى", "ي").replace("ة", "ه")
    return m


def _arabic_month_to_num(mon: str) -> int | None:
    key = _normalize_arabic_month_key(mon)
    if key in _AR_MONTHS:
        return _AR_MONTHS[key]
    if len(key) >= 2:
        for k, v in _AR_MONTHS.items():
            if k.startswith(key) or key.startswith(k):
                return v
    return None


def _normalize_text(s: str) -> str:
    s = s.replace("\u00A0", " ")
    s = re.sub(r"[‐‑‒–—−]", "-", s)  # unify dashes
    # OCR often splits years with spaces: "202 6" / "202 8" -> "2026" / "2028"
    s = re.sub(r"\b((?:19|20))\s*(\d)\s*(\d)\s*(\d)\b", r"\1\2\3\4", s)
    s = re.sub(r"\b((?:19|20)\d)\s+(\d)\b", r"\1\2", s)
    s = re.sub(r"\s+", " ", s)
    return s.strip()


def _parse_fr_0_99(words: str) -> int | None:
    w = words.lower().strip()
    w = _normalize_text(w).replace(" et ", " ")
    w = w.replace("-", " ")
    tokens = [t for t in w.split(" ") if t]
    if not tokens:
        return None

    joined = " ".join(tokens)
    if joined in _FR_UNITS:
        return _FR_UNITS[joined]

    if tokens[0] in _FR_TENS:
        base = _FR_TENS[tokens[0]]
        if len(tokens) == 1:
            return base
        rest = " ".join(tokens[1:])
        if rest in _FR_UNITS:
            return base + _FR_UNITS[rest]
        return None

    # 70..79
    if tokens and tokens[0] == "soixante" and len(tokens) >= 2:
        rest = " ".join(tokens[1:])
        if rest in _FR_UNITS and 10 <= _FR_UNITS[rest] <= 19:
            return 60 + _FR_UNITS[rest]

    # 80..99
    if tokens[:2] == ["quatre", "vingt"]:
        if len(tokens) == 2:
            return 80
        rest = " ".join(tokens[2:])
        if rest in _FR_UNITS:
            return 80 + _FR_UNITS[rest]
        return None

    return None


def _parse_year_from_words(text: str) -> int | None:
    t = _normalize_text(text.lower())
    m = re.search(
        r"\bdeux mille ([a-z0-9\s\-]+?)(?=\b(?:au|a|à|jusqu|jusque|to|until)\b|$)",
        t,
    )
    if not m:
        return None
    tail = m.group(1).strip()
    tail = " ".join(tail.replace("-", " ").split()[:4])
    n = _parse_fr_0_99(tail)
    if n is None:
        return None
    return 2000 + n


def _safe_date(y: int, m: int, d: int) -> date | None:
    try:
        return date(y, m, d)
    except Exception:
        return None


def _coerce_ocr_day(day: int) -> int:
    # Typical OCR: "531" instead of "31" (extra leading digit)
    if day > 99:
        return day % 100
    return day


def _normalize_month_token(mon: str) -> str:
    m = mon.lower().strip(".")
    m = re.sub(r"[^a-zéèêëàâîïôöùûüç]", "", m)
    # Common OCR month typos (FR)
    if m.startswith("janve"):
        m = "janv"
    if m.startswith("deeombre"):
        m = "decembre"
    if m.startswith("decombre"):
        m = "decembre"
    if m.startswith("decemb"):
        m = "decembre"
    if m in _MONTHS:
        return m
    # Best-effort for OCR typos: match by prefix (>=3 chars)
    if len(m) >= 3:
        for k in _MONTHS.keys():
            kk = k.lower().strip(".")
            if kk.startswith(m) or m.startswith(kk):
                return kk
        for k in _MONTHS.keys():
            kk = k.lower().strip(".")
            if m[:3] == kk[:3]:
                return kk
    return m


def _parse_date_expr(expr: str) -> date | None:
    e = _normalize_text(_arabic_digits_to_ascii(expr)).strip()
    # Common FR ordinals / OCR artifacts:
    # - "1er janvier 2026" -> "1 janvier 2026"
    # - OCR sometimes reads "1er" as "ter" ("ter Janvier 2026")
    # - OCR sometimes reads "1er" as "Yer"
    e = re.sub(r"(?i)\b(\d{1,2})\s*er\b", r"\1", e)
    e = re.sub(r"(?i)\byer\b", "1", e)
    e = re.sub(
        r"(?i)\bter\s+([a-zéèêëàâîïôöùûüç\.]+)\s+(\d{4})\b",
        r"1 \1 \2",
        e,
    )
    # Year split edge case inside a date blob: "202 8" -> "2028"
    e = re.sub(r"\b((?:19|20)\d)\s+(\d)\b", r"\1\2", e)

    m = re.fullmatch(r"(\d{4})[./-](\d{1,2})[./-](\d{1,2})", e)
    if m:
        return _safe_date(int(m.group(1)), int(m.group(2)), int(m.group(3)))

    m = re.fullmatch(r"(\d{1,2})[./-](\d{1,2})[./-](\d{4})", e)
    if m:
        return _safe_date(int(m.group(3)), int(m.group(2)), int(m.group(1)))

    m = re.fullmatch(r"(\d{1,2})[./-](\d{1,2})[./-](\d{2})", e)
    if m:
        yy = int(m.group(3))
        return _safe_date(2000 + yy, int(m.group(2)), int(m.group(1)))

    m = re.fullmatch(r"(\d{1,3})\s+([a-zéèêëàâîïôöùûüç\.]+)\s+(\d{4})", e, flags=re.IGNORECASE)
    if m:
        d = _coerce_ocr_day(int(m.group(1)))
        mon = _normalize_month_token(m.group(2))
        y = int(m.group(3))
        if mon in _MONTHS:
            return _safe_date(y, _MONTHS[mon], d)

    # Jour + mois arabe + année (ex. 15 يناير 2026 ou ١٥ يناير ٢٠٢٦ après conversion)
    m = re.fullmatch(
        r"(\d{1,3})\s+([\u0600-\u06FF]+)\s+(\d{4})",
        e,
    )
    if m:
        d = _coerce_ocr_day(int(m.group(1)))
        mn = _arabic_month_to_num(m.group(2))
        y = int(m.group(3))
        if mn is not None:
            return _safe_date(y, mn, d)

    m = re.fullmatch(r"([a-zéèêëàâîïôöùûüç\.]+)\s+(\d{4})", e, flags=re.IGNORECASE)
    if m:
        mon = m.group(1).lower().strip(".")
        y = int(m.group(2))
        if mon in _MONTHS:
            return _safe_date(y, _MONTHS[mon], 1)

    m = re.fullmatch(r"([\u0600-\u06FF]+)\s+(\d{4})", e)
    if m:
        mn = _arabic_month_to_num(m.group(1))
        y = int(m.group(2))
        if mn is not None:
            return _safe_date(y, mn, 1)

    y = _parse_year_from_words(e)
    if y is not None:
        return _safe_date(y, 1, 1)

    return None


def _parse_first_date_in_blob(blob: str) -> date | None:
    b = _normalize_text(_arabic_digits_to_ascii(blob))
    # numeric first
    for m in re.finditer(r"\b(\d{4}[./-]\d{1,2}[./-]\d{1,2})\b", b):
        d = _parse_date_expr(m.group(1))
        if d:
            return d
    for m in re.finditer(r"\b(\d{1,2}[./-]\d{1,2}[./-]\d{4})\b", b):
        d = _parse_date_expr(m.group(1))
        if d:
            return d
    for m in re.finditer(r"\b(\d{1,2}[./-]\d{1,2}[./-]\d{2})\b", b):
        d = _parse_date_expr(m.group(1))
        if d:
            return d
    # word dates: accept extra tail after the date, so we capture only the date chunk
    for m in re.finditer(
        r"\b((?:(?:\d{1,3})\s*(?:er)?|ter|yer)\s+[a-zéèêëàâîïôöùûüç\.]+\s+\d{4})\b",
        b,
        flags=re.IGNORECASE,
    ):
        d = _parse_date_expr(m.group(1))
        if d:
            return d
    for m in re.finditer(
        r"(?<![\d\u0660-\u0669\u06F0-\u06F9])(\d{1,3}\s+[\u0600-\u06FF]{2,24}\s+\d{4})(?!\d)",
        b,
    ):
        d = _parse_date_expr(m.group(1))
        if d:
            return d
    return None


def _extract_dates(text: str) -> tuple[date | None, date | None, int | None]:
    t = _normalize_text(_arabic_digits_to_ascii(text))
    t_l = t.lower()
    preferred_start: date | None = None
    preferred_end: date | None = None

    _d_pat = r"(\d{1,2}[./-]\d{1,2}[./-]\d{2,4})"

    # Arabe OCR collé (ex. police UAE : «من تاريخ14أبريل؛2021وما») — pas d’espaces jour/mois.
    m_ar_glue = re.search(
        r"تاريخ\s*(\d{1,3})([\u0600-\u06FF]{2,22})[؛,.\s:]*((?:19|20)\d{2})",
        t,
    )
    if m_ar_glue:
        dday = _coerce_ocr_day(int(m_ar_glue.group(1)))
        mn = _arabic_month_to_num(m_ar_glue.group(2))
        y = int(m_ar_glue.group(3))
        if mn is not None:
            d_found = _safe_date(y, mn, dday)
            if d_found and _is_plausible_insurance_date(d_found):
                preferred_start = preferred_start or d_found

    # Anglais : «April 14; 2021» / «incepted on April 14, 2021» (mois avant jour)
    m_en_md = re.search(
        r"(?i)\b(january|february|march|april|may|june|july|august|september|october|november|december)\s+"
        r"(\d{1,2})\s*[;,]\s*((?:19|20)\d{2})\b",
        t,
    )
    if m_en_md:
        mon = m_en_md.group(1).lower()
        dday = _coerce_ocr_day(int(m_en_md.group(2)))
        y = int(m_en_md.group(3))
        if mon in _MONTHS:
            d_found = _safe_date(y, _MONTHS[mon], dday)
            if d_found and _is_plausible_insurance_date(d_found):
                preferred_start = preferred_start or d_found

    # Libellés arabes (effet / expiration) — avant FR/EN
    m_ar_eff = re.search(
        rf"(?:تاريخ\s*(?:ال)?بداية|تاريخ\s*السريان|ساري\s*من|من\s*تاريخ).{{0,120}}?{_d_pat}",
        t,
    )
    m_ar_exp = re.search(
        rf"(?:تاريخ\s*(?:ال)?انتهاء|انتهاء\s*الصلاحية|ينتهي|صالح\s*حتى|إلى\s*تاريخ|الى\s*تاريخ|حتى\s*تاريخ).{{0,120}}?{_d_pat}",
        t,
    )
    if m_ar_eff or m_ar_exp:
        d1 = _parse_date_expr(m_ar_eff.group(1)) if m_ar_eff else None
        d2 = _parse_date_expr(m_ar_exp.group(1)) if m_ar_exp else None
        if d1 and not _is_plausible_insurance_date(d1):
            d1 = None
        if d2 and not _is_plausible_insurance_date(d2):
            d2 = None
        if d1 is not None:
            preferred_start = preferred_start or d1
        if d2 is not None:
            preferred_end = preferred_end or d2

    # labelled fields ("Date d'effet", "date d'expiration") have priority
    m_eff = re.search(
        rf"(?i)\bdate\s*d[’']?\s*effet\b.{{0,80}}?{_d_pat}",
        t,
    )
    m_exp = re.search(
        rf"(?i)\bdate\s*d[’']?\s*expiration\b.{{0,80}}?{_d_pat}",
        t,
    )
    # FR : échéance / fin de contrat (souvent la date de fin de police)
    m_fr_echeance = re.search(
        rf"(?i)\b(?:date\s*(?:de\s*)?(?:échéance|echeance)|fin\s*(?:de\s*)?validité|"
        rf"échéance\s*(?:du\s*)?(?:contrat|police))\b.{{0,100}}?{_d_pat}",
        t,
    )
    if m_fr_echeance and not m_exp:
        m_exp = m_fr_echeance

    # EN : effective / expiration (libellés courants sur polices bilingues)
    m_en_start_lbl = re.search(
        rf"(?i)\b(?:effective\s*date|coverage\s*(?:begins?|starts?)|policy\s*start|"
        rf"date\s*of\s*commencement|inception\s*date|period\s*from)\b.{{0,100}}?{_d_pat}",
        t,
    )
    m_en_end_lbl = re.search(
        rf"(?i)\b(?:expir(?:y|ation)\s*date|coverage\s*ends?|policy\s*end|valid\s*until|"
        rf"renewal\s*date|period\s*(?:to|until|through))\b.{{0,100}}?{_d_pat}",
        t,
    )
    if m_en_start_lbl:
        d_en_s = _parse_date_expr(m_en_start_lbl.group(1))
        if d_en_s and _is_plausible_insurance_date(d_en_s):
            preferred_start = preferred_start or d_en_s
    if m_en_end_lbl:
        d_en_e = _parse_date_expr(m_en_end_lbl.group(1))
        if d_en_e and _is_plausible_insurance_date(d_en_e):
            preferred_end = preferred_end or d_en_e

    if m_eff:
        d1 = _parse_date_expr(m_eff.group(1))
        d2 = _parse_date_expr(m_exp.group(1)) if m_exp else None
        if d1 and not _is_plausible_insurance_date(d1):
            d1 = None
        if d2 and not _is_plausible_insurance_date(d2):
            d2 = None
        # Ne pas retourner trop tôt avec une seule date : on garde la préférence,
        # puis on laisse la détection globale tenter de retrouver une date de fin.
        if d1 is not None:
            preferred_start = d1
        if d2 is not None:
            preferred_end = d2
            preferred_start = preferred_start or d1
    elif m_exp:
        # Ex. libellé « date d'échéance » / « fin de validité » sans ligne « date d'effet » détectée.
        d2 = _parse_date_expr(m_exp.group(1))
        if d2 and _is_plausible_insurance_date(d2):
            preferred_end = preferred_end or d2

    # Many contracts state only a start date ("pour prendre effet le ...") without an explicit end date.
    m_start_only = re.search(
        rf"(?i)\b(?:pour\s+prendre\s+effet|prendre\s+effet)\b.{{0,60}}?{_d_pat}\b",
        t,
    )
    if m_start_only:
        d1 = _parse_date_expr(m_start_only.group(1))
        if d1 and _is_plausible_insurance_date(d1):
            preferred_start = preferred_start or d1

    # common phrasing without "du": "<date> au <date>"
    year_pat = r"(?:19|20)\s*\d\s*\d"
    m = re.search(
        rf"(?i)\b((?:\d{{1,3}}|ter|yer)\s+[a-zéèêëàâîïôöùûüç\.]+\s+{year_pat})\s+au\s+((?:\d{{1,3}}|ter|yer)\s+[a-zéèêëàâîïôöùûüç\.]+\s+{year_pat})\b",
        t,
    )
    if m:
        d1 = _parse_date_expr(m.group(1))
        d2 = _parse_date_expr(m.group(2))
        if d1 and d2 and abs(d2.year - d1.year) > 1:
            d2_aligned = _safe_date(d1.year, d2.month, d2.day)
            if d2_aligned:
                d2 = d2_aligned
        if d1 and d2 and _is_plausible_insurance_date(d1) and _is_plausible_insurance_date(d2):
            start = min(d1, d2)
            end = max(d1, d2)
            return (start, end, end.year)

    # explicit ranges
    range_patterns = [
        r"(?i)\bdu\s+(.{3,80}?)\s+au\s+(.{3,80}?)\b",
        r"(?i)\bde\s+(.{3,80}?)\s+à\s+(.{3,80}?)\b",
        r"(?i)\bfrom\s+(.{3,80}?)\s+to\s+(.{3,80}?)\b",
    ]
    for pat in range_patterns:
        m = re.search(pat, t)
        if m:
            d1 = _parse_first_date_in_blob(m.group(1))
            d2 = _parse_first_date_in_blob(m.group(2))
            if d1 and d2 and _is_plausible_insurance_date(d1) and _is_plausible_insurance_date(d2):
                # OCR can misread year (e.g. 2028 instead of 2026). If both dates exist but
                # years are far apart, try to align end year to start year.
                if abs(d2.year - d1.year) > 1:
                    d2_aligned = _safe_date(d1.year, d2.month, d2.day)
                    if d2_aligned:
                        d2 = d2_aligned
                start = min(d1, d2)
                end = max(d1, d2)
                return (start, end, end.year)

    candidates: list[date] = []

    # numeric
    for m in re.finditer(r"\b(\d{1,2}[./-]\d{1,2}[./-]\d{4})\b", t):
        d = _parse_date_expr(m.group(1))
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)
    for m in re.finditer(r"\b(\d{4}[./-]\d{1,2}[./-]\d{1,2})\b", t):
        d = _parse_date_expr(m.group(1))
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)
    for m in re.finditer(r"\b(\d{1,2}[./-]\d{1,2}[./-]\d{2})\b", t):
        d = _parse_date_expr(m.group(1))
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)

    # month words
    for m in re.finditer(r"\b(\d{1,2})\s+([a-zéèêëàâîïôöùûüç\.]+)\s+(\d{4})\b", t, flags=re.IGNORECASE):
        d = _parse_date_expr(m.group(0))
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)

    # "1 janvier deux mille vingt-six" (OCR peut ajouter du bruit après l'année)
    for m in re.finditer(r"\b(\d{1,2})\s+([a-zéèêëàâîïôöùûüç\.]+)\b", t_l, flags=re.IGNORECASE):
        day = int(m.group(1))
        mon = m.group(2).lower().strip(".")
        if mon not in _MONTHS:
            continue
        tail = t_l[m.end() : m.end() + 80]
        year = _parse_year_from_words(tail)
        if year:
            d = _safe_date(year, _MONTHS[mon], day)
            if d and _is_plausible_insurance_date(d):
                candidates.append(d)

    for m in re.finditer(r"\b([a-zéèêëàâîïôöùûüç\.]+)\s+(\d{4})\b", t_l, flags=re.IGNORECASE):
        mon = m.group(1).lower().strip(".")
        if mon in _MONTHS:
            d = _safe_date(int(m.group(2)), _MONTHS[mon], 1)
            if d and _is_plausible_insurance_date(d):
                candidates.append(d)

    for m in re.finditer(
        r"(?<![\d\u0660-\u0669\u06F0-\u06F9])(\d{1,3}\s+[\u0600-\u06FF]{2,24}\s+\d{4})(?!\d)",
        t,
    ):
        d = _parse_date_expr(m.group(1))
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)

    # Éviter « 15 يناير 2026 » → faux « يناير 2026 » (1er du mois).
    for m in re.finditer(
        r"(?<!\d\s)([\u0600-\u06FF]{2,24})\s+(\d{4})(?!\d)",
        t,
    ):
        mn = _arabic_month_to_num(m.group(1))
        if mn is None:
            continue
        d = _safe_date(int(m.group(2)), mn, 1)
        if d and _is_plausible_insurance_date(d):
            candidates.append(d)

    if candidates:
        candidates = sorted(set(candidates))
        if preferred_start is not None:
            # Si on a une date d'effet fiable, tenter d'associer la meilleure date de fin
            # détectée globalement (strictement postérieure à la date de début).
            later = [d for d in candidates if d > preferred_start]
            if preferred_end is not None:
                end = preferred_end
                start = preferred_start
                if end < start:
                    start, end = end, start
                return (start, end, end.year)
            if later:
                end = max(later)
                return (preferred_start, end, end.year)

        if preferred_end is not None and preferred_start is None:
            start = candidates[0]
            end = preferred_end
            if end < start:
                start, end = end, start
            return (start, end, end.year)

        if len(candidates) == 1:
            # Un seul date détectée: on la traite comme startDate uniquement.
            only = candidates[0]
            return (only, None, only.year)
        start = candidates[0]
        end = candidates[-1]
        # Sans libellé expiration ni plage « du … au … » : deux dates dans le même mois et proches
        # correspondent souvent à du bruit OCR (ex. effet au 1er + autre jour du mois), pas à une fin de contrat.
        # Même mois et très proches : souvent bruit OCR (ex. jour du mois mal lu), pas début + fin de police.
        if (
            start.year == end.year
            and start.month == end.month
            and 0 <= (end - start).days <= 10
        ):
            return (start, None, start.year)
        return (start, end, end.year)

    if preferred_start is not None or preferred_end is not None:
        if preferred_start is not None and preferred_end is not None:
            start = min(preferred_start, preferred_end)
            end = max(preferred_start, preferred_end)
            return (start, end, end.year)
        only = preferred_start or preferred_end
        return (only, None, only.year if only else None)

    years = [int(y) for y in re.findall(r"\b(20\d{2})\b", t)]
    y_words = _parse_year_from_words(t)
    if y_words is not None:
        years.append(y_words)
    year = max(years) if years else None
    return (None, None, year)


class ManualDocumentInput(BaseModel):
    isValid: bool
    status: str | None = None
    year: int | None = None
    startDate: str | None = None  # ISO yyyy-mm-dd recommandé
    endDate: str | None = None
    note: str | None = None


@app.post("/documents/manual", response_model=InsuranceResult)
async def manual_document(payload: ManualDocumentInput):
    st = payload.status or ("VALID" if payload.isValid else "INVALID")
    raw = payload.note or "MANUAL_ENTRY"
    return InsuranceResult(
        isValid=payload.isValid,
        status=st,
        year=payload.year,
        startDate=payload.startDate,
        endDate=payload.endDate,
        rawText=raw,
    )


@app.post("/insurance/ocr", response_model=InsuranceResult)
async def insurance_ocr(file: UploadFile = File(...)):
    """
    Reçoit une image d'assurance, applique l'OCR,
    extrait une année de type 20XX et indique si elle est encore valide.
    """
    filename = (file.filename or "").lower()
    content = await file.read()
    text = ""

    if filename.endswith(".pdf") or file.content_type == "application/pdf":
        try:
            doc = fitz.open(stream=content, filetype="pdf")
            for page in doc:
                page_text = page.get_text()
                # Certains PDFs contiennent une année en texte extractible mais les vraies dates
                # (début/fin) sont dans une zone scannée. On OCR si on ne voit pas de pattern date.
                has_date_like = bool(
                    re.search(
                        r"(?i)\b(\d{1,2}[./-]\d{1,2}[./-]\d{2,4})\b|"
                        r"\b(janvier|février|fevrier|mars|avril|mai|juin|juillet|août|aout|septembre|octobre|novembre|décembre|decembre)\b|"
                        r"\b(january|february|march|april|may|june|july|august|september|october|november|december)\b|"
                        r"يناير|فبراير|مارس|أبريل|ابريل|إبريل|مايو|يونيو|يوليو|أغسطس|اغسطس|سبتمبر|أكتوبر|اكتوبر|نوفمبر|ديسمبر",
                        page_text,
                    )
                )
                if not has_date_like:
                    pix = page.get_pixmap(matrix=fitz.Matrix(2.5, 2.5))
                    if pix.colorspace and pix.colorspace.n >= 4:
                        pix = fitz.Pixmap(fitz.csRGB, pix)
                    img = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)
                    page_text += "\n" + pytesseract.image_to_string(img, lang="eng+fra+ara")
                text += page_text + "\n"
        except Exception as e:
            return InsuranceResult(isValid=False, rawText=f"PDF decoding error: {str(e)}")
    elif filename.endswith(".docx") or filename.endswith(".doc"):
        try:
            doc = docx.Document(io.BytesIO(content))
            text = "\n".join([p.text for p in doc.paragraphs])
        except Exception as e:
            return InsuranceResult(isValid=False, rawText=f"Word decoding error: {str(e)}")
    else:
        try:
            image = Image.open(io.BytesIO(content))
            image.load()
            text = pytesseract.image_to_string(image, lang="eng+fra+ara")
        except Exception as e:
            return InsuranceResult(isValid=False, rawText=f"Image decoding error: {str(e)}")

    start_dt, end_dt, year = _extract_dates(text)
    start_date = start_dt.isoformat() if start_dt else None
    end_date = end_dt.isoformat() if end_dt else None

    today = date.today()
    if end_dt:
        is_valid = end_dt >= today
        status = "VALID" if is_valid else "INVALID"
    elif start_dt:
        # Certains documents ne mentionnent pas la date de fin. Dans ce cas,
        # on considère valide si la couverture a déjà démarré (start <= today).
        is_valid = start_dt <= today
        status = "VALID" if is_valid else "INVALID"
    elif year is not None:
        is_valid = year >= today.year
        status = "VALID" if is_valid else "INVALID"
    else:
        is_valid = False
        status = "UNKNOWN"

    # IMPORTANT: même si invalid, on renvoie quand même startDate/endDate si détectés
    return InsuranceResult(
        isValid=is_valid,
        status=status,
        year=year,
        startDate=start_date,
        endDate=end_date,
        rawText=text,
    )


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)

