import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChartModule } from 'primeng/chart';
import { BlacklistRequestsComponent } from '../../rh/blacklist-requests/blacklist-requests.component';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ChartModule, BlacklistRequestsComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent implements OnInit {
  blacklistChartData: any;
  blacklistChartOptions: any;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    // Appel très simple : on réutilise la liste existante et on calcule les stats côté front.
    this.api.get<any[]>('/api/blacklist-requests').subscribe({
      next: (rows: any[]) => this.buildBlacklistChart(rows ?? []),
      error: () => this.buildBlacklistChart([]),
    });
  }

  private buildBlacklistChart(rows: any[]): void {
    const statusCounts: Record<string, number> = { Pending: 0, Approved: 0, Rejected: 0 };
    for (const r of rows) {
      const s = (r.status as string) ?? '';
      if (statusCounts[s] !== undefined) statusCounts[s]++;
    }

    this.blacklistChartData = {
      labels: ['Pending', 'Approved', 'Rejected'],
      datasets: [
        {
          data: [
            statusCounts['Pending'],
            statusCounts['Approved'],
            statusCounts['Rejected'],
          ],
          backgroundColor: ['#e98300', '#22c55e', '#ef4444'],
          hoverBackgroundColor: ['#b86800', '#16a34a', '#dc2626'],
        },
      ],
    };

    this.blacklistChartOptions = {
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
  }
}
