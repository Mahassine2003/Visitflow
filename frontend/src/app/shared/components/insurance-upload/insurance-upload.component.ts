import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
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
export class InsuranceUploadComponent implements OnDestroy {
  /** Libellé affiché au-dessus du champ fichier (optionnel ; le redesign masque si vide). */
  @Input() label = 'File';
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

  isDragOver = false;

  constructor(private api: ApiService) {}

  ngOnDestroy(): void {
    this.revokeObjectUrl();
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
    this.previewUrl = null;
    this.revokeObjectUrl();
    this.fileType = null;

    if (!file) {
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
}
