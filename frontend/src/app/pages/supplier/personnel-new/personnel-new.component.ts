import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { animate, style, transition, trigger } from '@angular/animations';
import { catchError, forkJoin, of } from 'rxjs';
import { ApiService } from '../../../services/api.service';
import { InsuranceUploadComponent } from '../../../shared/components/insurance-upload/insurance-upload.component';
import {
  DocumentIaResult,
  DocumentUploadIaComponent,
} from '../../../shared/components/document-upload-ia/document-upload-ia.component';

interface InsuranceResult {
  isValid: boolean;
  status?: string | null;
  year: number | null;
  startDate?: string | null;
  endDate?: string | null;
  rawText?: string | null;
}

interface TypeOfWorkOption {
  id: number;
  name: string;
  description: string;
  requiresInsurance: boolean;
}

interface ComplianceRequirement {
  id: number;
  type: string;
  title: string;
}

/** Documents attendus regroupés par type de travail (affichage). */
interface DocumentGroupByWorkType {
  typeOfWorkId: number;
  typeOfWorkName: string;
  documents: ComplianceRequirement[];
}

interface SupplierOption {
  id: number;
  companyName: string;
}

@Component({
  selector: 'app-personnel-new',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    InsuranceUploadComponent,
    DocumentUploadIaComponent,
  ],
  templateUrl: './personnel-new.component.html',
  styleUrl: './personnel-new.component.scss',
  animations: [
    trigger('modeSwitch', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(6px)' }),
        animate(
          '220ms ease',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
    ]),
    trigger('toastAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(110%)' }),
        animate(
          '300ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ opacity: 1, transform: 'translateX(0)' }),
        ),
      ]),
      transition(':leave', [
        animate(
          '200ms ease',
          style({ opacity: 0, transform: 'translateX(110%)' }),
        ),
      ]),
    ]),
  ],
})
export class PersonnelNewComponent implements OnInit, OnDestroy {
  form = this.fb.group({
    supplierId: [null as number | null, Validators.required],
    fullName: ['', Validators.required],
    cin: ['', Validators.required],
    phone: ['', Validators.required],
    fieldOfActivity: ['', Validators.required],
  });
  loading = false;
  error = '';

  suppliers: SupplierOption[] = [];
  typeOfWorks: TypeOfWorkOption[] = [];
  /** Types cochés (plusieurs possibles). */
  selectedTypeOfWorkIds: number[] = [];
  typeRequirements: ComplianceRequirement[] = [];
  /** Exigences « document » par type de travail (une section par type sélectionné). */
  documentGroupsByWorkType: DocumentGroupByWorkType[] = [];
  requirementsLoading = false;

  insuranceSource: 'ai' | 'manual' = 'ai';
  insurance1Result: InsuranceResult | null = null;
  insuranceFile: File | null = null;
  manualInsuranceIssue = '';
  manualInsuranceExpiry = '';
  manualInsuranceValid = true;
  optionalDocResults: Record<number, DocumentIaResult | null> = {};
  optionalDocFiles: Record<number, File | null> = {};
  docError: string | null = null;

  showManualToast = false;
  private manualToastTimer: ReturnType<typeof setTimeout> | null = null;

  @ViewChild('manualInsuranceFile') manualInsuranceFileRef?: ElementRef<HTMLInputElement>;

  /** Glisser-déposer sur la zone manuelle (même fichier qu’avec « Parcourir »). */
  manualFileDragOver = false;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnDestroy(): void {
    if (this.manualToastTimer != null) {
      clearTimeout(this.manualToastTimer);
      this.manualToastTimer = null;
    }
  }

  triggerManualToast(): void {
    if (this.manualToastTimer != null) {
      clearTimeout(this.manualToastTimer);
      this.manualToastTimer = null;
    }
    this.showManualToast = true;
    this.manualToastTimer = setTimeout(() => {
      this.showManualToast = false;
      this.manualToastTimer = null;
    }, 4500);
  }

  /** Après succès : toast manuel puis navigation (délai aligné sur l’animation du toast). */
  private navigateAfterSuccessfulSave(): void {
    if (this.insuranceSource === 'manual') {
      this.triggerManualToast();
      setTimeout(() => this.router.navigate(['/app/personnel']), 4500);
      return;
    }
    this.router.navigate(['/app/personnel']);
  }

  get optionalTrainingAndDocs(): ComplianceRequirement[] {
    return this.typeRequirements.filter(
      (r) => (r.type ?? '').toLowerCase() === 'document',
    );
  }

  /** Au moins un document attendu parmi les types sélectionnés. */
  get hasDocumentRequirements(): boolean {
    return this.documentGroupsWithDocs.length > 0;
  }

