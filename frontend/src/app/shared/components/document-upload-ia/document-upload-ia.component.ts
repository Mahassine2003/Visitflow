import { Component, EventEmitter, Input, OnDestroy, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';

export type DocumentUploadSource = 'ai' | 'manual';

export interface DocumentIaResult {
  validatedByAI: boolean;
  isValid: boolean;
  aiSkipped?: boolean;
  skipReason?: string | null;
  year?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  rawText?: string | null;
}

@Component({
  selector: 'app-document-upload-ia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './document-upload-ia.component.html',
  styleUrl: './document-upload-ia.component.scss',
})
export class DocumentUploadIaComponent implements OnDestroy {
  @Input() label = 'Document';
  /** Identifiant unique pour le groupe de boutons radio (plusieurs composants sur la même page). */
  @Input() fieldKey = '';
  @Input() showSourceChoice = true;
  @Output() resultChange = new EventEmitter<DocumentIaResult | null>();
  @Output() fileChange = new EventEmitter<File | null>();

  source: DocumentUploadSource = 'ai';
  file: File | null = null;
  previewUrl: string | null = null;
  viewUrl: string | null = null;
  fileType: 'image' | 'pdf' | 'word' | null = null;
  loading = false;
  result: DocumentIaResult | null = null;
  manualStartDate = '';
  manualEndDate = '';

  constructor(private api: ApiService) {}

  get radioGroupName(): string {
    const k = (this.fieldKey || this.label || 'doc').replace(/\s+/g, '-');
    return `doc-upload-src-${k}`;
  }

  ngOnDestroy(): void {
    this.revokeObjectUrl();
  }

  setSource(next: DocumentUploadSource): void {
    if (this.source === next) return;
    this.source = next;
    this.resetFileState();
  }

  private resetFileState(): void {
    this.result = null;
    this.resultChange.emit(null);
    this.file = null;
    this.fileChange.emit(null);
    this.previewUrl = null;
    this.revokeObjectUrl();
    this.fileType = null;
    this.manualStartDate = '';
    this.manualEndDate = '';
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.result = null;
    this.resultChange.emit(null);
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

    if (this.source === 'manual') {
      this.emitManualResult();
    }
  }

  clearFile(): void {
    this.resetFileState();
  }

  onManualDatesChanged(): void {
    if (this.source !== 'manual' || !this.file) return;
    this.emitManualResult();
  }

  openFile(): void {
    if (!this.viewUrl) return;
    window.open(this.viewUrl, '_blank', 'noopener,noreferrer');
  }

  detect(): void {
    if (!this.file || this.loading || this.source !== 'ai') return;

    const formData = new FormData();
    formData.append('file', this.file);

    this.loading = true;
    this.api.post<DocumentIaResult>('/api/ai/validate-document', formData).subscribe({
      next: (res) => {
        this.result = res;
        this.resultChange.emit(res);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
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

  private emitManualResult(): void {
    this.result = {
      validatedByAI: false,
      isValid: true,
      startDate: this.manualStartDate || null,
      endDate: this.manualEndDate || null,
    };
    this.resultChange.emit(this.result);
  }
}
