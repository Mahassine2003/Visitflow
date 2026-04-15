import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, MenuModule],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
})
export class AdminLayoutComponent {
  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-chart-bar', routerLink: '/admin/dashboard' },
    { label: 'Suppliers', icon: 'pi pi-briefcase', routerLink: '/admin/suppliers' },
    { label: 'Zones', icon: 'pi pi-map', routerLink: '/admin/zones' },
    { label: 'Types of Work', icon: 'pi pi-list', routerLink: '/admin/type-of-works' },
    { label: 'Personal', icon: 'pi pi-users', routerLink: '/admin/personnel/new' },
    { label: 'Interventions', icon: 'pi pi-clipboard', routerLink: '/admin/interventions' },
    { label: 'New intervention', icon: 'pi pi-plus', routerLink: '/admin/interventions/new' },
  ];

  constructor(public auth: AuthService) {}

  logout(): void {
    this.auth.logout();
  }
}
