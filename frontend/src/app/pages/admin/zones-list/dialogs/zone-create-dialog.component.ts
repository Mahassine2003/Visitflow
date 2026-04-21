import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-zone-create-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogModule],
  templateUrl: './zone-create-dialog.component.html',
  styleUrls: ['./zone-create-dialog.component.scss'],
})
export class ZoneCreateDialogComponent {
  @Input() visible = false;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  form = { name: '', description: '' };
  submitting = false;

  constructor(private api: ApiService) {}

  get canSave(): boolean {
    return this.form.name.trim().length >= 2 && !this.submitting;
  }

  handleClose(): void {
    if (this.submitting) return;
    this.form = { name: '', description: '' };
    this.closed.emit();
  }

  save(): void {
    if (!this.canSave) return;
    this.submitting = true;
    this.api
      .post('/api/admin/zones', {
        name: this.form.name.trim(),
        description: this.form.description.trim() || null,
      })
      .subscribe({
        next: () => {
          this.submitting = false;
          this.form = { name: '', description: '' };
          this.saved.emit();
        },
        error: () => {
          this.submitting = false;
        },
      });
  }
}
