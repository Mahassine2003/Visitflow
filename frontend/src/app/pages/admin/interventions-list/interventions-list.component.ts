import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../core/auth/auth.service';

export interface Intervention {
  id: number;
  plantId?: number | null;
  plantName?: string | null;
  title: string;
  supplierId: number;
  zoneIds: number[];
  typeOfWorkId: number;
  description: string;
  startDate: string;
  endDate: string;
  startTime: string;
  endTime: string;
  status: string;
  hseApprovalStatus: string;
  visitKey: string;
  ppi: string;
  minPersonnel: number;
  minZone: number;
  createdBy: string;
  firePermitDetails?: string | null;
  heightPermitDetails?: string | null;
  hseFormDetails?: string | null;
  isHseValidated?: boolean;
}

export interface ElementOptionItem {
  id: number;
  label: string;
}

export interface InterventionElement {
  id: number;
  elementTypeId: number;
  elementTypeName: string;
  label: string;
  isChecked: boolean;
  context: string;
  /** Champs liés au type (depuis l’API). */
  options?: ElementOptionItem[];
  fieldValues?: Record<string, string>;
}

export interface SafetyMeasure {
  id: number;
  description: string;
  addedBy: string;
  createdAt: string;
}

export interface InterventionDetail extends Intervention {
  elements: InterventionElement[];
  safetyMeasures: SafetyMeasure[];
}

interface ElementTypeOption {
  id: number;
  name: string;
  options?: ElementOptionItem[];
}

@Component({
  selector: 'app-interventions-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TagModule,
    ButtonModule,
    DialogModule,
    DropdownModule,
    TableModule,
  ],
  templateUrl: './interventions-list.component.html',
  styleUrl: './interventions-list.component.scss',
})
export class InterventionsListComponent implements OnInit {
  interventions: Intervention[] = [];
  /** Mise en évidence de la carte après ajout d’élément (flux utilisateur fournisseur). */
  userFocusedInterventionId: number | null = null;
  loading = false;

  /** Clés d’expansion des lignes (p-table). */
  expandedRowKeys: Record<string, boolean> = {};

  detailCache = new Map<number, InterventionDetail>();
  elementTypes: ElementTypeOption[] = [];

  showElementDialog = false;
  showMeasureDialog = false;
  selectedInterventionId: number | null = null;

  newElement = { elementTypeId: null as number | null, label: '', context: 0 };
  newMeasureDescription = '';

  showHseFormDialog = false;
  hseFormText = '';
  hseCommentText = '';

  constructor(
    private api: ApiService,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
    this.api.get<ElementTypeOption[]>('/api/element-types').subscribe({
      next: (data) => (this.elementTypes = data),
      error: () => {},
    });
  }

  load(): void {
    this.loading = true;
    this.api.get<Intervention[]>('/api/intervention').subscribe({
      next: (data) => {
        const list = data ?? [];
        this.interventions = list;
        this.detailCache.clear();
        this.expandedRowKeys = {};
        this.userFocusedInterventionId = null;
      },
      error: () => {},
      complete: () => (this.loading = false),
    });
  }

  getDetail(id: number, callback?: () => void, force = false): void {
    if (!force && this.detailCache.has(id)) {
      callback?.();
      return;
    }
    this.api.get<InterventionDetail>(`/api/intervention/${id}`).subscribe({
      next: (d) => {
        this.detailCache.set(id, d);
        callback?.();
      },
      error: () => {},
    });
  }

  openAddElement(id: number): void {
    this.selectedInterventionId = id;
    this.userFocusedInterventionId = id;
    this.newElement = {
      elementTypeId: this.elementTypes[0]?.id ?? null,
      label: '',
      context: 0,
    };
    this.getDetail(id, () => (this.showElementDialog = true));
  }

  openAddMeasure(id: number): void {
    this.selectedInterventionId = id;
    this.newMeasureDescription = '';
    this.getDetail(id, () => (this.showMeasureDialog = true));
  }

