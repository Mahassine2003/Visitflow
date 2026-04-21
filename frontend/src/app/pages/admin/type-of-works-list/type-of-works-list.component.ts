import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, Observable, of } from 'rxjs';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../services/api.service';

export interface TypeOfWork {
  id: number;
  name: string;
  description: string;
  requiresInsurance: boolean;
  requiresTraining: boolean;
}

export interface ComplianceRequirement {
  id: number;
  type: string;
  title: string;
}

@Component({
  selector: 'app-type-of-works-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, DialogModule],
  templateUrl: './type-of-works-list.component.html',
  styleUrl: './type-of-works-list.component.scss',
})
export class TypeOfWorksListComponent implements OnInit {
  typeOfWorks: TypeOfWork[] = [];
  page = 1;
  readonly pageSize = 8;
  documentRequirementCount = 0;
  loading = false;
  showForm = false;
  /** Case Training : affiche la zone pour ajouter des libellés de pièces (type API Document uniquement). */
  form = { name: '', description: '', requiresTraining: false };
  /** Libellés ajoutés à la création (POST Document après création du type). */
  pendingDocumentRequirements: { title: string }[] = [];
  newDocumentReqTitle = '';
  submitting = false;

  editDialog = false;
  editing: TypeOfWork | null = null;
  editForm = { name: '', description: '', requiresInsurance: true };
  editSubmitting = false;

  deletingId: number | null = null;

  reqDialogVisible = false;
  selectedTow: TypeOfWork | null = null;
  requirements: ComplianceRequirement[] = [];
  reqLoading = false;
  reqForm = { title: '' };
  reqSubmitting = false;
  reqTouched = false;

