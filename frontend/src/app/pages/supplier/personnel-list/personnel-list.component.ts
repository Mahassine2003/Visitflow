import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../core/auth/auth.service';

interface Personnel {
  id: number;
  fullName: string;
  cin: string;
  phone: string;
  fieldOfActivity: string;
  supplierId: number;
  isBlacklisted: boolean;
  typeOfWorkId?: number | null;
  typeOfWorkName?: string | null;
  address?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  createdAt?: string | null;
}

interface SupplierRow {
  id: number;
  companyName: string;
}

interface InsuranceRow {
  id: number;
  personnelId: number;
  providerName?: string | null;
  policyNumber?: string | null;
  startDate?: string | null;
  expiryDate?: string | null;
  isValid: boolean;
  filePath?: string | null;
  /** false = fichier ajouté manuellement, sans analyse IA */
  validatedByAi?: boolean;
}

interface ComplianceRequirement {
  id: number;
  type: string;
  title: string;
}

interface PersonnelDocumentRow {
  id: number;
  personnelId: number;
  documentType: string;
  filePath: string;
  isValid: boolean;
  validatedByAI: boolean;
}

@Component({
  selector: 'app-personnel-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, TagModule, ButtonModule, DialogModule],
  templateUrl: './personnel-list.component.html',
  styleUrl: './personnel-list.component.scss',
})
export class PersonnelListComponent implements OnInit {
  personnel: Personnel[] = [];
  suppliers: SupplierRow[] = [];
  loading = false;
  searchTerm = '';
  statusFilter: 'all' | 'active' | 'blacklisted' = 'all';
  page = 1;
  readonly pageSize = 10;

  detailVisible = false;
  detail: Personnel | null = null;
  detailSupplierName: string | null = null;
  detailInsurances: InsuranceRow[] = [];
  detailInsurancesLoading = false;
  detailRequiredDocs: ComplianceRequirement[] = [];
  detailRequiredDocsLoading = false;
  detailDocs: PersonnelDocumentRow[] = [];
  detailDocsLoading = false;

  constructor(
    private api: ApiService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.api.get<SupplierRow[]>('/api/supplier').subscribe({
      next: (data) => (this.suppliers = data ?? []),
      error: () => {},
    });
    this.load();
  }

  get totalPersonnelCount(): number {
    return this.personnel.length;
  }

  get blacklistedCount(): number {
    return this.personnel.filter((p) => p.isBlacklisted).length;
  }

  get activeCount(): number {
    return this.personnel.filter((p) => !p.isBlacklisted).length;
  }

