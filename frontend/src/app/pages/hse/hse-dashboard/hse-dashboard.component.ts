import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ChartModule } from 'primeng/chart';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-hse-dashboard',
  standalone: true,
  imports: [CommonModule, CardComponent, ChartModule],
  templateUrl: './hse-dashboard.component.html',
  styleUrl: './hse-dashboard.component.scss',
})
export class HseDashboardComponent implements OnInit {
  stats = [
    { label: 'Interventions to validate', value: 0 },
    { label: 'Rejected interventions', value: 0 },
    { label: 'Insurance to verify', value: 0 },
  ];

  interventionChartData: any;
  interventionChartOptions: any;

  interventionsPerMonthData: any;
  interventionsPerMonthOptions: any;

  interventionsPerTypeData: any;
  interventionsPerTypeOptions: any;

  private typeOfWorks: { id: number; name: string }[] = [];

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadInterventionStats();
  }

  private loadInterventionStats(): void {
    // Charger d'abord les types de travail pour nommer les barres par type
    this.api.get<{ id: number; name: string }[]>('/api/type-of-work').subscribe({
      next: (types) => {
        this.typeOfWorks = types ?? [];
        this.api.get<any[]>('/api/intervention').subscribe({
          next: (rows) => this.buildFromInterventions(rows ?? []),
          error: () => this.buildFromInterventions([]),
        });
      },
      error: () => {
        this.typeOfWorks = [];
        this.api.get<any[]>('/api/intervention').subscribe({
          next: (rows) => this.buildFromInterventions(rows ?? []),
          error: () => this.buildFromInterventions([]),
        });
      },
    });
  }

  private buildFromInterventions(rows: any[]): void {
    let pending = 0;
    let rejected = 0;
    let validated = 0;

    const perMonth: Record<string, number> = {};
    const perType: Record<number, number> = {};

    for (const r of rows) {
      const s = ((r.status as string) ?? '').toLowerCase();
      if (s === 'pending') pending++;
      else if (s === 'rejected') rejected++;
      else if (s === 'validated') validated++;

      const start = (r.startDate as string) ?? '';
      if (start) {
        // startDate au format "YYYY-MM-DD"
        const key = start.substring(0, 7); // "YYYY-MM"
        perMonth[key] = (perMonth[key] ?? 0) + 1;
      }

      const towId = (r.typeOfWorkId as number) ?? null;
      if (towId != null) {
        perType[towId] = (perType[towId] ?? 0) + 1;
      }
    }

    this.stats = [
      { label: 'Interventions to validate', value: pending },
      { label: 'Rejected interventions', value: rejected },
      { label: 'Validated interventions', value: validated },
    ];

    this.interventionChartData = {
      labels: ['Pending', 'Validated', 'Rejected'],
      datasets: [
        {
          data: [pending, validated, rejected],
          backgroundColor: ['#e98300', '#22c55e', '#ef4444'],
          hoverBackgroundColor: ['#b86800', '#16a34a', '#dc2626'],
        },
      ],
    };

    this.interventionChartOptions = {
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            usePointStyle: true,
            padding: 16,
          },
        },
      },
    };

    // Diagramme par mois (barres)
    const monthKeys = Object.keys(perMonth).sort();
    this.interventionsPerMonthData = {
      labels: monthKeys,
      datasets: [
        {
          label: 'Interventions',
          data: monthKeys.map((k) => perMonth[k]),
          backgroundColor: '#0ea5e9',
        },
      ],
    };
    this.interventionsPerMonthOptions = {
      responsive: true,
      plugins: {
        legend: {
          display: false,
        },
      },
      scales: {
        x: {
          ticks: { font: { size: 10 } },
        },
        y: {
          beginAtZero: true,
          ticks: { stepSize: 1 },
        },
      },
    };

    // Diagramme par type de travail (barres horizontales)
    const typeIds = Object.keys(perType)
      .map((id) => +id)
      .filter((id) => perType[id] > 0);
    const typeLabels = typeIds.map(
      (id) => this.typeOfWorks.find((t) => t.id === id)?.name ?? `Type #${id}`
    );

    this.interventionsPerTypeData = {
      labels: typeLabels,
      datasets: [
        {
          label: 'Interventions',
          data: typeIds.map((id) => perType[id]),
          backgroundColor: '#6366f1',
        },
      ],
    };
    this.interventionsPerTypeOptions = {
      indexAxis: 'y',
      responsive: true,
      plugins: {
        legend: {
          display: false,
        },
      },
      scales: {
        x: {
          beginAtZero: true,
          ticks: { stepSize: 1 },
        },
        y: {
          ticks: { font: { size: 10 } },
        },
      },
    };
  }
}
