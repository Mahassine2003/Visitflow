import { ApplicationRef, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  ValidatorFn,
  AbstractControl,
} from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ApiService } from '../../../services/api.service';

interface Zone {
  id: number;
  name: string;
}

interface TypeOfWork {
  id: number;
  name: string;
}

interface Supplier {
  id: number;
  companyName: string;
}

interface Personnel {
  id: number;
  fullName: string;
}

interface Plant {
  id: number;
  name: string;
}

type Tac2SubSite = 'Toubkal' | 'Indus';

interface InterventionResponse {
  id: number;
}

export interface InterventionWizardFieldDefinitionDto {
  id: number;
  key: string;
  label: string;
  fieldType: number;
  sortOrder: number;
  isRequired: boolean;
  /** 0 = custom field, 1 = list bound to an existing SQL table/column. */
  creationMode?: number;
  sourceSchema?: string | null;
  sourceTable?: string | null;
  sourceColumn?: string | null;
  /** Labels for dropdown when fieldType is Select (4). */
  options?: string[] | null;
}

interface SchemaTableRow {
  schemaName: string;
  tableName: string;
}

interface SchemaColumnRow {
  columnName: string;
  dataType: string;
}

const CREATION_CUSTOM = 0;
const CREATION_DATABASE = 1;

/** Fixed fields (not from DB definitions): supplier, personnel, dates — no ✎/− on toolbar. */
export type CoreFieldKey = 'supplierId' | 'personnel' | 'startDate' | 'endDate';

/** Keys mapped to intervention create payload; excluded from CustomFieldValues JSON. */
const RESERVED_WIZARD_PAYLOAD_KEYS = new Set([
  'title',
  'description',
  'ppi',
  'zoneIds',
  'typeOfWorkId',
  'startTime',
  'endTime',
]);

@Component({
  selector: 'app-intervention-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, DialogModule],
  templateUrl: './intervention-wizard.component.html',
  styleUrl: './intervention-wizard.component.scss',
})
export class InterventionWizardComponent implements OnInit {
  /** 0 = general info, 1 = fire, 2 = height, 3 = validation */
  activeStep = 0;

  readonly wizardStepLabels = [
    'General information',
    'Fire permit',
    'Height permit',
    'Validation',
  ];

  readonly maxStepIndex = 3;

