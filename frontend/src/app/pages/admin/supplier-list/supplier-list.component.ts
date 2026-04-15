import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import {
  animate,
  style,
  transition,
  trigger,
} from '@angular/animations';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../core/auth/auth.service';
import { InsuranceUploadComponent } from '../../../shared/components/insurance-upload/insurance-upload.component';

export interface Supplier {
  id: number;
  companyName: string;
  /** ICE (identifiant commun de l’entreprise), champ API `ice`. */
  ice?: string;
  email: string;
  phone?: string;
  address?: string;
  status?: string;
}

export interface SupplierPersonnel {
  id: number;
  fullName: string;
  cin: string;
  phone: string;
  supplierId: number;
  fieldOfActivity: string;
  isBlacklisted: boolean;
}

interface SupplierCompanyDocument {
  id: number;
  supplierId: number;
  documentType: string;
  filePath: string;
  fileType: string;
  uploadedAt: string;
}

interface InsuranceResult {
  isValid: boolean;
  status?: string | null;
  year: number | null;
  startDate?: string | null;
  endDate?: string | null;
  rawText?: string | null;
}

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    TagModule,
    ButtonModule,
    DialogModule,
    ReactiveFormsModule,
    InsuranceUploadComponent,
  ],
  templateUrl: './supplier-list.component.html',
  styleUrl: './supplier-list.component.scss',
  animations: [
    trigger('backdropAnim', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease', style({ opacity: 1 })),
      ]),
      transition(':leave', [
        animate('150ms ease', style({ opacity: 0 })),
      ]),
    ]),
    trigger('modalAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.92) translateY(20px)' }),
        animate(
          '300ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' }),
        ),
      ]),
      transition(':leave', [
        animate('200ms ease', style({ opacity: 0, transform: 'scale(0.95)' })),
      ]),
    ]),
    trigger('deleteModalAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.88) translateY(24px)' }),
        animate(
          '300ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' }),
        ),
      ]),
      transition(':leave', [
        animate('200ms ease', style({ opacity: 0, transform: 'scale(0.92)' })),
      ]),
    ]),
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(16px)' }),
        animate('350ms ease', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class SupplierListComponent implements OnInit {
  suppliers: Supplier[] = [];
  loading = false;

  showCreate = false;
  createLoading = false;
  createError: string | null = null;
  insuranceResult: InsuranceResult | null = null;
  form = this.fb.group({
    companyName: ['', Validators.required],
    ice: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    address: [''],
  });

  editVisible = false;
  editLoading = false;
  editError: string | null = null;
  editingSupplierId: number | null = null;
  editForm = this.fb.group({
    companyName: ['', Validators.required],
    ice: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    address: [''],
  });

  selectedId: number | null = null;
  supplier: Supplier | null = null;
  detailLoading = false;
  personnel: SupplierPersonnel[] = [];
  personnelLoading = false;

  companyDocs: SupplierCompanyDocument[] = [];
  companyDocsLoading = false;
  filterText = '';
  showDeleteModal = false;
  selectedSupplierToDelete: Supplier | null = null;
  isDeleting = false;

  constructor(
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadSuppliers();
    this.route.queryParamMap.subscribe((params) => {
      const isNew = params.get('new') === '1';
      const idRaw = params.get('id');
      if (isNew) {
        this.showCreate = true;
        this.selectedId = null;
        this.supplier = null;
        this.personnel = [];
        this.companyDocs = [];
        this.companyDocsLoading = false;
        this.insuranceResult = null;
      } else if (idRaw) {
        const id = +idRaw;
        this.showCreate = false;
        if (id !== this.selectedId) {
          this.selectedId = id;
          this.loadSupplier(id);
          this.loadPersonnel(id);
          this.loadCompanyDocuments(id);
        }
      } else {
        this.showCreate = false;
        this.selectedId = null;
        this.supplier = null;
        this.personnel = [];
        this.companyDocs = [];
        this.companyDocsLoading = false;
      }
    });
  }

  openCreate(): void {
    this.router.navigate(['/app/suppliers'], { queryParams: { new: '1' } });
  }

  get filteredSuppliers(): Supplier[] {
    if (!this.filterText.trim()) return this.suppliers;
    const q = this.filterText.toLowerCase();
    return this.suppliers.filter((s) =>
      (s.companyName ?? '').toLowerCase().includes(q) ||
      (s.email ?? '').toLowerCase().includes(q) ||
      (s.ice ?? '').toLowerCase().includes(q) ||
      (s.phone ?? '').toLowerCase().includes(q) ||
      (s.address ?? '').toLowerCase().includes(q),
    );
  }

  cancelCreate(): void {
    this.router.navigate(['/app/suppliers']);
  }

  submitCreate(): void {
    if (this.form.invalid || this.createLoading) {
      this.form.markAllAsTouched();
      return;
    }
    this.createLoading = true;
    this.createError = null;
    const body = {
      companyName: this.form.value.companyName,
      ice: this.form.value.ice,
      email: this.form.value.email,
      phone: this.form.value.phone,
      address: this.form.value.address || null,
    };
    this.api.post<{ id: number }>('/api/supplier', body).subscribe({
      next: () => {
        this.form.reset();
        this.router.navigate(['/app/suppliers']);
        this.loadSuppliers();
      },
      error: (err) => {
        this.createError = err?.error?.message ?? 'Error while creating.';
        this.createLoading = false;
      },
      complete: () => (this.createLoading = false),
    });
  }

  openDetail(id: number): void {
    this.router.navigate(['/app/suppliers'], { queryParams: { id: String(id) } });
  }

  closeDetail(): void {
    this.router.navigate(['/app/suppliers']);
  }

  loadSuppliers(): void {
    this.loading = true;
    this.api.get<Supplier[]>('/api/supplier').subscribe({
      next: (data) => {
        this.suppliers = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  loadSupplier(id: number): void {
    this.detailLoading = true;
    this.supplier = null;
    this.api.get<Supplier>(`/api/supplier/${id}`).subscribe({
      next: (data) => {
        this.supplier = data;
        this.detailLoading = false;
      },
      error: () => (this.detailLoading = false),
    });
  }

  loadCompanyDocuments(supplierId: number): void {
    this.companyDocsLoading = true;
    this.api.get<SupplierCompanyDocument[]>(`/api/supplier/${supplierId}/company-documents`).subscribe({
      next: (data) => {
        this.companyDocs = data ?? [];
        this.companyDocsLoading = false;
      },
      error: () => (this.companyDocsLoading = false),
    });
  }

  openCompanyDocument(d: SupplierCompanyDocument): void {
    if (!d?.id) return;
    this.api.getBlob(`/api/supplier/company-documents/${d.id}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank', 'noopener,noreferrer');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: () => alert("Impossible d’ouvrir le document."),
    });
  }

  loadPersonnel(supplierId: number): void {
    this.personnelLoading = true;
    this.api.get<SupplierPersonnel[]>(`/api/supplier/${supplierId}/personnel`).subscribe({
      next: (data) => {
        this.personnel = data;
        this.personnelLoading = false;
      },
      error: () => (this.personnelLoading = false),
    });
  }

  blacklist(personnelId: number, isBlacklisted: boolean): void {
    this.api
      .post(`/api/admin/personnel/${personnelId}/blacklist?isBlacklisted=${isBlacklisted}`, {})
      .subscribe({
        next: () => {
          const p = this.personnel.find((x) => x.id === personnelId);
          if (p) p.isBlacklisted = isBlacklisted;
        },
      });
  }

  addPersonnel(): void {
    const id = this.supplier?.id;
    if (!id) return;
    if (this.auth.hasAnyRole(['ADMIN'])) {
      this.router.navigate(['/app/personnel/new'], { queryParams: { supplierId: id } });
    } else if (this.auth.hasAnyRole(['USER'])) {
      this.router.navigate(['/app/personnel/create'], { queryParams: { supplierId: id } });
    }
  }

  /** Édition / suppression fournisseur : réservé à l’admin (API + UI). */
  canManageSuppliers(): boolean {
    return this.auth.hasAnyRole(['ADMIN']);
  }

  openEdit(s: Supplier): void {
    this.editingSupplierId = s.id;
    this.editError = null;
    this.editForm.patchValue({
      companyName: s.companyName ?? '',
      ice: s.ice ?? '',
      email: s.email ?? '',
      phone: s.phone ?? '',
      address: s.address ?? '',
    });
    this.editVisible = true;
  }

  closeEdit(): void {
    this.editVisible = false;
    this.editingSupplierId = null;
    this.editError = null;
    this.editLoading = false;
  }

  submitEdit(): void {
    if (this.editForm.invalid || this.editLoading || this.editingSupplierId == null) {
      this.editForm.markAllAsTouched();
      return;
    }
    const id = this.editingSupplierId;
    const v = this.editForm.getRawValue();
    this.editLoading = true;
    this.editError = null;
    const body = {
      companyName: v.companyName,
      ice: v.ice,
      email: v.email,
      phone: v.phone,
      address: v.address || null,
    };
    this.api.put<Supplier>(`/api/supplier/${id}`, body).subscribe({
      next: (updated) => {
        const idx = this.suppliers.findIndex((x) => x.id === id);
        if (idx >= 0) this.suppliers = [...this.suppliers.slice(0, idx), updated, ...this.suppliers.slice(idx + 1)];
        if (this.supplier?.id === id) this.supplier = updated;
        this.closeEdit();
        if (this.selectedId === id) this.loadSupplier(id);
      },
      error: (err: { error?: { message?: string } }) => {
        this.editError = err?.error?.message ?? 'Update failed.';
        this.editLoading = false;
      },
    });
  }

  deleteSupplier(s: Supplier): void {
    if (!this.canManageSuppliers()) return;
    this.openDeleteModal(s);
  }

  openDeleteModal(supplier: Supplier): void {
    this.selectedSupplierToDelete = supplier;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    if (this.isDeleting) return;
    this.showDeleteModal = false;
    this.selectedSupplierToDelete = null;
  }

  confirmDelete(): void {
    if (!this.selectedSupplierToDelete || this.isDeleting) return;
    const target = this.selectedSupplierToDelete;
    this.isDeleting = true;
    this.api.delete(`/api/supplier/${target.id}`).subscribe({
      next: () => {
        if (this.selectedId === target.id) this.closeDetail();
        this.loadSuppliers();
        this.isDeleting = false;
        this.closeDeleteModal();
      },
      error: (err: { error?: { message?: string } }) => {
        alert(err?.error?.message ?? 'Delete failed.');
        this.isDeleting = false;
      },
    });
  }
}
