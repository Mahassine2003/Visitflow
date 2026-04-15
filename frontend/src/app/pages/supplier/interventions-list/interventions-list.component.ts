import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ApiService } from '../../../services/api.service';

interface Intervention {
  id: number;
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
}

@Component({
  selector: 'app-supplier-interventions-list',
  standalone: true,
  imports: [CommonModule, TableModule, TagModule, ButtonModule],
  templateUrl: './interventions-list.component.html',
  styleUrl: './interventions-list.component.scss',
})
export class SupplierInterventionsListComponent implements OnInit {
  interventions: Intervention[] = [];
  loading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.get<Intervention[]>('/api/intervention').subscribe({
      next: (data) => {
        this.interventions = data ?? [];
      },
      error: () => {},
      complete: () => (this.loading = false),
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

  createNew(): void {
    this.router.navigate(['/app/interventions/new']);
  }
}
