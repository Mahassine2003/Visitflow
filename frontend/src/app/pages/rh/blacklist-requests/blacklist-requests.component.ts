import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Router } from '@angular/router';

export interface BlacklistRequestRow {
  id: number;
  personnelId: number;
  reason: string;
  status: string;
  requestedBy: string;
  createdAt: string;
  personnel?: { fullName: string };
  reviewedByUser?: { fullName: string };
}

@Component({
  selector: 'app-blacklist-requests',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, TagModule, ButtonModule],
  templateUrl: './blacklist-requests.component.html',
})
export class BlacklistRequestsComponent implements OnInit {
  /** Masque le titre principal (ex. intégré dans le dashboard admin). */
  @Input() embedded = false;

  requests: BlacklistRequestRow[] = [];
  loading = false;

  /** IDs de demandes pour lesquelles la liste a déjà été générée (côté navigateur). */
  generatedOnce = new Set<number>();

  personnelOptions: { id: number; fullName: string }[] = [];
  rhOptions: { id: number; fullName: string }[] = [];

  newPersonnelId: number | null = null;
  newReviewerUserId: number | null = null;
  newReason = '';

  constructor(
    private api: ApiService,
    public auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Charge toujours la liste :
    // - pour un USER : historique de ses propres demandes
    // - pour un RH/ADMIN : demandes à traiter (logique côté API)
    this.load();

    if (this.isRequester()) {
      this.loadPersonnelOptions();
      this.loadRhOptions();
    }

    const raw = localStorage.getItem('blacklistGeneratedOnce');
    if (raw) {
      try {
        const ids: number[] = JSON.parse(raw);
        this.generatedOnce = new Set(ids);
      } catch {
        this.generatedOnce = new Set<number>();
      }
    }
  }

  loadPersonnelOptions(): void {
    this.api.get<any[]>('/api/personnel').subscribe({
      next: (data) => {
        const rows = data ?? [];
        this.personnelOptions = rows
          .map((p) => ({
            id: p['id'] as number,
            fullName: (p['fullName'] as string) ?? '',
          }))
          .filter((x) => x.id != null && x.fullName);

        // Pour simplifier le formulaire côté utilisateur,
        // on sélectionne automatiquement le premier personnel disponible.
        if (!this.newPersonnelId && this.personnelOptions.length > 0) {
          this.newPersonnelId = this.personnelOptions[0].id;
        }
      },
      error: () => {},
    });
  }

  loadRhOptions(): void {
    this.api.get<{ id: number; fullName: string }[]>('/api/users/rh').subscribe({
      next: (data) => (this.rhOptions = data ?? []),
      error: () => {},
    });
  }

  load(): void {
    this.loading = true;
    this.api.get<BlacklistRequestRow[]>('/api/blacklist-requests').subscribe({
      next: (data) => (this.requests = data),
      error: () => {},
      complete: () => (this.loading = false),
    });
  }

  submitUserRequest(): void {
    if (!this.newPersonnelId || !this.newReviewerUserId || !this.newReason.trim())
      return;
    this.api
      .post('/api/blacklist-requests', {
        personnelId: this.newPersonnelId,
        reviewerUserId: this.newReviewerUserId,
        reason: this.newReason.trim(),
      })
      .subscribe({
        next: () => {
          this.newReason = '';
          this.newPersonnelId = null;
          this.newReviewerUserId = null;
          this.load();
        },
      });
  }

  review(id: number, approve: boolean): void {
    this.api.post(`/api/blacklist-requests/${id}/review?approve=${approve}`, {}).subscribe({
      next: () => this.load(),
    });
  }

  isReviewer(): boolean {
    return this.auth.hasAnyRole(['RH', 'ADMIN']);
  }

  isRequester(): boolean {
    return this.auth.hasAnyRole(['USER']);
  }

  /** Ouvre la page liste du personnel. */
  goToPersonnelList(): void {
    this.router.navigate(['/app/personnel']);
  }

  canGenerateFor(r: BlacklistRequestRow): boolean {
    return r.status === 'Approved' && !this.generatedOnce.has(r.id);
  }

  generateList(r: BlacklistRequestRow): void {
    if (!this.canGenerateFor(r)) return;
    this.api.getBlob('/api/pdf/blacklist').subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        this.generatedOnce.add(r.id);
        localStorage.setItem(
          'blacklistGeneratedOnce',
          JSON.stringify(Array.from(this.generatedOnce.values()))
        );
      },
      error: () => {},
    });
  }
}
