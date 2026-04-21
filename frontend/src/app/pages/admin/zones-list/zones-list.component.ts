import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  animate,
  query,
  stagger,
  style,
  transition,
  trigger,
} from '@angular/animations';
import { ApiService } from '../../../services/api.service';
import { ZoneCardComponent } from './zone-card/zone-card.component';
import { ZoneCreateDialogComponent } from './dialogs/zone-create-dialog.component';
import { ZoneEditDialogComponent } from './dialogs/zone-edit-dialog.component';
import { ZoneDeleteDialogComponent } from './dialogs/zone-delete-dialog.component';

export interface Zone {
  id: number;
  name: string;
  description?: string;
  interventionCount?: number;
}

@Component({
  selector: 'app-zones-list',
  standalone: true,
  imports: [
    CommonModule,
    ZoneCardComponent,
    ZoneCreateDialogComponent,
    ZoneEditDialogComponent,
    ZoneDeleteDialogComponent,
  ],
  templateUrl: './zones-list.component.html',
  styleUrl: './zones-list.component.scss',
  animations: [
    trigger('listAnimation', [
      transition('* => *', [
        query(
          ':enter',
          [
            style({ opacity: 0, transform: 'translateY(16px)' }),
            stagger(
              40,
              animate(
                '220ms ease-out',
                style({ opacity: 1, transform: 'translateY(0)' }),
              ),
            ),
          ],
          { optional: true },
        ),
        query(
          ':leave',
          [
            animate(
              '180ms ease-in',
              style({ opacity: 0, transform: 'scale(0.95)' }),
            ),
          ],
          { optional: true },
        ),
      ]),
    ]),
    trigger('cardAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate(
          '200ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
    ]),
  ],
})
export class ZonesListComponent implements OnInit {
  zones = signal<Zone[]>([]);
  loading = signal(false);
  page = signal(1);
  readonly pageSize = 9;

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.zones().length / this.pageSize)),
  );
  readonly pages = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1),
  );
  readonly pagedZones = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.zones().slice(start, start + this.pageSize);
  });
  readonly totalInterventions = computed(() =>
    this.zones().reduce((acc, z) => acc + (z.interventionCount ?? 0), 0),
  );

  createDialogVisible = false;
  editDialogVisible = false;
  deleteDialogVisible = false;
  selectedZone: Zone | null = null;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.get<Zone[]>('/api/admin/zones').subscribe({
      next: (data) => {
        this.zones.set(data ?? []);
        const maxPage = this.totalPages();
        if (this.page() > maxPage) {
          this.page.set(maxPage);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goTo(p: number): void {
    if (p >= 1 && p <= this.totalPages()) {
      this.page.set(p);
    }
  }

  goFirst(): void {
    this.page.set(1);
  }

  goLast(): void {
    this.page.set(this.totalPages());
  }

  goPrev(): void {
    if (this.page() > 1) {
      this.page.update((p) => p - 1);
    }
  }

  goNext(): void {
    if (this.page() < this.totalPages()) {
      this.page.update((p) => p + 1);
    }
  }

  openCreate(): void {
    this.createDialogVisible = true;
  }

  closeCreate(reload = false): void {
    this.createDialogVisible = false;
    if (reload) this.load();
  }

  openEdit(zone: Zone): void {
    this.selectedZone = zone;
    this.editDialogVisible = true;
  }

  closeEdit(reload = false): void {
    this.editDialogVisible = false;
    this.selectedZone = null;
    if (reload) this.load();
  }

  confirmDelete(zone: Zone): void {
    this.selectedZone = zone;
    this.deleteDialogVisible = true;
  }

  closeDelete(reload = false): void {
    this.deleteDialogVisible = false;
    this.selectedZone = null;
    if (reload) this.load();
  }
}