  get filteredPersonnel(): Personnel[] {
    const q = this.searchTerm.trim().toLowerCase();
    return this.personnel.filter((p) => {
      if (this.statusFilter === 'blacklisted' && !p.isBlacklisted) return false;
      if (this.statusFilter === 'active' && p.isBlacklisted) return false;
      if (!q) return true;
      const supplier = this.supplierName(p.supplierId).toLowerCase();
      return (
        p.fullName.toLowerCase().includes(q) ||
        p.cin.toLowerCase().includes(q) ||
        p.fieldOfActivity.toLowerCase().includes(q) ||
        (p.typeOfWorkName ?? '').toLowerCase().includes(q) ||
        supplier.includes(q)
      );
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredPersonnel.length / this.pageSize));
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get pagedPersonnel(): Personnel[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredPersonnel.slice(start, start + this.pageSize);
  }

  get pageStartCount(): number {
    if (this.filteredPersonnel.length === 0) return 0;
    return (this.page - 1) * this.pageSize + 1;
  }

  get pageEndCount(): number {
    if (this.filteredPersonnel.length === 0) return 0;
    return Math.min(this.page * this.pageSize, this.filteredPersonnel.length);
  }

  setStatusFilter(filter: 'all' | 'active' | 'blacklisted'): void {
    this.statusFilter = filter;
    this.page = 1;
  }

  onSearchTermChange(): void {
    this.page = 1;
  }

  goTo(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page = page;
  }

  goPrev(): void {
    if (this.page > 1) this.page -= 1;
  }

  goNext(): void {
    if (this.page < this.totalPages) this.page += 1;
  }

  initials(fullName: string): string {
    const parts = (fullName || '').trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return 'P';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return `${parts[0][0] ?? ''}${parts[1][0] ?? ''}`.toUpperCase();
  }

  avatarTone(id: number): 'tone-blue' | 'tone-teal' | 'tone-purple' {
    const i = Math.abs(id) % 3;
    if (i === 0) return 'tone-blue';
    if (i === 1) return 'tone-teal';
    return 'tone-purple';
  }

  supplierName(supplierId: number): string {
    return this.suppliers.find((s) => s.id === supplierId)?.companyName ?? `— (#${supplierId})`;
  }

  load(): void {
    this.loading = true;
    this.api.get<Record<string, unknown>[]>('/api/personnel').subscribe({
      next: (data) => {
        const rows = data ?? [];
        this.personnel = rows.map((p) => {
          const tw = p['typeOfWork'] as { name?: string } | undefined;
          return {
            id: p['id'] as number,
            fullName: (p['fullName'] as string) ?? '',
            cin: (p['cin'] as string) ?? '',
            phone: (p['phone'] as string) ?? '',
            fieldOfActivity:
              (p['fieldOfActivity'] as string) ??
              (p['position'] as string) ??
              '—',
            supplierId: p['supplierId'] as number,
            isBlacklisted: !!(p['isBlacklisted'] as boolean),
            typeOfWorkId: (p['typeOfWorkId'] as number | null) ?? null,
            typeOfWorkName: (p['typeOfWorkName'] as string) ?? tw?.name ?? null,
            address: (p['address'] as string) ?? null,
            startDate: (p['startDate'] as string) ?? null,
            endDate: (p['endDate'] as string) ?? null,
            createdAt: (p['createdAt'] as string) ?? null,
          };
        });
        if (this.page > this.totalPages) this.page = this.totalPages;
      },
      error: () => {},
      complete: () => (this.loading = false),
    });
  }

  openDetail(p: Personnel): void {
    this.detail = p;
    this.detailSupplierName = this.supplierName(p.supplierId);
    this.detailVisible = true;
    this.loadInsurances(p.id);
    this.loadRequiredDocs(p.typeOfWorkId ?? null);
    this.loadDocs(p.id);
  }

  closeDetail(): void {
    this.detailVisible = false;
    this.detail = null;
    this.detailSupplierName = null;
    this.detailInsurances = [];
    this.detailRequiredDocs = [];
    this.detailDocs = [];
  }

  private loadInsurances(personnelId: number): void {
    this.detailInsurancesLoading = true;
    this.api.get<InsuranceRow[]>(`/api/insurance?personnelId=${personnelId}`).subscribe({
      next: (data) => {
        this.detailInsurances = data ?? [];
        this.detailInsurancesLoading = false;
      },
      error: () => (this.detailInsurancesLoading = false),
    });
  }

  openInsurance(i: InsuranceRow): void {
    if (!i?.id) return;
    this.api.getBlob(`/api/insurance/${i.id}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank', 'noopener,noreferrer');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: () => alert("Impossible d’ouvrir l’assurance."),
    });
  }

  private loadRequiredDocs(typeOfWorkId: number | null): void {
    if (!typeOfWorkId) {
      this.detailRequiredDocs = [];
      return;
    }
    this.detailRequiredDocsLoading = true;
    this.api.get<ComplianceRequirement[]>(`/api/type-of-work/${typeOfWorkId}/requirements`).subscribe({
      next: (data) => {
        const all = data ?? [];
        this.detailRequiredDocs = all.filter((x) => (x.type ?? '').toLowerCase() === 'document');
        this.detailRequiredDocsLoading = false;
      },
      error: () => (this.detailRequiredDocsLoading = false),
    });
  }

  private loadDocs(personnelId: number): void {
    this.detailDocsLoading = true;
    this.api.get<PersonnelDocumentRow[]>(`/api/personnel/${personnelId}/documents`).subscribe({
      next: (data) => {
        this.detailDocs = data ?? [];
        this.detailDocsLoading = false;
      },
      error: () => (this.detailDocsLoading = false),
    });
  }

  /** Libellé affiché à la place du chemin Windows complet (comme pour l’assurance). */
  fileBasename(path: string | null | undefined): string {
    if (!path?.trim()) return '';
    const normalized = path.replace(/\\/g, '/');
    const seg = normalized.split('/').filter(Boolean);
    return seg.length ? seg[seg.length - 1]! : path;
  }

  private guessMimeFromPath(path: string | null | undefined): string {
    const ext = (path ?? '').split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf':
        return 'application/pdf';
      case 'png':
        return 'image/png';
      case 'jpg':
      case 'jpeg':
        return 'image/jpeg';
      default:
        return '';
    }
  }

  openPersonnelDoc(d: PersonnelDocumentRow): void {
    if (!d?.id) return;
    this.api.getBlob(`/api/personnel/documents/${d.id}/download`).subscribe({
      next: (blob) => {
        const mime = blob.type || this.guessMimeFromPath(d.filePath);
        const typed = mime ? new Blob([blob], { type: mime }) : blob;
        const url = URL.createObjectURL(typed);
        window.open(url, '_blank', 'noopener,noreferrer');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: () => alert("Impossible d’ouvrir le document."),
    });
  }

  addPersonnel(): void {
    if (this.auth.hasAnyRole(['ADMIN'])) {
      this.router.navigate(['/app/personnel/new']);
    } else {
      this.router.navigate(['/app/personnel/create']);
    }
  }

  /** Affiche le bouton d’ajout pour ADMIN et USER (pas pour RH). */
  canAddPersonnel(): boolean {
    return this.auth.hasAnyRole(['ADMIN', 'USER']);
  }

  canManage(): boolean {
    return this.auth.hasAnyRole(['ADMIN', 'RH']);
  }

  deletePersonnel(p: Personnel): void {
    if (!confirm(`Delete ${p.fullName}?`)) return;
    this.api.delete(`/api/personnel/${p.id}`).subscribe({ next: () => this.load() });
  }

  toggleBlacklist(p: Personnel): void {
    this.api.post(`/api/personnel/${p.id}/blacklist?isBlacklisted=${!p.isBlacklisted}`, {}).subscribe({
      next: () => this.load(),
    });
  }
}
