import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-supplier-layout',
  standalone: true,
  imports: [RouterOutlet, MenuModule],
  templateUrl: './supplier-layout.component.html',
  styleUrl: './supplier-layout.component.scss',
})
export class SupplierLayoutComponent {
  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-chart-line', routerLink: '/supplier/dashboard' },
    { label: 'Personal', icon: 'pi pi-users', routerLink: '/supplier/personnel' },
    { label: 'Documents / permits', icon: 'pi pi-file', routerLink: '/supplier/documents' },
    { label: 'Interventions', icon: 'pi pi-clipboard', routerLink: '/supplier/interventions' },
    { label: 'File (AI)', icon: 'pi pi-file', routerLink: '/supplier/assurance' },
  ];

  constructor(public auth: AuthService) {}

  logout(): void {
    this.auth.logout();
  }
}
