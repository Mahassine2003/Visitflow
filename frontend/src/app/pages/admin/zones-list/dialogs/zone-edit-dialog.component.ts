import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../../services/api.service';
import { Zone } from '../zones-list.component';

@Component({
  selector: 'app-zone-edit-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogModule],
  templateUrl: './zone-edit-dialog.component.html',
  styleUrls: ['./zone-edit-dialog.component.scss'],
})
export class ZoneEditDialogComponent implements OnChanges {
  @Input() visible = false;
  @Input() zone: Zone | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  form = { name: '', description: '' };
  submitting = false;

  constructor(private api: ApiService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['zone']) {
      this.form = {
        name: this.zone?.name ?? '',
        description: this.zone?.description ?? '',
      };
    }
  }

  get canSave(): boolean {
    return !!this.zone && this.form.name.trim().length >= 2 && !this.submitting;
  }

  handleClose(): void {
    if (this.submitting) return;
    this.closed.emit();
  }

  save(): void {
    if (!this.zone || !this.canSave) return;
    this.submitting = true;
    this.api
      .put(`/api/admin/zones/${this.zone.id}`, {
        id: this.zone.id,
        name: this.form.name.trim(),
        description: this.form.description.trim() || null,
      })
      .subscribe({
        next: () => {
          this.submitting = false;
          this.saved.emit();
        },
        error: () => {
          this.submitting = false;
        },
      });
  }
}
