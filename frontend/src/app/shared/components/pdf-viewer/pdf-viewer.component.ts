import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxExtendedPdfViewerModule } from 'ngx-extended-pdf-viewer';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-pdf-viewer',
  standalone: true,
  imports: [CommonModule, NgxExtendedPdfViewerModule],
  templateUrl: './pdf-viewer.component.html',
  styleUrl: './pdf-viewer.component.scss',
})
export class PdfViewerComponent implements OnInit {
  @Input() interventionId!: string;

  pdfSrc: string | undefined;
  loading = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    if (this.interventionId) {
      this.loadPdf();
    }
  }

  loadPdf(): void {
    this.loading = true;
    this.api.getBlob(`/api/pdf/intervention/${this.interventionId}`).subscribe({
      next: (blob) => {
        this.pdfSrc = URL.createObjectURL(blob);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  download(): void {
    if (!this.pdfSrc) {
      return;
    }
    const link = document.createElement('a');
    link.href = this.pdfSrc;
    link.download = `intervention-${this.interventionId}.pdf`;
    link.click();
  }
}
