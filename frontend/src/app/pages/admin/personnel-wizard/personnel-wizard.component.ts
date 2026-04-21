import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { catchError, forkJoin, of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { animate, style, transition, trigger } from '@angular/animations';
import { StepperModule } from 'primeng/stepper';
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

interface Supplier {
  id: number;
  companyName: string;
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

interface DocumentGroupByWorkType {
  typeOfWorkId: number;
  typeOfWorkName: string;
  documents: ComplianceRequirement[];
}

@Component({
  selector: 'app-personnel-wizard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    StepperModule,
    InsuranceUploadComponent,
    DocumentUploadIaComponent,
  ],
  templateUrl: './personnel-wizard.component.html',
  styleUrl: './personnel-wizard.component.scss',
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
  ],
})
export class PersonnelWizardComponent implements OnInit {
  activeStep = 0;

  personalForm = this.fb.group({
    supplierId: [null as number | null, Validators.required],
    fullName: ['', Validators.required],
    cin: ['', Validators.required],
    phone: ['', Validators.required],
    position: ['', Validators.required],
    startDate: [''],
    endDate: [''],
  });

  insuranceSource: 'ai' | 'manual' = 'ai';
  insurance1Result: InsuranceResult | null = null;
  insuranceAiFile: File | null = null;
  insuranceManualFile: File | null = null;
  manualInsuranceIssue = '';
  manualInsuranceExpiry = '';
  manualInsuranceValid = true;
  optionalDocResults: Record<number, DocumentIaResult | null> = {};
  optionalDocFiles: Record<number, File | null> = {};

  docError: string | null = null;

  suppliers: Supplier[] = [];
  typeOfWorks: TypeOfWorkOption[] = [];
  selectedTypeOfWorkIds: number[] = [];
  typeRequirements: ComplianceRequirement[] = [];
  documentGroupsByWorkType: DocumentGroupByWorkType[] = [];
  requirementsLoading = false;

  @ViewChild('manualInsuranceFile') manualInsuranceFileRef?: ElementRef<HTMLInputElement>;

  manualFileDragOver = false;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  get optionalTrainingAndDocs(): ComplianceRequirement[] {
    return this.typeRequirements.filter(
      (r) => (r.type ?? '').toLowerCase() === 'document',
    );
  }

  get hasDocumentRequirements(): boolean {
    return this.documentGroupsWithDocs.length > 0;
  }

  get documentGroupsWithDocs(): DocumentGroupByWorkType[] {
    return this.documentGroupsByWorkType.filter((g) => g.documents.length > 0);
  }

  trackDocumentReq(_i: number, r: ComplianceRequirement): number {
    return r.id;
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
    this.loadSuppliers();
    this.loadTypeOfWorks();
    const sid = this.route.snapshot.queryParamMap.get('supplierId');
    if (sid) this.personalForm.patchValue({ supplierId: +sid });
  }

  isTypeSelected(id: number): boolean {
    return this.selectedTypeOfWorkIds.includes(id);
  }

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
          .pipe(catchError(() => of([] as ComplianceRequirement[]))),
      ),
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

  loadSuppliers(): void {
    this.api.get<Supplier[]>('/api/supplier').subscribe({
      next: (data) => (this.suppliers = data),
    });
  }

  loadTypeOfWorks(): void {
    this.api.get<TypeOfWorkOption[]>('/api/type-of-work').subscribe({
      next: (data) => (this.typeOfWorks = data),
    });
  }

  setOptionalResult(id: number, res: DocumentIaResult | null): void {
    this.optionalDocResults = { ...this.optionalDocResults, [id]: res };
  }

  setOptionalFile(id: number, file: File | null): void {
    this.optionalDocFiles = { ...this.optionalDocFiles, [id]: file };
  }

  setInsuranceSource(next: 'ai' | 'manual'): void {
    if (this.insuranceSource === next) return;
    this.insuranceSource = next;
    // Keep uploaded insurance data when switching mode.
    // This prevents losing the selected file/results unintentionally.
    this.manualFileDragOver = false;
    this.cdr.markForCheck();
  }

  onManualInsuranceFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0] ?? null;
    this.insuranceManualFile = f;
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
      this.insuranceManualFile = f;
      this.cdr.markForCheck();
    }
  }

  clearManualInsuranceFile(): void {
    this.insuranceManualFile = null;
    if (this.manualInsuranceFileRef?.nativeElement) {
      this.manualInsuranceFileRef.nativeElement.value = '';
    }
    this.cdr.markForCheck();
  }

  openManualInsuranceFile(): void {
    if (!this.insuranceManualFile) return;
    const fileUrl = URL.createObjectURL(this.insuranceManualFile);
    window.open(fileUrl, '_blank', 'noopener,noreferrer');
    setTimeout(() => URL.revokeObjectURL(fileUrl), 60000);
  }

  formatManualFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  submit(): void {
    this.docError = null;
    if (this.personalForm.invalid) {
      this.personalForm.markAllAsTouched();
      return;
    }

    const personal = this.personalForm.value;
    const supplierId = personal.supplierId;
    if (supplierId == null) return;
    const selectedInsuranceFile =
      this.insuranceSource === 'ai' ? this.insuranceAiFile : this.insuranceManualFile;

    if (!selectedInsuranceFile) {
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

    const typeOfWorkId =
      this.selectedTypeOfWorkIds.length > 0 ? this.selectedTypeOfWorkIds[0] : null;

    const payload = {
      fullName: personal.fullName,
      cin: personal.cin,
      phone: personal.phone,
      fieldOfActivity: personal.position,
      supplierId,
      isBlacklisted: false,
      typeOfWorkId,
    };

    this.api.post<{ id: number }>('/api/supplier/personnel', payload).subscribe({
      next: (created) => {
        const personnelId = created?.id;
        if (!personnelId) {
          this.router.navigate(['/app/personnel']);
          return;
        }

        const uploads = [];

        if (selectedInsuranceFile) {
          const fd = new FormData();
          const ai = this.insuranceSource === 'ai';
          fd.append('isValid', ai ? String(!!this.insurance1Result?.isValid) : String(this.manualInsuranceValid));
          fd.append('issueDate', ai ? (this.insurance1Result?.startDate ?? '') : this.manualInsuranceIssue);
          fd.append('expiryDate', ai ? (this.insurance1Result?.endDate ?? '') : this.manualInsuranceExpiry);
          fd.append('validatedByAi', String(ai));
          fd.append('file', selectedInsuranceFile);
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
          this.router.navigate(['/app/personnel']);
          return;
        }

        forkJoin(uploads.map((o) => o.pipe(catchError(() => of(null))))).subscribe({
          next: () => this.router.navigate(['/app/personnel']),
          error: () => this.router.navigate(['/app/personnel']),
        });
      },
      error: () => this.router.navigate(['/app/personnel']),
    });
  }
}