  saveElement(): void {
    const id = this.selectedInterventionId;
    if (!id || !this.newElement.elementTypeId || !this.newElement.label.trim()) return;
    this.api
      .post<InterventionElement>(`/api/intervention/${id}/elements`, {
        elementTypeId: this.newElement.elementTypeId,
        label: this.newElement.label.trim(),
        isChecked: false,
        context: this.newElement.context,
      })
      .subscribe({
        next: () => {
          this.detailCache.delete(id);
          this.getDetail(
            id,
            () => {
              this.showElementDialog = false;
              this.newElement.label = '';
              this.userFocusedInterventionId = id;
            },
            true
          );
        },
      });
  }

  saveMeasure(): void {
    const id = this.selectedInterventionId;
    if (!id || !this.newMeasureDescription.trim()) return;
    this.api
      .post<SafetyMeasure>(`/api/intervention/${id}/safety-measures`, {
        description: this.newMeasureDescription.trim(),
      })
      .subscribe({
        next: () => {
          this.detailCache.delete(id);
          this.getDetail(id, () => {
            this.showMeasureDialog = false;
            this.newMeasureDescription = '';
          });
        },
      });
  }

  approveHse(id: number, approved: boolean): void {
    this.api.post(`/api/intervention/${id}/hse-approve?approved=${approved}`, {}).subscribe({
      next: () => {
        this.detailCache.delete(id);
        this.load();
      },
    });
  }

  openPdf(id: number): void {
    this.api.getBlob(`/api/pdf/intervention/${id}`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
    });
  }

  severity(status: string): 'success' | 'danger' | 'warning' | 'info' {
    const s = (status ?? '').toLowerCase();
    if (s === 'validated') return 'success';
    if (s === 'rejected') return 'danger';
    if (s === 'pending') return 'warning';
    return 'info';
  }

  isHse(): boolean {
    return this.auth.hasAnyRole(['HSE']);
  }

  isRh(): boolean {
    return this.auth.hasAnyRole(['RH']);
  }

  cachedDetail(id: number): InterventionDetail | undefined {
    return this.detailCache.get(id);
  }

  openHseForm(id: number): void {
    this.selectedInterventionId = id;
    this.getDetail(id, () => {
      const d = this.detailCache.get(id);
      this.hseFormText = d?.hseFormDetails ?? '';
      this.hseCommentText = '';
      this.showHseFormDialog = true;
    });
  }

  /** Options affichées pour un élément (API ou secours depuis les types chargés). */
  elementOptions(el: InterventionElement): ElementOptionItem[] {
    if (el.options?.length) return el.options;
    const t = this.elementTypes.find((x) => x.id === el.elementTypeId);
    return t?.options ?? [];
  }

  fieldValue(el: InterventionElement, optionId: number): string {
    return el.fieldValues?.[String(optionId)] ?? '';
  }

  onFieldValueChange(el: InterventionElement, optionId: number, value: string): void {
    if (!el.fieldValues) el.fieldValues = {};
    el.fieldValues[String(optionId)] = value;
  }

  saveElementFields(interventionId: number, el: InterventionElement): void {
    this.api
      .patch<InterventionElement>(`/api/intervention/${interventionId}/elements/${el.id}/fields`, {
        fieldValues: el.fieldValues ?? {},
      })
      .subscribe({
        next: (updated) => {
          const d = this.detailCache.get(interventionId);
          if (!d) return;
          const idx = d.elements.findIndex((x) => x.id === el.id);
          if (idx >= 0) {
            d.elements[idx] = { ...d.elements[idx], ...updated };
          }
        },
        error: () => {},
      });
  }

  saveHseForm(): void {
    const id = this.selectedInterventionId;
    if (!id) return;
    this.api
      .patch(`/api/intervention/${id}/workflow`, {
        hseFormDetails: this.hseFormText || null,
        hseComment: this.hseCommentText || null,
      })
      .subscribe({
        next: () => {
          this.detailCache.delete(id);
          this.showHseFormDialog = false;
          this.load();
        },
      });
  }
}
