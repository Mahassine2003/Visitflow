import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../../services/api.service';
import { Zone } from '../zones-list.component';

@Component({
  selector: 'app-zone-delete-dialog',
  standalone: true,
  imports: [CommonModule, DialogModule],
  template: `
    <p-dialog
      [visible]="visible"
      [modal]="true"
      [draggable]="false"
      [resizable]="false"
      [style]="{ width: 'min(400px, 95vw)' }"
      appendTo="body"
      (onHide)="handleClose()"
    >
      <div class="delete-wrap">
        <svg class="warning-icon" viewBox="0 0 24 24" aria-hidden="true">
          <path d="M12 3l10 18H2L12 3z" />
          <path d="M12 9v5M12 17h.01" />
        </svg>

        <h2 class="dialog-title">Delete zone?</h2>
        <p class="dialog-message">
          Are you sure you want to delete "{{ zone?.name }}"?
          This action cannot be undone.
        </p>
      </div>

      <ng-template pTemplate="footer">
        <div class="dialog-actions">
          <button type="button" class="btn-cancel" [disabled]="submitting" (click)="handleClose()">
            Cancel
          </button>
          <button type="button" class="btn-delete" [disabled]="submitting || !zone" (click)="remove()">
            Delete
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
    :host ::ng-deep .p-dialog .p-dialog-footer {
      padding-left: 28px;
      padding-right: 28px;
    }

    :host ::ng-deep .p-dialog .p-dialog-content {
      padding-top: 28px;
      padding-bottom: 10px;
    }

    :host ::ng-deep .p-dialog .p-dialog-footer {
      padding-top: 8px;
      padding-bottom: 24px;
    }

    .delete-wrap {
      text-align: center;
    }

    .warning-icon {
      width: 40px;
      height: 40px;
      margin: 0 auto 12px;
      fill: none;
      stroke: #e87722;
      stroke-width: 1.8;
      stroke-linecap: round;
      stroke-linejoin: round;
    }

    .dialog-title {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: #1a1a2e;
      text-align: center;
    }

    .dialog-message {
      margin: 10px 0 0;
      font-size: 13px;
      color: #6b7280;
      text-align: center;
      line-height: 1.45;
    }

    .dialog-actions {
      display: flex;
      justify-content: center;
      gap: 8px;
    }

    .btn-cancel,
    .btn-delete {
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

    .btn-delete {
      border: none;
      background: #dc2626;
      color: #fff;
    }

    .btn-delete:disabled,
    .btn-cancel:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `],
})
export class ZoneDeleteDialogComponent {
  @Input() visible = false;
  @Input() zone: Zone | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() deleted = new EventEmitter<void>();

  submitting = false;

  constructor(private api: ApiService) {}

  handleClose(): void {
    if (this.submitting) return;
    this.closed.emit();
  }

  remove(): void {
    if (!this.zone || this.submitting) return;
    this.submitting = true;
    this.api.delete(`/api/admin/zones/${this.zone.id}`).subscribe({
      next: () => {
        this.submitting = false;
        this.deleted.emit();
      },
      error: () => {
        this.submitting = false;
      },
    });
  }
}
