import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../services/api.service';

export interface Zone {
  id: number;
  name: string;
  description?: string;
}

@Component({
  selector: 'app-zones-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, DialogModule],
  templateUrl: './zones-list.component.html',
  styleUrl: './zones-list.component.scss',
})
export class ZonesListComponent implements OnInit {
  zones: Zone[] = [];
  loading = false;
  showForm = false;
  form = { name: '', description: '' };
  submitting = false;

  editDialog = false;
  editing: Zone | null = null;
  editForm = { name: '', description: '' };
  editSubmitting = false;

  deletingId: number | null = null;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadZones();
  }

  loadZones(): void {
    this.loading = true;
    this.api.get<Zone[]>('/api/admin/zones').subscribe({
      next: (data) => {
        this.zones = data;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    if (!this.showForm) this.form = { name: '', description: '' };
  }

  submitZone(): void {
    if (!this.form.name.trim() || this.submitting) return;
    this.submitting = true;
    this.api
      .post<Zone>('/api/admin/zones', {
        name: this.form.name.trim(),
        description: this.form.description?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.loadZones();
          this.toggleForm();
          this.submitting = false;
        },
        error: () => (this.submitting = false),
      });
  }

  openEdit(z: Zone): void {
    this.editing = z;
    this.editForm = { name: z.name, description: z.description ?? '' };
    this.editDialog = true;
  }

  closeEdit(): void {
    this.editDialog = false;
    this.editing = null;
  }

  saveEdit(): void {
    if (!this.editing || !this.editForm.name.trim() || this.editSubmitting) return;
    this.editSubmitting = true;
    this.api
      .put<Zone>(`/api/admin/zones/${this.editing.id}`, {
        id: this.editing.id,
        name: this.editForm.name.trim(),
        description: this.editForm.description?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.loadZones();
          this.closeEdit();
          this.editSubmitting = false;
        },
        error: () => (this.editSubmitting = false),
      });
  }

  deleteZone(z: Zone): void {
    if (!confirm(`Delete zone "${z.name}"?`)) return;
    this.deletingId = z.id;
    this.api.delete(`/api/admin/zones/${z.id}`).subscribe({
      next: () => {
        this.loadZones();
        this.deletingId = null;
      },
      error: (err) => {
        this.deletingId = null;
        const msg =
          err?.error && typeof err.error === 'string'
            ? err.error
            : 'Cannot delete (zone may be in use).';
        alert(msg);
      },
    });
  }
}
