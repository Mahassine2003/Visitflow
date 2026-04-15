import { Routes } from '@angular/router';
import { ActorLayoutComponent } from './layouts/actor-layout/actor-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { LoginComponent } from './pages/auth/login/login.component';
import { AuthLayoutComponent } from './pages/auth/auth-layout/auth-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      {
        path: 'login',
        component: LoginComponent,
        data: { animation: 'authLogin' },
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./pages/auth/register/register.component').then(
            (m) => m.RegisterComponent
          ),
        data: { animation: 'authRegister' },
      },
    ],
  },

  {
    path: 'app',
    component: ActorLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'hse-dashboard',
        canActivate: [roleGuard],
        data: { roles: ['HSE'] },
        loadComponent: () =>
          import('./pages/hse/hse-dashboard/hse-dashboard.component').then(
            (m) => m.HseDashboardComponent
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/profile/profile.component').then(
            (m) => m.ProfileComponent
          ),
      },
      {
        path: 'interventions',
        loadComponent: () =>
          import('./pages/admin/interventions-list/interventions-list.component').then(
            (m) => m.InterventionsListComponent
          ),
      },
      {
        path: 'interventions/new',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN', 'USER'] },
        loadComponent: () =>
          import('./pages/admin/intervention-wizard/intervention-wizard.component').then(
            (m) => m.InterventionWizardComponent
          ),
      },
      {
        path: 'suppliers',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN', 'USER'] },
        loadComponent: () =>
          import('./pages/admin/supplier-list/supplier-list.component').then(
            (m) => m.SupplierListComponent
          ),
      },
      {
        path: 'suppliers/new',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN', 'USER'], mode: 'create' },
        loadComponent: () =>
          import('./pages/admin/supplier-legacy-redirect/supplier-legacy-redirect.component').then(
            (m) => m.SupplierLegacyRedirectComponent
          ),
      },
      {
        path: 'suppliers/:id',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN', 'USER'], mode: 'detail' },
        loadComponent: () =>
          import('./pages/admin/supplier-legacy-redirect/supplier-legacy-redirect.component').then(
            (m) => m.SupplierLegacyRedirectComponent
          ),
      },
      {
        path: 'zones',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
        loadComponent: () =>
          import('./pages/admin/zones-list/zones-list.component').then(
            (m) => m.ZonesListComponent
          ),
      },
      {
        path: 'type-of-works',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
        loadComponent: () =>
          import('./pages/admin/type-of-works-list/type-of-works-list.component').then(
            (m) => m.TypeOfWorksListComponent
          ),
      },
      {
        path: 'documents',
        canActivate: [roleGuard],
        data: { roles: ['USER'] },
        loadComponent: () =>
          import('./pages/supplier/documents-upload/documents-upload.component').then(
            (m) => m.DocumentsUploadComponent
          ),
      },
      {
        path: 'my-interventions',
        redirectTo: 'interventions',
        pathMatch: 'full',
      },
      {
        path: 'personnel',
        loadComponent: () =>
          import('./pages/supplier/personnel-list/personnel-list.component').then(
            (m) => m.PersonnelListComponent
          ),
      },
      {
        path: 'personnel/create',
        canActivate: [roleGuard],
        data: { roles: ['USER'] },
        loadComponent: () =>
          import('./pages/supplier/personnel-new/personnel-new.component').then(
            (m) => m.PersonnelNewComponent
          ),
      },
      {
        path: 'personnel/new',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
        loadComponent: () =>
          import('./pages/admin/personnel-wizard/personnel-wizard.component').then(
            (m) => m.PersonnelWizardComponent
          ),
      },
      {
        path: 'blacklist-requests',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN', 'RH', 'USER'] },
        loadComponent: () =>
          import('./pages/rh/blacklist-requests/blacklist-requests.component').then(
            (m) => m.BlacklistRequestsComponent
          ),
      },
      { path: '', redirectTo: 'interventions', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
