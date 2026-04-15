import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { AuthService } from '../../core/auth/auth.service';
import { ToastComponent } from '../../shared/toast/toast.component';

@Component({
  selector: 'app-actor-layout',
  standalone: true,
  imports: [RouterOutlet, MenuModule, ToastComponent],
  templateUrl: './actor-layout.component.html',
  styleUrl: './actor-layout.component.scss',
})
export class ActorLayoutComponent {
  readonly menuItems: MenuItem[];
  companyLogoUrl: string =
    localStorage.getItem('company_logo_url') ?? 'assets/TE_Connectivity_logo.svg';

  constructor(public auth: AuthService) {
    this.menuItems = this.buildMenu();
  }

  onLogoError(): void {
    this.companyLogoUrl = 'assets/company-logo.svg';
  }

  logout(): void {
    this.auth.logout();
  }

  private buildMenu(): MenuItem[] {
    const items: Array<MenuItem & { roles?: string[] }> = [
      { label: 'HSE dashboard', icon: 'pi pi-chart-bar', routerLink: '/app/hse-dashboard', routerLinkActiveOptions: { exact: true }, roles: ['HSE'] },
      { label: 'Documents (intervention)', icon: 'pi pi-upload', routerLink: '/app/documents', routerLinkActiveOptions: { exact: true }, roles: ['USER'] },
      { label: 'My Profile', icon: 'pi pi-user', routerLink: '/app/profile', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN', 'RH', 'HSE', 'USER'] },
      {
        label: 'Interventions',
        icon: 'pi pi-clipboard',
        routerLink: '/app/interventions',
        routerLinkActiveOptions: { exact: true },
        roles: ['ADMIN', 'RH', 'HSE', 'USER'],
      },
      { label: 'New intervention', icon: 'pi pi-plus', routerLink: '/app/interventions/new', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN', 'USER'] },
      { label: 'Suppliers', icon: 'pi pi-briefcase', routerLink: '/app/suppliers', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN', 'USER'] },
      { label: 'Zones', icon: 'pi pi-map', routerLink: '/app/zones', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN'] },
      { label: 'Types of Work', icon: 'pi pi-list', routerLink: '/app/type-of-works', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN'] },
      { label: 'Personal', icon: 'pi pi-users', routerLink: '/app/personnel', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN', 'RH', 'USER'] },
      { label: 'Add personal', icon: 'pi pi-user-plus', routerLink: '/app/personnel/new', routerLinkActiveOptions: { exact: true }, roles: ['ADMIN'] },
      { label: 'Add personal', icon: 'pi pi-user-plus', routerLink: '/app/personnel/create', routerLinkActiveOptions: { exact: true }, roles: ['USER'] },
      { label: 'Blacklist requests', icon: 'pi pi-ban', routerLink: '/app/blacklist-requests', routerLinkActiveOptions: { exact: true }, roles: ['RH', 'USER'] },
    ];

    return items.filter((item) => !item.roles || this.auth.hasAnyRole(item.roles));
  }
}