  /** Toutes les exigences (Training, Document, …) pour le dialogue — tri affichage. */
  get sortedRequirements(): ComplianceRequirement[] {
    return [...(this.requirements ?? [])].sort(
      (a, b) =>
        (a.type ?? '').localeCompare(b.type ?? '', undefined, { sensitivity: 'base' }) ||
        (a.title ?? '').localeCompare(b.title ?? '', undefined, { sensitivity: 'base' }),
    );
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.typeOfWorks.length / this.pageSize));
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get pagedTypeOfWorks(): TypeOfWork[] {
    const start = (this.page - 1) * this.pageSize;
    return this.typeOfWorks.slice(start, start + this.pageSize);
  }

  get insuredCoveragePercent(): number {
    if (this.typeOfWorks.length === 0) return 0;
    const insured = this.typeOfWorks.filter((x) => x.requiresInsurance).length;
    return Math.round((insured / this.typeOfWorks.length) * 100);
  }

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.get<TypeOfWork[]>('/api/admin/type-of-works').subscribe({
      next: (data) => {
        this.typeOfWorks = data ?? [];
        if (this.page > this.totalPages) this.page = this.totalPages;
        this.loadDocumentMetric();
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  private loadDocumentMetric(): void {
    const rows = this.typeOfWorks ?? [];
    if (rows.length === 0) {
      this.documentRequirementCount = 0;
      return;
    }
    const calls: Observable<ComplianceRequirement[]>[] = rows.map((row) =>
      this.api
        .get<ComplianceRequirement[]>(`/api/type-of-work/${row.id}/requirements`)
        .pipe(catchError(() => of([]))),
    );
    forkJoin(calls).subscribe({
      next: (all) => {
        this.documentRequirementCount = all.reduce((acc, reqs) => acc + (reqs?.length ?? 0), 0);
      },
      error: () => {
        this.documentRequirementCount = rows.filter((x) => x.requiresTraining).length;
      },
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    if (!this.showForm) this.resetCreateForm();
  }

  goTo(p: number): void {
    if (p >= 1 && p <= this.totalPages) this.page = p;
  }

  goFirst(): void {
    this.page = 1;
  }

  goLast(): void {
    this.page = this.totalPages;
  }

  goPrev(): void {
    if (this.page > 1) this.page -= 1;
  }

  goNext(): void {
    if (this.page < this.totalPages) this.page += 1;
  }

  private resetCreateForm(): void {
    this.form = { name: '', description: '', requiresTraining: false };
    this.pendingDocumentRequirements = [];
    this.newDocumentReqTitle = '';
  }

  onRequiresTrainingChange(checked: boolean): void {
    if (!checked) {
      this.pendingDocumentRequirements = [];
      this.newDocumentReqTitle = '';
    }
  }

  addPendingDocumentRequirement(): void {
    const t = this.newDocumentReqTitle.trim();
    if (!t) return;
    this.pendingDocumentRequirements = [...this.pendingDocumentRequirements, { title: t }];
    this.newDocumentReqTitle = '';
  }

  removePendingDocumentRequirement(index: number): void {
    this.pendingDocumentRequirements = this.pendingDocumentRequirements.filter((_, i) => i !== index);
  }

  /** Réponse POST peut exposer `id` ou `Id` selon la config JSON. */
  private parseCreatedTypeOfWorkId(created: unknown): number | null {
    if (created == null || typeof created !== 'object') return null;
    const o = created as Record<string, unknown>;
    const raw = o['id'] ?? o['Id'];
    const n = Number(raw);
    return Number.isFinite(n) && n > 0 ? n : null;
  }

  submitForm(): void {
    if (!this.form.name.trim() || this.submitting) return;
    this.submitting = true;
    this.api
      .post<TypeOfWork>('/api/admin/type-of-works', {
        name: this.form.name.trim(),
        description: this.form.description?.trim() ?? '',
        requiresInsurance: true,
        requiresTraining: this.form.requiresTraining,
        trainingTitle: null,
      })
      .subscribe({
        next: (created) => {
          const id = this.parseCreatedTypeOfWorkId(created);
          const pendingDocs = this.form.requiresTraining ? this.pendingDocumentRequirements : [];
          if (pendingDocs.length > 0 && id == null) {
            alert(
              'The work type was created, but the new id could not be read. Add requirements with the Requirements button.',
            );
            this.afterCreateSuccess();
            return;
          }
          if (id != null && pendingDocs.length > 0) {
            const calls: Observable<ComplianceRequirement | null>[] = [];
            for (const row of pendingDocs) {
              calls.push(
                this.api
                  .post<ComplianceRequirement>(`/api/admin/type-of-works/${id}/requirements`, {
                    type: 'Document',
                    title: row.title,
                  })
                  .pipe(
                    catchError((err) => {
                      console.error('Document requirement create failed', err);
                      return of(null);
                    }),
                  ),
              );
            }
            forkJoin(calls).subscribe({
              complete: () => this.afterCreateSuccess(),
            });
          } else {
            this.afterCreateSuccess();
          }
        },
        error: () => (this.submitting = false),
      });
  }

  private afterCreateSuccess(): void {
    this.load();
    this.showForm = false;
    this.resetCreateForm();
    this.submitting = false;
  }

  openEdit(t: TypeOfWork): void {
    this.editing = t;
    this.editForm = {
      name: t.name,
      description: t.description ?? '',
      requiresInsurance: t.requiresInsurance,
    };
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
      .put<TypeOfWork>(`/api/admin/type-of-works/${this.editing.id}`, {
        id: this.editing.id,
        name: this.editForm.name.trim(),
        description: this.editForm.description?.trim() ?? '',
        requiresInsurance: this.editForm.requiresInsurance,
      })
      .subscribe({
        next: () => {
          this.load();
          this.closeEdit();
          this.editSubmitting = false;
        },
        error: () => (this.editSubmitting = false),
      });
  }

  deleteType(t: TypeOfWork): void {
    if (!confirm(`Delete work type "${t.name}"?`)) return;
    this.deletingId = t.id;
    this.api.delete(`/api/admin/type-of-works/${t.id}`).subscribe({
      next: () => {
        this.load();
        this.deletingId = null;
      },
      error: (err) => {
        this.deletingId = null;
        const msg =
          err?.error && typeof err.error === 'string'
            ? err.error
            : 'Cannot delete (work type is used by interventions or personal records).';
        alert(msg);
      },
    });
  }

  openRequirements(t: TypeOfWork): void {
    this.selectedTow = t;
    this.reqDialogVisible = true;
    this.reqForm = { title: '' };
    this.loadRequirements(t.id);
  }

  closeReqDialog(): void {
    this.reqDialogVisible = false;
    this.selectedTow = null;
    this.requirements = [];
    this.reqTouched = false;
  }

  loadRequirements(typeOfWorkId: number): void {
    this.reqLoading = true;
    this.api.get<ComplianceRequirement[]>(`/api/type-of-work/${typeOfWorkId}/requirements`).subscribe({
      next: (data) => {
        this.requirements = (data ?? []).map((raw) => this.normalizeRequirementRow(raw));
        this.reqLoading = false;
      },
      error: () => (this.reqLoading = false),
    });
  }

  private normalizeRequirementRow(raw: unknown): ComplianceRequirement {
    const o = raw && typeof raw === 'object' ? (raw as Record<string, unknown>) : {};
    const id = Number(o['id'] ?? o['Id']);
    return {
      id: Number.isFinite(id) ? id : 0,
      type: String(o['type'] ?? o['Type'] ?? ''),
      title: String(o['title'] ?? o['Title'] ?? ''),
    };
  }

  addRequirement(): void {
    this.reqTouched = true;
    if (!this.selectedTow || !this.reqForm.title.trim() || this.reqSubmitting) return;
    this.reqSubmitting = true;
    this.api
      .post<ComplianceRequirement>(`/api/admin/type-of-works/${this.selectedTow.id}/requirements`, {
        type: 'Document',
        title: this.reqForm.title.trim(),
      })
      .subscribe({
        next: () => {
          this.reqForm.title = '';
          this.reqTouched = false;
          this.loadRequirements(this.selectedTow!.id);
          this.reqSubmitting = false;
        },
        error: () => (this.reqSubmitting = false),
      });
  }

  deleteRequirement(r: ComplianceRequirement): void {
    if (!confirm(`Delete "${r.title}"?`)) return;
    this.api.delete(`/api/admin/type-of-works/requirements/${r.id}`).subscribe({
      next: () => this.selectedTow && this.loadRequirements(this.selectedTow.id),
    });
  }
}
