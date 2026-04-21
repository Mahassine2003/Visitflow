import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { animate, style, transition, trigger } from '@angular/animations';
import { ApiService } from '../../../services/api.service';

interface InsuranceResult {
  isValid: boolean;
  status?: string | null;
  year: number | null;
  startDate?: string | null;
  endDate?: string | null;
  rawText?: string | null;
}

const MAX_INSURANCE_BYTES = 10 * 1024 * 1024;

function isAllowedInsuranceFile(file: File): boolean {
  const name = (file.name || '').toLowerCase();
  const type = (file.type || '').toLowerCase();
  const extOk = /\.(pdf|jpe?g|png)$/i.test(name);
  const typeOk =
    type === 'application/pdf' ||
    type === 'image/jpeg' ||
    type === 'image/jpg' ||
    type === 'image/png' ||
    type === 'image/pjpeg';
  return extOk || typeOk;
}

@Component({
  selector: 'app-insurance-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './insurance-upload.component.html',
  styleUrl: './insurance-upload.component.scss',
  animations: [
    trigger('fileAppear', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate(
          '280ms ease',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
      transition(':leave', [
        animate('180ms ease', style({ opacity: 0 })),
      ]),
    ]),
  ],
})
export class InsuranceUploadComponent implements OnDestroy, OnChanges {
  /** Libellé affiché au-dessus du champ fichier (optionnel ; le redesign masque si vide). */
  @Input() label = 'File';
  @Input() persistedFile: File | null = null;
  @Input() persistedResult: InsuranceResult | null = null;
  @Output() resultChange = new EventEmitter<InsuranceResult>();
  @Output() fileChange = new EventEmitter<File | null>();

  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;

  file: File | null = null;
  previewUrl: string | null = null;
  viewUrl: string | null = null;
  fileType: 'image' | 'pdf' | 'word' | null = null;
  loading = false;
  result: InsuranceResult | null = null;
  /** Message d'erreur réseau / API (affiché si l'analyse échoue). */
  detectError: string | null = null;
  /** Fichier refusé (format / taille) avant envoi à l’IA. */
  fileRuleError: string | null = null;

  isDragOver = false;

  constructor(private api: ApiService) {}

  ngOnDestroy(): void {
    this.revokeObjectUrl();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('persistedFile' in changes) {
      const incoming = this.persistedFile;
      if (!incoming) {
        if (this.file) this.applyFile(null);
      } else if (!this.file || !this.isSameFile(this.file, incoming)) {
        this.applyFile(incoming);
      }
    }

    if ('persistedResult' in changes) {
      if (this.persistedResult) {
        this.result = this.persistedResult;
      } else if (!this.loading) {
        this.result = null;
      }
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0];
    this.applyFile(f ?? null);
  }

  /** Alimente le composant avec un fichier (sélection, drop) sans modifier le flux API. */
  applyFile(file: File | null): void {
    this.result = null;
    this.detectError = null;
    this.fileRuleError = null;
    this.previewUrl = null;
    this.revokeObjectUrl();
    this.fileType = null;

    if (!file) {
      this.file = null;
      this.fileChange.emit(null);
      return;
    }

    if (file.size > MAX_INSURANCE_BYTES) {
      this.fileRuleError = `Fichier trop volumineux (max. 10 Mo). Taille actuelle : ${this.formatSize(file.size)}.`;
      this.file = null;
      this.fileChange.emit(null);
      return;
    }
    if (!isAllowedInsuranceFile(file)) {
      this.fileRuleError =
        'Format non accepté. Utilisez uniquement PDF, JPG ou PNG (comme indiqué : « PDF, JPG, PNG · Max 10 Mo »).';
      this.file = null;
      this.fileChange.emit(null);
      return;
    }

    this.file = file;
    this.fileChange.emit(file);
    this.viewUrl = URL.createObjectURL(file);
    const name = file.name.toLowerCase();

    if (file.type.startsWith('image/')) {
      this.fileType = 'image';
      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    } else if (file.type === 'application/pdf' || name.endsWith('.pdf')) {
      this.fileType = 'pdf';
    } else if (
      name.endsWith('.docx') ||
      name.endsWith('.doc') ||
      file.type === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' ||
      file.type === 'application/msword'
    ) {
      this.fileType = 'word';
    }
  }

  clearFile(): void {
    this.applyFile(null);
    if (this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.value = '';
    }
  }

  onDragOver(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.isDragOver = false;
    const f = ev.dataTransfer?.files?.[0];
    if (f) {
      this.applyFile(f);
    }
  }

  openFile(): void {
    if (!this.viewUrl) return;
    window.open(this.viewUrl, '_blank', 'noopener,noreferrer');
  }

  detect(): void {
    if (!this.file || this.loading) {
      return;
    }

    const formData = new FormData();
    formData.append('file', this.file);

    this.loading = true;
    this.detectError = null;
    this.api
      .post<InsuranceResult>('/api/ai/validate-insurance', formData)
      .subscribe({
        next: (res) => {
          this.result = res;
          this.resultChange.emit(res);
          this.loading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.loading = false;
          const msg =
            (typeof err.error === 'object' && err.error && 'message' in err.error
              ? String((err.error as { message?: string }).message)
              : null) ||
            err.statusText ||
            'Error during analysis.';
          this.detectError =
            err.status === 0
              ? 'Cannot reach the API. Check your connection and that the backend is running.'
              : err.status >= 500
                ? `Server error (${err.status}). Ensure the Python AI microservice is running (port 8000) and appsettings.json matches.`
                : msg;
        },
      });
  }

  private revokeObjectUrl(): void {
    if (this.viewUrl) {
      URL.revokeObjectURL(this.viewUrl);
      this.viewUrl = null;
    }
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  /** Texte court après analyse : police aux dates valides ou non. */
  validitySummary(result: InsuranceResult | null): string {
    if (!result) return '';
    if (result.isValid) {
      return 'Valide : les dates détectées indiquent une assurance encore en vigueur (date de fin ≥ aujourd’hui, ou règle équivalente du document).';
    }
    if ((result.status || '').toUpperCase() === 'UNKNOWN') {
      return 'Invalide : impossible d’extraire des dates de police de façon fiable. Vérifiez la qualité du scan ou utilisez l’envoi manuel.';
    }
    return 'Invalide : la date de fin de couverture est dépassée ou les dates ne permettent pas de valider la police.';
  }

  private isSameFile(a: File | null, b: File | null): boolean {
    if (!a || !b) return false;
    return (
      a.name === b.name &&
      a.size === b.size &&
      a.type === b.type &&
      a.lastModified === b.lastModified
    );
  }
}
