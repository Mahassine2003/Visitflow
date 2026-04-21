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
  template: `
    <p-dialog
      [visible]="visible"
      [modal]="true"
      [draggable]="false"
      [resizable]="false"
      [style]="{ width: 'min(440px, 95vw)' }"
      appendTo="body"
      (onHide)="handleClose()"
    >
      <ng-template pTemplate="header">
        <h2 class="dialog-title">Edit zone</h2>
      </ng-template>

      <div class="dialog-body">
        <label class="field-label">Name</label>
        <input
          class="field-input"
          [(ngModel)]="form.name"
          [disabled]="submitting"
          placeholder="Zone name"
        />

        <label class="field-label">Description</label>
        <textarea
          class="field-input field-textarea"
          [(ngModel)]="form.description"
          [disabled]="submitting"
          rows="3"
          placeholder="Optional description"
        ></textarea>
      </div>

      <ng-template pTemplate="footer">
        <div class="dialog-actions">
          <button type="button" class="btn-cancel" [disabled]="submitting" (click)="handleClose()">
            Cancel
          </button>
          <button
            type="button"
            class="btn-save"
            [disabled]="!canSave"
            (click)="save()"
          >
            Save
          </button>
        </div>
      </ng-template>
    </p-dialog>
  `,
  styles: [`
    :host ::ng-deep .p-dialog {
      border-radius: 14px;
      overflow: hidden;
    }

    :host ::ng-deep .p-dialog .p-dialog-content,
    :host ::ng-deep .p-dialog .p-dialog-header,
    :host ::ng-deep .p-dialog .p-dialog-footer {
      padding-left: 28px;
      padding-right: 28px;
    }

    :host ::ng-deep .p-dialog .p-dialog-header {
      padding-top: 24px;
      padding-bottom: 8px;
    }

    :host ::ng-deep .p-dialog .p-dialog-content {
      padding-top: 6px;
      padding-bottom: 8px;
    }

    :host ::ng-deep .p-dialog .p-dialog-footer {
      padding-top: 6px;
      padding-bottom: 24px;
    }

    .dialog-title {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: #1a1a2e;
    }

    .dialog-body {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .field-label {
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-top: 6px;
    }

    .field-input {
      width: 100%;
      border: 0.5px solid #e5e7eb;
      border-radius: 8px;
      padding: 10px 12px;
      font-size: 13px;
      color: #1a1a2e;
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
      font-family: inherit;
    }

    .field-input:focus {
      border-color: #e87722;
      box-shadow: 0 0 0 3px rgba(232, 119, 34, 0.15);
    }

    .field-textarea {
      resize: vertical;
      min-height: 80px;
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }

    .btn-cancel,
    .btn-save {
      border-radius: 8px;
      padding: 9px 16px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
    }

    .btn-cancel {
      border: 0.5px solid #e5e7eb;
      background: transparent;
      color: #4b5563;
    }

    .btn-save {
      border: none;
      background: #e87722;
      color: #fff;
    }

    .btn-save:disabled,
    .btn-cancel:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `],
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
    return !!this.zone && this.form.name.trim().length > 0 && !this.submitting;
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