  /** Groupes ayant au moins un document (affichage). */
  get documentGroupsWithDocs(): DocumentGroupByWorkType[] {
    return this.documentGroupsByWorkType.filter((g) => g.documents.length > 0);
  }

  trackDocumentReq(_i: number, r: ComplianceRequirement): number {
    return r.id;
  }

  setInsuranceSource(next: 'ai' | 'manual'): void {
    if (this.insuranceSource === next) return;
    this.insuranceSource = next;
    this.insurance1Result = null;
    this.insuranceFile = null;
    this.manualInsuranceIssue = '';
    this.manualInsuranceExpiry = '';
    this.manualInsuranceValid = true;
    this.manualFileDragOver = false;
    if (this.manualInsuranceFileRef?.nativeElement) {
      this.manualInsuranceFileRef.nativeElement.value = '';
    }
    this.cdr.markForCheck();
  }

  onManualInsuranceFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0] ?? null;
    this.insuranceFile = f;
    this.insurance1Result = null;
    this.cdr.markForCheck();
  }

  onManualInsuranceDragOver(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.manualFileDragOver = true;
  }

  onManualInsuranceDragLeave(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.manualFileDragOver = false;
  }

  onManualInsuranceDrop(ev: DragEvent): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.manualFileDragOver = false;
    const f = ev.dataTransfer?.files?.[0];
    if (f) {
      this.insuranceFile = f;
      this.insurance1Result = null;
      this.cdr.markForCheck();
    }
  }

  clearManualInsuranceFile(): void {
    this.insuranceFile = null;
    this.insurance1Result = null;
    if (this.manualInsuranceFileRef?.nativeElement) {
      this.manualInsuranceFileRef.nativeElement.value = '';
    }
    this.cdr.markForCheck();
  }

  formatManualFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  trackWorkTypeGroup(_i: number, g: DocumentGroupByWorkType): number {
    return g.typeOfWorkId;
  }

  private normalizeRequirementRow(raw: unknown): ComplianceRequirement {
    const o = raw && typeof raw === 'object' ? (raw as Record<string, unknown>) : {};
    const id = Number(o['id'] ?? o['Id']);
    const type = String(o['type'] ?? o['Type'] ?? '');
    const title = String(o['title'] ?? o['Title'] ?? '');
    return {
      id: Number.isFinite(id) ? id : NaN,
      type,
      title,
    };
  }

  ngOnInit(): void {
    const sid = this.route.snapshot.queryParamMap.get('supplierId');
    if (sid) {
      const n = +sid;
      if (!Number.isNaN(n)) this.form.patchValue({ supplierId: n });
    }
    this.api.get<SupplierOption[]>('/api/supplier').subscribe({
      next: (data) => (this.suppliers = data ?? []),
    });
    this.api.get<TypeOfWorkOption[]>('/api/type-of-work').subscribe({
      next: (data) => (this.typeOfWorks = data ?? []),
    });
  }

  isTypeSelected(id: number): boolean {
    return this.selectedTypeOfWorkIds.includes(id);
  }

  /** Bascule la sélection d’un type (toute la carte est cliquable). */
  toggleType(id: number): void {
    if (this.selectedTypeOfWorkIds.includes(id)) {
      this.selectedTypeOfWorkIds = this.selectedTypeOfWorkIds.filter((x) => x !== id);
    } else {
      this.selectedTypeOfWorkIds = [...this.selectedTypeOfWorkIds, id];
    }
    this.reloadMergedRequirements();
  }

  private reloadMergedRequirements(): void {
    this.docError = null;
    if (this.selectedTypeOfWorkIds.length === 0) {
      this.typeRequirements = [];
      this.documentGroupsByWorkType = [];
      this.optionalDocResults = {};
      this.optionalDocFiles = {};
      return;
    }

    this.requirementsLoading = true;
    forkJoin(
      this.selectedTypeOfWorkIds.map((tid) =>
        this.api
          .get<ComplianceRequirement[]>(`/api/type-of-work/${tid}/requirements`)
          .pipe(catchError(() => of([] as ComplianceRequirement[])))
      )
    ).subscribe({
      next: (arrays) => {
        const normalizedArrays = arrays.map((arr) =>
          (arr ?? []).map((raw) => this.normalizeRequirementRow(raw)).filter((r) => Number.isFinite(r.id)),
        );

        const byId = new Map<number, ComplianceRequirement>();
        for (const arr of normalizedArrays) {
          for (const r of arr) {
            byId.set(r.id, r);
          }
        }
        this.typeRequirements = Array.from(byId.values());

        this.documentGroupsByWorkType = this.selectedTypeOfWorkIds.map((tid, i) => {
          const tow = this.typeOfWorks.find((x) => x.id === tid);
          const arr = normalizedArrays[i] ?? [];
          const documents = arr.filter(
            (r) => (r.type ?? '').toLowerCase() === 'document',
          );
          const name = tow?.name?.trim();
          return {
            typeOfWorkId: tid,
            typeOfWorkName: name && name.length > 0 ? name : `#${tid}`,
            documents,
          };
        });

        this.optionalDocResults = {};
        this.optionalDocFiles = {};
        for (const r of this.optionalTrainingAndDocs) {
          this.optionalDocResults[r.id] = null;
        }
        this.requirementsLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.requirementsLoading = false;
      },
    });
  }

  setOptionalResult(id: number, res: DocumentIaResult | null): void {
    this.optionalDocResults = { ...this.optionalDocResults, [id]: res };
  }

  setOptionalFile(id: number, file: File | null): void {
    this.optionalDocFiles = { ...this.optionalDocFiles, [id]: file };
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    const sid = this.form.value.supplierId;
    if (sid == null) {
      this.form.get('supplierId')?.markAsTouched();
      return;
    }

    this.docError = null;
    const v = this.form.getRawValue();
    if (!this.insuranceFile) {
      this.docError = 'Insurance document is required.';
      return;
    }
    if (this.insuranceSource === 'ai') {
      if (!this.insurance1Result) {
        this.docError = 'Please analyze the insurance document with AI first, or switch to manual upload.';
        return;
      }
      if (!this.insurance1Result.isValid) {
        this.docError = 'Invalid insurance document (AI). Use manual upload if needed.';
        return;
      }
    }

    for (const r of this.optionalTrainingAndDocs) {
      const file = this.optionalDocFiles[r.id];
      if (!file) continue;
      const res = this.optionalDocResults[r.id];
      if (!res) {
        this.docError = `Choose AI validation or manual upload for: "${r.title}".`;
        return;
      }
      if (res.validatedByAI && !res.isValid) {
        this.docError = `Invalid document (AI): "${r.title}".`;
        return;
      }
    }

    this.loading = true;
    this.error = '';
    /** L’API ne stocke qu’un seul type : on envoie le premier coché. */
    const typeOfWorkId =
      this.selectedTypeOfWorkIds.length > 0 ? this.selectedTypeOfWorkIds[0] : null;

    const body = {
      fullName: v.fullName,
      cin: v.cin,
      phone: v.phone,
      fieldOfActivity: v.fieldOfActivity,
      supplierId: sid,
      isBlacklisted: false,
      typeOfWorkId,
    };
    this.api.post<{ id: number }>('/api/supplier/personnel', body).subscribe({
      next: (created) => {
        const personnelId = created?.id;
        if (!personnelId) {
          this.router.navigate(['/app/personnel']);
          return;
        }

        const uploads = [];

        if (this.insuranceFile) {
          const fd = new FormData();
          const ai = this.insuranceSource === 'ai';
          fd.append('isValid', ai ? String(!!this.insurance1Result?.isValid) : String(this.manualInsuranceValid));
          fd.append('issueDate', ai ? (this.insurance1Result?.startDate ?? '') : this.manualInsuranceIssue);
          fd.append('expiryDate', ai ? (this.insurance1Result?.endDate ?? '') : this.manualInsuranceExpiry);
          fd.append('validatedByAi', String(ai));
          fd.append('file', this.insuranceFile);
          uploads.push(this.api.post(`/api/insurance/${personnelId}/upload`, fd));
        }

        for (const r of this.optionalTrainingAndDocs) {
          const file = this.optionalDocFiles[r.id];
          if (!file) continue;
          const res = this.optionalDocResults[r.id];
          if (!res) continue;
          const fd = new FormData();
          fd.append('documentType', r.title);
          fd.append('isValid', String(res.isValid ?? true));
          fd.append('validatedByAI', String(!!res.validatedByAI));
          fd.append('file', file);
          uploads.push(this.api.post(`/api/personnel/${personnelId}/documents/upload`, fd));
        }

        if (uploads.length === 0) {
          this.navigateAfterSuccessfulSave();
          return;
        }

        forkJoin(uploads.map((o) => o.pipe(catchError(() => of(null))))).subscribe({
          next: () => this.navigateAfterSuccessfulSave(),
          error: () => this.router.navigate(['/app/personnel']),
        });
      },
      error: (err: { error?: string | { title?: string } }) => {
        const e = err?.error;
        if (typeof e === 'string') this.error = e;
        else if (e && typeof e === 'object' && e.title) this.error = e.title;
        else this.error = 'Error while saving.';
        this.loading = false;
      },
      complete: () => (this.loading = false),
    });
  }
}
