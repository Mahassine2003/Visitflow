import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  username: string;
  role: string;
}

export type AppRole = 'ADMIN' | 'HSE' | 'RH' | 'USER';

export interface User {
  id: string;
  email: string;
  role: string;
  supplierId?: string | null;
  token?: string;
  fullName?: string | null;
}

const TOKEN_KEY = 'auth_token';
const ROLE_KEY = 'auth_role';
const USER_ID_KEY = 'auth_user_id';
const SUPPLIER_ID_KEY = 'auth_supplier_id';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _user = signal<User | null>(null);
  readonly user = this._user.asReadonly();

  constructor(private router: Router, private http: HttpClient) {
    const token = localStorage.getItem(TOKEN_KEY);
    const role = localStorage.getItem(ROLE_KEY);
    const userId = localStorage.getItem(USER_ID_KEY);
    const supplierId = localStorage.getItem(SUPPLIER_ID_KEY);

    if (token && role && userId) {
      this._user.set({
        id: userId,
        email: '', // peut être rempli après un appel profil
        role,
        supplierId,
        token,
      });
    }
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', credentials).pipe(
      tap((response) => {
        const token = response.accessToken;
        const role = this.normalizeRole(response.role);
        const jwt = this.tryDecodeJwt(token);
        const userId = ((jwt?.['sub'] as string | number | undefined) ?? response.username ?? '').toString() || response.username;
        const fullName =
          (jwt?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string | undefined) ??
          (jwt?.['name'] as string | undefined) ??
          null;

        localStorage.setItem(TOKEN_KEY, token);
        localStorage.setItem(ROLE_KEY, role);
        localStorage.setItem(USER_ID_KEY, userId);

        this._user.set({
          id: userId,
          email: credentials.email,
          role,
          token,
          fullName,
        });
      })
    );
  }

  register(payload: {
    name: string;
    email: string;
    password: string;
    role?: AppRole;
  }): Observable<AuthResponse> {
    const body = {
      fullName: payload.name,
      username: payload.name,
      email: payload.email,
      password: payload.password,
      role: payload.role ?? 'USER',
    };

    return this.http.post<AuthResponse>('/api/auth/register', body);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(USER_ID_KEY);
    localStorage.removeItem(SUPPLIER_ID_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!this._user();
  }

  hasRole(role: string): boolean {
    const current = this._user();
    return !!current && current.role === this.normalizeRole(role);
  }

  hasAnyRole(roles: string[]): boolean {
    const current = this._user();
    if (!current) return false;
    return roles.map((r) => this.normalizeRole(r)).includes(current.role as AppRole);
  }

  getRole(): AppRole | null {
    const role = this._user()?.role;
    return role ? this.normalizeRole(role) : null;
  }

  getDefaultRoute(): string {
    const role = this.getRole();
    if (role === 'HSE') {
      return '/app/hse-dashboard';
    }
    return '/app/interventions';
  }

  getToken(): string | null {
    return this._user()?.token ?? localStorage.getItem(TOKEN_KEY);
  }

  private normalizeRole(role: string | null | undefined): AppRole {
    const normalized = (role ?? '').toUpperCase();
    switch (normalized) {
      case 'ADMIN':
      case 'HSE':
      case 'RH':
      case 'USER':
        return normalized;
      default:
        return 'USER';
    }
  }

  private tryDecodeJwt(token: string): Record<string, unknown> | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const payload = parts[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = normalized + '='.repeat((4 - (normalized.length % 4)) % 4);
      const json = decodeURIComponent(
        atob(padded)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(json) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}