  generalForm = this.fb.group({
    supplierId: [null as number | null, Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    minPersonnel: [0, [Validators.required, Validators.min(0)]],
    minZone: [0, [Validators.required, Validators.min(0)]],
  });

  customFieldForm: FormGroup = this.fb.group({});

  fireForm = this.fb.group({
    details: [''],
  });

  heightForm = this.fb.group({
    details: [''],
  });

  assignForm = this.fb.group({
    personnelIds: this.fb.control<number[]>([]),
  });

  zones: Zone[] = [];
  typeOfWorks: TypeOfWork[] = [];
  suppliers: Supplier[] = [];
  personnelList: Personnel[] = [];
  plants: Plant[] = [];
  /** Required before showing the rest of step 1 (intervention details). */
  selectedPlantId: number | null = null;
  selectedTac2SubSite: Tac2SubSite | null = null;
  pendingTac2PlantId: number | null = null;
  readonly tac2SubSites: Tac2SubSite[] = ['Toubkal', 'Indus'];

  fieldDefinitions: InterventionWizardFieldDefinitionDto[] = [];
  fieldDefinitionsLoading = false;

  fieldDialogVisible = false;
  fieldDialogMode: 'add' | 'edit' = 'add';
  editingFieldId: number | null = null;
  /** Dialog state for add/edit custom field (fieldType: Text=0, Number=1, Date=2, Time=3, Select=4). */
  fieldForm = {
    label: '',
    fieldType: 0,
    isRequired: false,
    sortOrder: 0,
    /** One option per line when type is Selection. */
    fieldOptionsLines: '',
  };
  fieldDialogSubmitting = false;
  deletingFieldId: number | null = null;

  /** Add-field dialog step: mode choice then form. */
  fieldAddWizardStep: 'choice' | 'form' = 'choice';
  /** Mode for a new field: custom vs database-linked. */
  addFieldKind: 'custom' | 'database' | null = null;
  schemaTables: SchemaTableRow[] = [];
  schemaTablesLoading = false;
  schemaColumns: SchemaColumnRow[] = [];
  schemaColumnsLoading = false;
  bindingSchema = 'dbo';
  bindingTable = '';
  bindingColumn = '';
  /** Table &lt;select&gt; value (schema + table). */
  bindingTableKey = '';
  /** Options loaded from DB for fields with creationMode === 1. */
  dbBoundSelectOptions: Record<string, string[]> = {};
  dbBoundSelectLoading: Record<string, boolean> = {};

  /** Stable slug from API — selection by `id` alone was unreliable after JSON / CD. */
  selectedFieldKey: string | null = null;

  /** Selected fixed field (title, supplier, etc.) — mutually exclusive with `selectedFieldKey`. */
  selectedCoreKey: CoreFieldKey | null = null;

  /** Fire (1) or Height (2) step: selected permit text area for ✎ / −. */
  selectedPermitKey: 'fire' | 'height' | null = null;

  interventionId: number | null = null;

  readonly coreFieldLabels: Record<CoreFieldKey, string> = {
    supplierId: 'Supplier',
    personnel: 'Personnel (by name)',
    startDate: 'Start date',
    endDate: 'End date',
  };

  readonly fieldTypeOptions = [
    { value: 0, label: 'Text' },
    { value: 4, label: 'Selection (dropdown)' },
    { value: 1, label: 'Number' },
    { value: 2, label: 'Date' },
    { value: 3, label: 'Time' },
  ];

  get sortedFieldDefinitions(): InterventionWizardFieldDefinitionDto[] {
    return [...this.fieldDefinitions].sort(
      (a, b) =>
        a.sortOrder - b.sortOrder ||
        this.normalizeFieldId(a.id) - this.normalizeFieldId(b.id),
    );
  }

  /**
   * Show dynamic fields as soon as known; do not hide them while reloading
   * (avoids flicker when `fieldDefinitionsLoading` is true again).
   */
  get showCustomFieldsSection(): boolean {
    if (!this.fieldDefinitionsLoading) return true;
    return this.fieldDefinitions.length > 0;
  }

  /** Current selection for toolbar + row highlight (always from latest `fieldDefinitions`). */
  get selectedField(): InterventionWizardFieldDefinitionDto | null {
    if (this.selectedFieldKey == null || this.selectedFieldKey === '') return null;
    return this.fieldDefinitions.find((x) => x.key === this.selectedFieldKey) ?? null;
  }

  /** `fieldForm.fieldType` may be number or string from the dialog select. */
  get isFieldFormSelectType(): boolean {
    return Number(this.fieldForm.fieldType) === 4;
  }

  /** No field selected — used for visual state only (buttons stay clickable). */
  get toolbarEditNoSelection(): boolean {
    if (this.activeStep === 0) {
      return !this.selectedField && this.selectedCoreKey == null;
    }
    if (this.activeStep === 1 || this.activeStep === 2) {
      return this.selectedPermitKey == null;
    }
    return true;
  }

  /** Supplier, personnel, dates: no ✎ / − (direct entry on the form). */
  get toolbarCoreBlocked(): boolean {
    return (
      this.activeStep === 0 &&
      (this.selectedCoreKey === 'supplierId' ||
        this.selectedCoreKey === 'personnel' ||
        this.selectedCoreKey === 'startDate' ||
        this.selectedCoreKey === 'endDate')
    );
  }

  /** Step 1 (General) ✎/− toolbar: dimmed on wrong step or empty/blocked selection. */
  get toolbarEditDimmedStep0(): boolean {
    if (this.activeStep !== 0) return true;
    return this.toolbarEditNoSelection || this.toolbarCoreBlocked;
  }

  /** Fire / Height ✎/− toolbar: dimmed when not on step or nothing selected. */
  get toolbarPermitActionsDimmed(): boolean {
    if (this.activeStep !== 1 && this.activeStep !== 2) return true;
    return this.toolbarEditNoSelection;
  }

  /** True while a DELETE request is in flight (avoid double submit). */
  get toolbarDeleteBusy(): boolean {
    return this.deletingFieldId != null;
  }

  get fieldDialogSelectionInvalid(): boolean {
    if (this.fieldDialogMode === 'add' && this.addFieldKind === 'database') return false;
    return this.isFieldFormSelectType && this.parseFieldOptionsFromLines().length === 0;
  }

  get fieldDialogSaveDisabled(): boolean {
    if (this.fieldDialogSubmitting) return true;
    if (this.fieldDialogMode === 'add' && this.fieldAddWizardStep === 'choice') return true;
    if (!this.fieldForm.label.trim()) return true;
    if (this.fieldDialogMode === 'add' && this.addFieldKind === 'database' && this.fieldAddWizardStep === 'form') {
      if (!this.bindingSchema.trim() || !this.bindingTable.trim() || !this.bindingColumn.trim()) return true;
    }
    if (this.fieldDialogMode === 'edit' && this.editingFieldIsDatabase) {
      if (!this.bindingSchema.trim() || !this.bindingTable.trim() || !this.bindingColumn.trim()) return true;
    }
    return this.fieldDialogSelectionInvalid;
  }

  get editingFieldIsDatabase(): boolean {
    if (this.fieldDialogMode !== 'edit' || this.editingFieldId == null) return false;
    const d = this.fieldDefinitions.find((x) => this.normalizeFieldId(x.id) === this.editingFieldId);
    return (d?.creationMode ?? CREATION_CUSTOM) === CREATION_DATABASE;
  }

  get showWizardCustomFieldForm(): boolean {
    if (this.fieldDialogMode === 'add' && this.fieldAddWizardStep === 'choice') return false;
    if (this.fieldDialogMode === 'add' && this.addFieldKind === 'database') return false;
    if (this.fieldDialogMode === 'edit' && this.editingFieldIsDatabase) return false;
    return true;
  }

  get showWizardDatabaseFieldForm(): boolean {
    if (this.fieldDialogMode === 'add' && this.fieldAddWizardStep === 'form' && this.addFieldKind === 'database') {
      return true;
    }
    if (this.fieldDialogMode === 'edit' && this.editingFieldIsDatabase) return true;
    return false;
  }

  get supplierLabel(): string {
    const id = this.generalForm.value.supplierId;
    if (id == null) return '—';
    const s = this.suppliers.find((x) => x.id === id);
    return s?.companyName ?? '—';
  }

  get selectedPlantLabel(): string {
    if (this.selectedPlantId == null) return '—';
    const selectedPlant = this.plants.find((p) => p.id === this.selectedPlantId);
    if (!selectedPlant) return '—';
    if (this.isTac2Plant(selectedPlant) && this.selectedTac2SubSite) {
      return `${selectedPlant.name} - ${this.selectedTac2SubSite}`;
    }
    return selectedPlant.name;
  }

  /** Checked personnel (validation recap). */
  get selectedPersonnelNamesDisplay(): string {
    const ids = this.assignForm.value.personnelIds ?? [];
    if (!ids.length) return '—';
    const names = ids.map((id) => this.personnelList.find((p) => p.id === id)?.fullName ?? `#${id}`);
    return names.filter((n) => n.length > 0).join(', ') || '—';
  }

  /** DB-defined fields for the Validation step (values from the dynamic form). */
  get validationCustomFieldRows(): { label: string; value: string }[] {
    let raw: Record<string, unknown> = {};
    try {
      raw = this.customFieldForm.getRawValue() as Record<string, unknown>;
    } catch {
      raw = {};
    }
    const rows: { label: string; value: string }[] = [];
    for (const d of this.sortedFieldDefinitions) {
      const v = raw[d.key];
      let display = '';
      if (d.key === 'zoneIds' && Array.isArray(v)) {
        const ids = v as number[];
        const names = ids
          .map((id) => this.zones.find((z) => z.id === id)?.name)
          .filter((n): n is string => !!n && n.length > 0);
        display = names.length ? names.join(', ') : '';
      } else if (d.key === 'typeOfWorkId' && v !== null && v !== undefined && v !== '') {
        const id = Number(v);
        display = Number.isFinite(id)
          ? (this.typeOfWorks.find((t) => t.id === id)?.name ?? String(v))
          : String(v);
      } else if (v === null || v === undefined) {
        display = '';
      } else {
        display = String(v).trim();
      }
      rows.push({ label: d.label, value: display.length > 0 ? display : '—' });
    }
    return rows;
  }

  /** Custom field row highlight. */
  isCustomFieldSelected(d: InterventionWizardFieldDefinitionDto): boolean {
    return this.selectedFieldKey != null && d.key === this.selectedFieldKey;
  }

  isCoreSelected(key: CoreFieldKey): boolean {
    return this.selectedCoreKey === key;
  }

  trackByFieldKey(_index: number, d: InterventionWizardFieldDefinitionDto): string {
    return d.key;
  }

  /** Stable numeric id from API (handles string / PascalCase payloads). */
  private normalizeFieldId(id: unknown): number {
    const n = Number(id);
    return Number.isFinite(n) ? n : NaN;
  }

  private normalizeFieldDefinition(raw: Record<string, unknown>): InterventionWizardFieldDefinitionDto {
    const id = this.normalizeFieldId(raw['id'] ?? raw['Id']);
    const key = String(raw['key'] ?? raw['Key'] ?? '');
    const label = String(raw['label'] ?? raw['Label'] ?? '');
    const fieldType = Number(raw['fieldType'] ?? raw['FieldType'] ?? 0);
    const sortOrder = Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0);
    const isRequired = Boolean(raw['isRequired'] ?? raw['IsRequired']);
    const options = this.normalizeFieldOptions(raw['options'] ?? raw['Options']);
    const creationMode = Number(raw['creationMode'] ?? raw['CreationMode'] ?? CREATION_CUSTOM);
    const sourceSchema = (raw['sourceSchema'] ?? raw['SourceSchema']) as string | null | undefined;
    const sourceTable = (raw['sourceTable'] ?? raw['SourceTable']) as string | null | undefined;
    const sourceColumn = (raw['sourceColumn'] ?? raw['SourceColumn']) as string | null | undefined;
    return {
      id: Number.isFinite(id) ? id : NaN,
      key,
      label,
      fieldType: Number.isFinite(fieldType) ? fieldType : 0,
      sortOrder: Number.isFinite(sortOrder) ? sortOrder : 0,
      isRequired,
      creationMode: Number.isFinite(creationMode) ? creationMode : CREATION_CUSTOM,
      sourceSchema: sourceSchema ?? null,
      sourceTable: sourceTable ?? null,
      sourceColumn: sourceColumn ?? null,
      options,
    };
  }

