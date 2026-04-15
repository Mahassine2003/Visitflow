import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ApiService } from '../../../services/api.service';

interface InterventionItem {
  id: number;
  title: string;
  visitKey: string;
  supplierId: number;
}

@Component({
  selector: 'app-documents-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule],
  templateUrl: './documents-upload.component.html',
  styleUrl: './documents-upload.component.scss',
})
export class DocumentsUploadComponent implements OnInit {
  documentTypes = ['Assurance', 'CIN', 'Permit', 'FirePermit', 'WorkAtHeight'];
  interventions: InterventionItem[] = [];
  selectedInterventionId: number | null = null;

  form = {
    entityType: 'INTERVENTION',
    entityId: 0,
    documentType: 'Assurance',
    filePath: '',
    startDate: '',
    endDate: '',
    isValid: true,
  };
  message = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.get<InterventionItem[]>('/api/intervention').subscribe({
      next: (items) => {
        this.interventions = items ?? [];
      },
    });
  }

  submit(): void {
    if (!this.selectedInterventionId) {
      this.message = 'Select an intervention first.';
      return;
    }

    this.form.entityId = this.selectedInterventionId;
    const body = {
      entityType: this.form.entityType,
      entityId: this.form.entityId,
      documentType: this.form.documentType,
      filePath: this.form.filePath || '/uploads/placeholder.pdf',
      startDate: this.form.startDate || null,
      endDate: this.form.endDate || null,
      isValid: this.form.isValid,
    };
    this.api.post('/api/supplier/documents', body).subscribe({
      next: () => (this.message = 'Document saved.'),
      error: () => (this.message = 'Save error.'),
    });
  }
}
