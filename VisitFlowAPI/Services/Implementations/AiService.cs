using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VisitFlowAPI.DTOs.Ai;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class AiService : IAiService
{
    private readonly ILogger<AiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiService(ILogger<AiService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public Task ProcessOcrResultAsync(string ocrJson)
    {
        _logger.LogInformation("Received OCR result: {Json}", ocrJson);
        return Task.CompletedTask;
    }

    public async Task<InsuranceValidationResultDto> ValidateInsuranceAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required", nameof(file));
        }

        var baseUrl = _configuration.GetSection("Ai")["ServiceBaseUrl"] ?? "http://localhost:8000";
        var client = _httpClientFactory.CreateClient("AiService");

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");
        content.Add(fileContent, "file", file.FileName);

        var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/insurance/ocr", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<InsuranceValidationResultDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result is null)
        {
            throw new InvalidOperationException("Invalid response from AI service.");
        }

        // Some OCR responses return only `year` but leave `startDate`/`endDate` null.
        // We try to extract dates from `rawText` so the UI can always display them (even when invalid).
        if (!string.IsNullOrWhiteSpace(result.RawText))
        {
            var start = result.StartDate;
            var end = result.EndDate;

            if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
            {
                var extracted = TryExtractStartEndDatesFromRawText(result.RawText, result.Year);
                if (string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(extracted.StartDate))
                    result.StartDate = extracted.StartDate;
                if (string.IsNullOrWhiteSpace(end) && !string.IsNullOrWhiteSpace(extracted.EndDate))
                {
                    // Avoid setting a bogus endDate equal to startDate when the document has no expiration.
                    if (!string.Equals(extracted.EndDate, result.StartDate, StringComparison.Ordinal))
                    {
                        result.EndDate = extracted.EndDate;
                    }
                }
            }
        }

        // Règle métier : si aucune date d'expiration n'est trouvée, on considère l'assurance comme valide.
        if (string.IsNullOrWhiteSpace(result.EndDate))
        {
            result.IsValid = true;
            // Pas de libellé séparé : l’UI n’affiche que le badge VALID + année + date de début.
            result.Status = null;
        }

        // Année OCR incomplète ou aberrante (ex. 202 au lieu de 2026) : dériver du début si possible.
        if (result.Year is < 2000 or > 2100)
        {
            result.Year = null;
        }

        if (result.Year is null &&
            !string.IsNullOrWhiteSpace(result.StartDate) &&
            DateTime.TryParse(result.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDt))
        {
            result.Year = startDt.Year;
        }

        return result;
    }

    public async Task<DocumentValidationResultDto> ValidateDocumentAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required", nameof(file));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        var isPdf = ext == ".pdf" || contentType.Contains("application/pdf");
        var fileNameLower = (file.FileName ?? string.Empty).ToLowerInvariant();

        // Par défaut, on laisse l'IA traiter tous les documents (images, PDF, etc.)
        // sauf les PDF d'intervention et de blacklist, qui restent hors du flux IA.
        var isInterventionOrBlacklistPdf = isPdf &&
                                           (fileNameLower.Contains("intervention") ||
                                            fileNameLower.Contains("blacklist"));

        if (isInterventionOrBlacklistPdf)
        {
            return new DocumentValidationResultDto
            {
                AiSkipped = true,
                SkipReason = "Les PDF d'intervention ou de blacklist ne sont pas validés par l'IA.",
                ValidatedByAI = false,
                IsValid = false
            };
        }

        var insurance = await ValidateInsuranceAsync(file);
        return new DocumentValidationResultDto
        {
            AiSkipped = false,
            ValidatedByAI = true,
            IsValid = insurance.IsValid,
            Year = insurance.Year,
            StartDate = insurance.StartDate,
            EndDate = insurance.EndDate,
            RawText = insurance.RawText
        };
    }

    private const int MinInsuranceDocumentYear = 2000;

    private static bool TryMapEnglishMonth(string monthLower, out int month)
    {
        month = monthLower switch
        {
            "january" => 1,
            "february" => 2,
            "march" => 3,
            "april" => 4,
            "may" => 5,
            "june" => 6,
            "july" => 7,
            "august" => 8,
            "september" => 9,
            "october" => 10,
            "november" => 11,
            "december" => 12,
            _ => 0
        };
        return month != 0;
    }

    private static (string? StartDate, string? EndDate) TryExtractStartEndDatesFromRawText(string? rawText, int? fallbackYear)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return (null, null);

        // Example OCR: "Pour la peode au v01/2026 au 311272026"
        // So we try to detect:
        // - start as month/year (MM/YYYY) => day 01
        // - end as day/month/year, sometimes with missing separators => DDMMYYYY
        var text = rawText;

        // Anglais « April 14; 2021 » (mois avant jour) — courant sur polices bilingues arabe/anglais
        var englishMonthDayYearRegex = new Regex(
            @"\b(january|february|march|april|may|june|july|august|september|october|november|december)\s+"
            + @"(\d{1,2})\s*[;,]\s*((?:19|20)\d{2})\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // Start: MM/YYYY
        var startMonthYearRegex = new Regex(
            @"(?<!\d)(0[1-9]|1[0-2])\s*[/\-.]?\s*((?:19|20)\d{2})(?!\d)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // End: DD/MM/YYYY (with separators)
        var endDayMonthYearSeparatedRegex = new Regex(
            @"(?<!\d)(0?[1-9]|[12]\d|3[01])\s*[/\-.]\s*(0?[1-9]|1[0-2])\s*[/\-.]\s*((?:19|20)\d{2})(?!\d)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // End: DDMMYYYY (without separators, often OCR-compacted)
        var endDayMonthYearCompactRegex = new Regex(
            // Some OCR variants insert a stray digit between month and year (ex: 311272026 ~= 31/12/2026)
            @"(?<!\d)(0?[1-9]|[12]\d|3[01])((?:0?[1-9]|1[0-2]))\d?((?:19|20)\d{2})(?!\d)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        int? startMonth = null;
        int? startYear = null;
        int startIdx = -1;

        var englishMdy = englishMonthDayYearRegex.Match(text);
        if (englishMdy.Success)
        {
            var monthName = englishMdy.Groups[1].Value.ToLowerInvariant();
            var day = int.Parse(englishMdy.Groups[2].Value, CultureInfo.InvariantCulture);
            var y = int.Parse(englishMdy.Groups[3].Value, CultureInfo.InvariantCulture);
            if (TryMapEnglishMonth(monthName, out var sm) && y >= MinInsuranceDocumentYear && y <= 2100)
            {
                try
                {
                    var d = new DateTime(y, sm, day);
                    return (
                        StartDate: d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        EndDate: null
                    );
                }
                catch (ArgumentOutOfRangeException)
                {
                    // continue with other heuristics
                }
            }
        }

        var startMatches = startMonthYearRegex.Matches(text);
        if (startMatches.Count > 0)
        {
            // Pick the first month/year occurrence as the start.
            var m = startMatches[0];
            startIdx = m.Index;
            startMonth = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            startYear = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            if (startYear < MinInsuranceDocumentYear)
            {
                startMonth = null;
                startYear = null;
                startIdx = -1;
            }
        }

        DateTime? startDate = null;
        var startIsOnlyMonthYear = false;
        if (startMonth.HasValue && (startYear.HasValue || fallbackYear.HasValue))
        {
            var y = startYear ?? fallbackYear!.Value;
            if (y >= MinInsuranceDocumentYear)
            {
                var m = startMonth.Value;
                startDate = new DateTime(y, m, 1);
                startIsOnlyMonthYear = true;
            }
        }

        DateTime? endDate = null;

        var separatedEndMatches = endDayMonthYearSeparatedRegex.Matches(text);
        var compactEndMatches = endDayMonthYearCompactRegex.Matches(text);

        var hasStart = startIdx >= 0;
        var endCandidates = new List<DateTime>();

        void TryAddEndFromMatch(Match m)
        {
            if (hasStart && m.Index <= startIdx) return;
            if (m.Groups.Count < 4) return;

            var endDay = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var endMonth = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            var endYear = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);

            if (endDay is < 1 or > 31 || endMonth is < 1 or > 12) return;
            if (endYear < MinInsuranceDocumentYear || endYear > 2100) return;

            try
            {
                endCandidates.Add(new DateTime(endYear, endMonth, endDay));
            }
            catch (ArgumentOutOfRangeException)
            {
                // ignore invalid calendar dates from OCR
            }
        }

        foreach (Match m in separatedEndMatches) TryAddEndFromMatch(m);
        foreach (Match m in compactEndMatches) TryAddEndFromMatch(m);

        // Plusieurs dates dans le texte : prendre la plus récente comme fin probable.
        if (endCandidates.Count > 0)
        {
            endDate = endCandidates.Max();
        }

        // Début déduit en « 1er du mois » (MM/AAAA seul) + « fin » le même mois/année : ce n’est en général pas
        // une date d’expiration (ex. 01/2023 + 16/01/2023 dans l’OCR sans libellé fin de contrat).
        if (endDate is { } ed && startDate is { } sd && startIsOnlyMonthYear &&
            ed.Year == sd.Year && ed.Month == sd.Month)
        {
            endDate = null;
        }

        return (
            StartDate: startDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            EndDate: endDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        );
    }
}