  private normalizeFieldOptions(raw: unknown): string[] | undefined {
    if (raw == null) return undefined;
    if (Array.isArray(raw)) {
      const list = raw.map((x) => String(x ?? '').trim()).filter((s) => s.length > 0);
      return list.length ? list : undefined;
    }
    if (typeof raw === 'string' && raw.trim()) {
      try {
        const parsed = JSON.parse(raw) as unknown;
        if (Array.isArray(parsed)) {
          const list = parsed.map((x) => String(x ?? '').trim()).filter((s) => s.length > 0);
          return list.length ? list : undefined;
        }
      } catch {
        /* ignore */
      }
    }
    return undefined;
  }

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private cdr: ChangeDetectorRef,
    private appRef: ApplicationRef,
  ) {}

  ngOnInit(): void {
    this.clampActiveStep();
    this.loadLookups();
    this.loadFieldDefinitions();
    this.generalForm.get('supplierId')?.valueChanges.subscribe((id) => {
      if (id) this.loadPersonnel(id);
      else this.personnelList = [];
    });
  }

  loadLookups(): void {
    this.api.get<Zone[]>('/api/admin/zones').subscribe({
      next: (data) => {
        this.zones = data ?? [];
        this.refreshViewAfterAsync();
      },
    });
    this.api.get<TypeOfWork[]>('/api/admin/type-of-works').subscribe({
      next: (data) => {
        this.typeOfWorks = data ?? [];
        this.refreshViewAfterAsync();
      },
    });
    this.api.get<Supplier[]>('/api/supplier').subscribe({
      next: (data) => {
        this.suppliers = data ?? [];
        this.refreshViewAfterAsync();
      },
    });
    const plantOrder = ['TFZ', 'TAC1', 'TAC2'];
    this.api.get<Plant[]>('/api/plant').subscribe({
      next: (data) => {
        const rows = data ?? [];
        this.plants = [...rows].sort(
          (a, b) =>
            plantOrder.indexOf(a.name) - plantOrder.indexOf(b.name) ||
            a.name.localeCompare(b.name),
        );
        this.refreshViewAfterAsync();
      },
    });
  }

  private viewRefreshCoalesce = false;
  private pendingFullAppTick = false;

  /**
   * Ensures dynamic selects (zones, work type, etc.) paint after HTTP — same idea as
   * explicit detectChanges() on field click, without requiring user interaction.
   * Coalesces parallel lookup responses into one follow-up pass.
   */
  private refreshViewAfterAsync(options?: { fullAppTick: boolean }): void {
    if (options?.fullAppTick) {
      this.pendingFullAppTick = true;
    }
    this.cdr.detectChanges();
    if (this.viewRefreshCoalesce) return;
    this.viewRefreshCoalesce = true;
    queueMicrotask(() => {
      this.viewRefreshCoalesce = false;
      this.cdr.detectChanges();
      if (this.pendingFullAppTick) {
        this.pendingFullAppTick = false;
        requestAnimationFrame(() => this.appRef.tick());
      }
    });
  }

  private isTac2Plant(plant: Plant): boolean {
    return plant.name.trim().toUpperCase() === 'TAC2';
  }

  onPlantCardClick(plant: Plant): void {
    if (this.isTac2Plant(plant)) {
      this.pendingTac2PlantId = plant.id;
      this.selectedPlantId = null;
      this.selectedTac2SubSite = null;
      this.cdr.detectChanges();
      return;
    }
    this.pendingTac2PlantId = null;
    this.selectedTac2SubSite = null;
    this.selectedPlantId = plant.id;
    this.cdr.detectChanges();
  }

  selectTac2SubSite(subSite: Tac2SubSite): void {
    if (this.pendingTac2PlantId == null) return;
    this.selectedPlantId = this.pendingTac2PlantId;
    this.selectedTac2SubSite = subSite;
    this.pendingTac2PlantId = null;
    this.cdr.detectChanges();
  }

  cancelTac2SubSiteSelection(): void {
    this.pendingTac2PlantId = null;
    this.selectedTac2SubSite = null;
    this.cdr.detectChanges();
  }

  clearPlantSelection(): void {
    this.selectedPlantId = null;
    this.selectedTac2SubSite = null;
    this.pendingTac2PlantId = null;
    this.cdr.detectChanges();
  }

  loadFieldDefinitions(): void {
    this.fieldDefinitionsLoading = true;
    this.api.get<InterventionWizardFieldDefinitionDto[]>('/api/admin/intervention-wizard-fields').subscribe({
      next: (data) => {
        const rows = Array.isArray(data)
          ? data
              .map((row) => this.normalizeFieldDefinition(row as unknown as Record<string, unknown>))
              .filter((x) => Number.isFinite(x.id) && x.key.length > 0)
          : [];
        this.fieldDefinitions = rows;
        if (
          this.selectedFieldKey != null &&
          !rows.some((x) => x.key === this.selectedFieldKey)
        ) {
          this.selectedFieldKey = null;
        }
        this.rebuildCustomFieldForm();
        this.hydrateDbBoundSelectOptions();
        this.fieldDefinitionsLoading = false;
        this.refreshViewAfterAsync({ fullAppTick: true });
      },
      error: () => {
        this.fieldDefinitionsLoading = false;
        this.refreshViewAfterAsync({ fullAppTick: true });
      },
    });
  }

  private rebuildCustomFieldForm(): void {
    let previous: Record<string, unknown> = {};
    try {
      previous = this.customFieldForm.getRawValue() as Record<string, unknown>;
    } catch {
      previous = {};
    }
    const next = this.fb.group({});
    const sorted = [...this.fieldDefinitions].sort(
      (a, b) =>
        a.sortOrder - b.sortOrder ||
        this.normalizeFieldId(a.id) - this.normalizeFieldId(b.id),
    );
    for (const d of sorted) {
      if (d.key === 'plant') continue;
      const raw = previous[d.key];
      if (d.key === 'zoneIds') {
        const validators: ValidatorFn[] = d.isRequired ? [this.zoneIdsRequiredValidator()] : [];
        let initial: number[] = [];
        if (Array.isArray(raw)) initial = raw as number[];
        else if (typeof raw === 'string' && raw.trim()) {
          initial = raw
            .split(/[,\s]+/)
            .map((s) => Number(s.trim()))
            .filter((n) => Number.isFinite(n));
        }
        next.addControl(d.key, this.fb.control<number[]>(initial, validators));
        continue;
      }
      if (d.key === 'typeOfWorkId') {
        const validators: ValidatorFn[] = d.isRequired ? [Validators.required] : [];
        let initial: number | null = null;
        if (raw !== undefined && raw !== null && raw !== '') {
          const n = Number(raw);
          initial = Number.isFinite(n) ? n : null;
        }
        next.addControl(d.key, this.fb.control<number | null>(initial, validators));
        continue;
      }
      const validators = d.isRequired ? [Validators.required] : [];
      const initial = raw !== undefined && raw !== null && raw !== '' ? raw : '';
      next.addControl(d.key, this.fb.control(initial, validators));
    }
    this.customFieldForm = next;
    this.cdr.markForCheck();
  }

  isSelectField(d: InterventionWizardFieldDefinitionDto): boolean {
    return Number(d.fieldType) === 4;
  }

  /** Called from the template: no side effects (no HTTP) to avoid change-detection loops. */
  getFieldOptions(d: InterventionWizardFieldDefinitionDto): string[] {
    if (d.creationMode === CREATION_DATABASE && this.isSelectField(d)) {
      return this.dbBoundSelectOptions[d.key] ?? [];
    }
    const o = d.options;
    return Array.isArray(o) ? o.filter((x) => (x ?? '').toString().trim().length > 0) : [];
  }

  private hydrateDbBoundSelectOptions(): void {
    const bound = this.fieldDefinitions.filter(
      (d) =>
        d.creationMode === CREATION_DATABASE &&
        Number(d.fieldType) === 4 &&
        d.sourceTable &&
        d.sourceColumn,
    );
    const incomingKeys = new Set(bound.map((d) => d.key));
    for (const k of Object.keys(this.dbBoundSelectOptions)) {
      if (!incomingKeys.has(k)) delete this.dbBoundSelectOptions[k];
    }
    for (const k of Object.keys(this.dbBoundSelectLoading)) {
      if (!incomingKeys.has(k)) delete this.dbBoundSelectLoading[k];
    }
    for (const d of bound) {
      if (!Object.prototype.hasOwnProperty.call(this.dbBoundSelectOptions, d.key)) {
        this.loadDistinctForField(d);
      }
    }
  }

  private loadDistinctForField(d: InterventionWizardFieldDefinitionDto): void {
    const key = d.key;
    if (!d.sourceTable?.trim() || !d.sourceColumn?.trim()) return;
    if (this.dbBoundSelectLoading[key]) return;
    if (Object.prototype.hasOwnProperty.call(this.dbBoundSelectOptions, key)) return;
    this.dbBoundSelectLoading[key] = true;
    const schema = (d.sourceSchema && d.sourceSchema.trim()) || 'dbo';
    const params = new URLSearchParams({
      schema,
      table: d.sourceTable.trim(),
      column: d.sourceColumn.trim(),
    });
    this.api.get<string[]>(`/api/admin/database-schema/distinct-values?${params.toString()}`).subscribe({
      next: (rows) => {
        this.dbBoundSelectOptions[key] = rows ?? [];
        this.dbBoundSelectLoading[key] = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.dbBoundSelectOptions[key] = [];
        this.dbBoundSelectLoading[key] = false;
        this.cdr.markForCheck();
      },
    });
  }

  inputType(d: InterventionWizardFieldDefinitionDto): string {
    const ft = Number(d.fieldType);
    switch (ft) {
      case 1:
        return 'number';
      case 2:
        return 'date';
      case 3:
        return 'time';
      default:
        return 'text';
    }
  }

  /** Special rendering for reserved keys (zones, work type, long description). */
  fieldWidget(
    d: InterventionWizardFieldDefinitionDto,
  ): 'zones' | 'workType' | 'textarea' | 'select' | 'input' {
    if (d.key === 'zoneIds') return 'zones';
    if (d.key === 'typeOfWorkId') return 'workType';
    if (d.key === 'description') return 'textarea';
    if (this.isSelectField(d)) return 'select';
    return 'input';
  }

  private zoneIdsRequiredValidator(): ValidatorFn {
    return (control: AbstractControl) => {
      const v = control.value;
      if (Array.isArray(v) && v.length > 0) return null;
      return { required: true };
    };
  }

  private parseFieldOptionsFromLines(): string[] {
    return this.fieldForm.fieldOptionsLines
      .split(/\r?\n/)
      .map((s) => s.trim())
      .filter((s) => s.length > 0);
  }

  openAddFieldDialog(): void {
    this.fieldDialogSubmitting = false;
    this.fieldDialogMode = 'add';
    this.editingFieldId = null;
    this.fieldAddWizardStep = 'choice';
    this.addFieldKind = null;
    this.schemaColumns = [];
    this.bindingSchema = 'dbo';
    this.bindingTable = '';
    this.bindingColumn = '';
    this.bindingTableKey = '';
    const maxOrder = this.fieldDefinitions.reduce((m, x) => Math.max(m, x.sortOrder), 0);
    this.fieldForm = {
      label: '',
      fieldType: 0,
      isRequired: false,
      sortOrder: maxOrder + 1,
      fieldOptionsLines: '',
    };
    this.fieldDialogVisible = true;
    this.cdr.detectChanges();
  }

  tableRowKey(t: SchemaTableRow): string {
    return `${t.schemaName}\x1e${t.tableName}`;
  }

  parseTableRowKey(key: string): { schema: string; table: string } | null {
    const parts = key.split('\x1e');
    if (parts.length !== 2 || !parts[0] || !parts[1]) return null;
    return { schema: parts[0], table: parts[1] };
  }

  confirmAddFieldChoice(): void {
    if (this.addFieldKind !== 'custom' && this.addFieldKind !== 'database') return;
    this.fieldAddWizardStep = 'form';
    if (this.addFieldKind === 'database') {
      this.fieldForm.fieldType = 4;
      this.loadSchemaTables();
    }
    this.cdr.detectChanges();
  }

  onBindingTableKeyChange(key: string): void {
    this.bindingTableKey = key;
    this.bindingColumn = '';
    this.schemaColumns = [];
    const parsed = key ? this.parseTableRowKey(key) : null;
    if (!parsed) {
      this.bindingSchema = 'dbo';
      this.bindingTable = '';
      return;
    }
    this.bindingSchema = parsed.schema;
    this.bindingTable = parsed.table;
    this.loadSchemaColumns(parsed.schema, parsed.table);
  }

  private loadSchemaTables(): void {
    this.schemaTablesLoading = true;
    this.schemaTables = [];
    this.api.get<SchemaTableRow[]>('/api/admin/database-schema/tables').subscribe({
      next: (data) => {
        this.schemaTables = data ?? [];
        this.schemaTablesLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.schemaTablesLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  private loadSchemaColumns(schema: string, table: string): void {
    this.schemaColumnsLoading = true;
    this.schemaColumns = [];
    const q = new URLSearchParams({ schema, table });
    this.api.get<SchemaColumnRow[]>(`/api/admin/database-schema/columns?${q.toString()}`).subscribe({
      next: (data) => {
        this.schemaColumns = data ?? [];
        this.schemaColumnsLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.schemaColumnsLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  selectField(d: InterventionWizardFieldDefinitionDto): void {
    this.selectedFieldKey = d.key && d.key.length > 0 ? d.key : null;
    this.selectedCoreKey = null;
    this.selectedPermitKey = null;
    this.cdr.detectChanges();
  }

  selectCoreField(key: CoreFieldKey): void {
    this.selectedCoreKey = key;
    this.selectedFieldKey = null;
    this.selectedPermitKey = null;
    this.cdr.detectChanges();
  }

  selectPermitField(key: 'fire' | 'height'): void {
    this.selectedPermitKey = key;
    this.selectedFieldKey = null;
    this.selectedCoreKey = null;
    this.cdr.detectChanges();
  }

  isPermitSelected(key: 'fire' | 'height'): boolean {
    return this.selectedPermitKey === key;
  }

  editSelectedField(
    source: 'general-toolbar' | 'fire-toolbar' | 'height-toolbar' = 'general-toolbar',
  ): void {
    if (source === 'general-toolbar' && this.activeStep !== 0) {
      return;
    }
    if (source === 'fire-toolbar' && this.activeStep !== 1) {
      return;
    }
    if (source === 'height-toolbar' && this.activeStep !== 2) {
      return;
    }

    if (source === 'fire-toolbar') {
      if (this.selectedPermitKey !== 'fire') {
        return;
      }
      this.focusPermitTextarea('wizard-fire-details');
      return;
    }

    if (source === 'height-toolbar') {
      if (this.selectedPermitKey !== 'height') {
        return;
      }
      this.focusPermitTextarea('wizard-height-details');
      return;
    }

    if (this.toolbarCoreBlocked) {
      return;
    }
    if (this.toolbarEditNoSelection) {
      return;
    }

    const def = this.selectedField;
    if (def) {
      const id = this.normalizeFieldId(def.id);
      if (!Number.isFinite(id)) {
        return;
      }
      setTimeout(() => this.openEditFieldDialog(def), 0);
    }
  }

  deleteSelectedField(): void {
    if (this.toolbarCoreBlocked) {
      return;
    }
    if (this.activeStep === 1 && this.selectedPermitKey === 'fire') {
      this.fireForm.patchValue({ details: '' });
      this.cdr.markForCheck();
      return;
    }
    if (this.activeStep === 2 && this.selectedPermitKey === 'height') {
      this.heightForm.patchValue({ details: '' });
      this.cdr.markForCheck();
      return;
    }
    const def = this.selectedField;
    if (def) {
      this.deleteField(def);
    }
  }

  private focusPermitTextarea(elementId: string): void {
    const el = document.getElementById(elementId);
    if (el && 'focus' in el) {
      (el as HTMLElement).focus({ preventScroll: true });
    }
  }

  openEditFieldDialog(d: InterventionWizardFieldDefinitionDto): void {
    this.fieldDialogSubmitting = false;
    const id = this.normalizeFieldId(d.id);
    if (!Number.isFinite(id)) {
      return;
    }
    this.fieldDialogMode = 'edit';
    this.editingFieldId = id;
    this.fieldAddWizardStep = 'form';
    const ft = Number(d.fieldType);
    const so = Number(d.sortOrder);
    this.fieldForm = {
      label: d.label,
      fieldType: Number.isFinite(ft) ? ft : 0,
      isRequired: !!d.isRequired,
      sortOrder: Number.isFinite(so) ? Math.trunc(so) : 0,
      fieldOptionsLines: (d.options ?? []).filter((x) => (x ?? '').toString().trim()).join('\n'),
    };
    const isDb = (d.creationMode ?? CREATION_CUSTOM) === CREATION_DATABASE;
    this.addFieldKind = isDb ? 'database' : 'custom';
    this.bindingSchema = (d.sourceSchema && d.sourceSchema.trim()) || 'dbo';
    this.bindingTable = (d.sourceTable && d.sourceTable.trim()) || '';
    this.bindingColumn = (d.sourceColumn && d.sourceColumn.trim()) || '';
    this.bindingTableKey = '';
    this.schemaColumns = [];
    if (isDb && this.bindingTable) {
      this.bindingTableKey = this.tableRowKey({
        schemaName: this.bindingSchema,
        tableName: this.bindingTable,
      });
      this.loadSchemaTables();
      this.loadSchemaColumns(this.bindingSchema, this.bindingTable);
    }
    this.fieldDialogVisible = true;
    this.cdr.detectChanges();
  }

  closeFieldDialog(): void {
    this.fieldDialogVisible = false;
    this.fieldDialogSubmitting = false;
    this.editingFieldId = null;
    this.fieldAddWizardStep = 'choice';
    this.addFieldKind = null;
    this.bindingTableKey = '';
  }

  saveFieldDialog(): void {
    const label = this.fieldForm.label.trim();
    if (!label || this.fieldDialogSubmitting) return;
    if (this.fieldDialogMode === 'add' && this.fieldAddWizardStep === 'choice') return;

    const rawFt = Number(this.fieldForm.fieldType);
    const fieldType = Number.isFinite(rawFt)
      ? Math.min(4, Math.max(0, Math.trunc(rawFt)))
      : 0;
    const rawSo = Number(this.fieldForm.sortOrder);
    const sortOrder = Number.isFinite(rawSo) ? Math.max(0, Math.trunc(rawSo)) : 0;

    const editedDef =
      this.fieldDialogMode === 'edit' && this.editingFieldId != null
        ? this.fieldDefinitions.find((x) => this.normalizeFieldId(x.id) === this.editingFieldId)
        : undefined;
    const isDbBinding =
      this.fieldDialogMode === 'add'
        ? this.addFieldKind === 'database'
        : (editedDef?.creationMode ?? CREATION_CUSTOM) === CREATION_DATABASE;

    if (!isDbBinding && fieldType === 4 && this.parseFieldOptionsFromLines().length === 0) {
      return;
    }
    const fieldOptions = !isDbBinding && fieldType === 4 ? this.parseFieldOptionsFromLines() : undefined;

    this.fieldDialogSubmitting = true;
    if (this.fieldDialogMode === 'add') {
      const creationMode = this.addFieldKind === 'database' ? CREATION_DATABASE : CREATION_CUSTOM;
      const body: Record<string, unknown> = {
        creationMode,
        label,
        fieldType: isDbBinding ? 4 : fieldType,
        isRequired: this.fieldForm.isRequired,
        sortOrder,
      };
      if (creationMode === CREATION_DATABASE) {
        body['sourceSchema'] = this.bindingSchema.trim() || 'dbo';
        body['sourceTable'] = this.bindingTable.trim();
        body['sourceColumn'] = this.bindingColumn.trim();
      } else if (fieldType === 4 && fieldOptions) {
        body['fieldOptions'] = fieldOptions;
      }
      this.api.post<InterventionWizardFieldDefinitionDto>('/api/admin/intervention-wizard-fields', body).subscribe({
        next: () => {
          this.fieldDialogSubmitting = false;
          this.closeFieldDialog();
          this.loadFieldDefinitions();
        },
        error: () => (this.fieldDialogSubmitting = false),
      });
    } else if (this.editingFieldId != null && Number.isFinite(this.editingFieldId)) {
      const editId = Math.trunc(this.editingFieldId);
      const creationMode = isDbBinding ? CREATION_DATABASE : CREATION_CUSTOM;
      const body: Record<string, unknown> = {
        creationMode,
        label,
        fieldType: isDbBinding ? 4 : fieldType,
        isRequired: this.fieldForm.isRequired,
        sortOrder,
      };
      if (creationMode === CREATION_DATABASE) {
        body['sourceSchema'] = this.bindingSchema.trim() || 'dbo';
        body['sourceTable'] = this.bindingTable.trim();
        body['sourceColumn'] = this.bindingColumn.trim();
      } else if (fieldType === 4 && fieldOptions) {
        body['fieldOptions'] = fieldOptions;
      }
      this.api
        .put<InterventionWizardFieldDefinitionDto>(`/api/admin/intervention-wizard-fields/${editId}`, body)
        .subscribe({
          next: () => {
            this.fieldDialogSubmitting = false;
            this.closeFieldDialog();
            this.loadFieldDefinitions();
          },
          error: () => (this.fieldDialogSubmitting = false),
        });
    } else {
      this.fieldDialogSubmitting = false;
    }
  }

  deleteField(d: InterventionWizardFieldDefinitionDto): void {
    if (!confirm(`Delete the field "${d.label}"? This cannot be undone.`)) return;
    const idNum = this.normalizeFieldId(d.id);
    if (!Number.isFinite(idNum)) return;
    this.deletingFieldId = idNum;
    this.api.delete(`/api/admin/intervention-wizard-fields/${idNum}`).subscribe({
      next: () => {
        this.deletingFieldId = null;
        if (this.selectedFieldKey === d.key) {
          this.selectedFieldKey = null;
        }
        this.loadFieldDefinitions();
      },
      error: () => (this.deletingFieldId = null),
    });
  }

  private serializeCustomFields(): Record<string, string> {
    const raw = this.customFieldForm.getRawValue() as Record<string, unknown>;
    const out: Record<string, string> = {};
    for (const k of Object.keys(raw)) {
      if (RESERVED_WIZARD_PAYLOAD_KEYS.has(k)) continue;
      const v = raw[k];
      if (v === null || v === undefined) out[k] = '';
      else if (Array.isArray(v)) out[k] = JSON.stringify(v);
      else out[k] = String(v);
    }
    return out;
  }

  private getPayloadZoneIds(): number[] {
    const ctrl = this.customFieldForm.get('zoneIds');
    const v = ctrl?.value;
    if (Array.isArray(v)) return (v as number[]).filter((x) => Number.isFinite(x));
    return [];
  }

  private getPayloadTypeOfWorkId(): number {
    const raw = this.customFieldForm.get('typeOfWorkId')?.value;
    const n = Number(raw);
    return Number.isFinite(n) ? n : 0;
  }

  private getPayloadString(key: string): string {
    const v = this.customFieldForm.get(key)?.value;
    if (v === null || v === undefined) return '';
    return String(v).trim();
  }

  loadPersonnel(supplierId: number): void {
    this.api.get<Personnel[]>(`/api/supplier/${supplierId}/personnel`).subscribe({
      next: (data) => (this.personnelList = data),
    });
  }

  private clampActiveStep(): void {
    if (
      typeof this.activeStep !== 'number' ||
      this.activeStep < 0 ||
      this.activeStep > this.maxStepIndex
    ) {
      this.activeStep = 0;
    }
  }

  goToStep(index: number): void {
    const i = Math.max(0, Math.min(this.maxStepIndex, index));
    if (i > 0 && this.selectedPlantId == null) {
      this.activeStep = 0;
      this.clearFieldSelection();
      this.cdr.markForCheck();
      return;
    }
    if (i === this.activeStep) return;
    this.activeStep = i;
    this.clearFieldSelection();
    this.cdr.markForCheck();
  }

  private clearFieldSelection(): void {
    this.selectedFieldKey = null;
    this.selectedCoreKey = null;
    this.selectedPermitKey = null;
  }

  next(): void {
    if (this.activeStep === 0 && this.selectedPlantId == null) {
      return;
    }
    if (this.activeStep < this.maxStepIndex) {
      this.activeStep++;
      this.clearFieldSelection();
      this.cdr.markForCheck();
    }
  }

  prev(): void {
    if (this.activeStep > 0) {
      this.activeStep--;
      this.clearFieldSelection();
      this.cdr.markForCheck();
    }
  }

  isPersonnelSelected(id: number): boolean {
    return (this.assignForm.value.personnelIds ?? []).includes(id);
  }

  togglePersonnel(id: number, checked: boolean): void {
    this.selectCoreField('personnel');
    const control = this.assignForm.controls.personnelIds;
    const current = control.value ?? [];
    if (checked) {
      control.setValue([...current, id]);
    } else {
      control.setValue(current.filter((x) => x !== id));
    }
  }

  submit(): void {
    if (this.selectedPlantId == null) {
      return;
    }
    if (this.generalForm.invalid || this.customFieldForm.invalid) {
      this.generalForm.markAllAsTouched();
      this.customFieldForm.markAllAsTouched();
      return;
    }

    const g = this.generalForm.value;
    const personnelIds = this.assignForm.value.personnelIds ?? [];
    const zoneIds = this.getPayloadZoneIds();

    this.generalForm.patchValue({
      minPersonnel: personnelIds.length,
      minZone: zoneIds.length,
    });

    const customFieldValues = this.serializeCustomFields();
    const hasCustomValues = Object.keys(customFieldValues).length > 0;

    const payload = {
      plantId: this.selectedPlantId,
      title: this.getPayloadString('title'),
      supplierId: g.supplierId!,
      typeOfWorkId: this.getPayloadTypeOfWorkId(),
      zoneIds,
      description: this.getPayloadString('description'),
      startDate: g.startDate!,
      endDate: g.endDate!,
      startTime: this.getPayloadString('startTime') || '08:00',
      endTime: this.getPayloadString('endTime') || '17:00',
      ppi: this.getPayloadString('ppi'),
      minPersonnel: personnelIds.length,
      minZone: zoneIds.length,
      firePermitDetails: this.fireForm.value.details || null,
      heightPermitDetails: this.heightForm.value.details || null,
      ...(hasCustomValues ? { customFieldValues } : {}),
    };

    this.api.post<InterventionResponse>('/api/intervention', payload).subscribe({
      next: (res) => {
        this.interventionId = res.id;
        if (personnelIds.length > 0) {
          this.api.post(`/api/intervention/${res.id}/assign-personnel`, personnelIds).subscribe();
        }
      },
    });
  }
}
