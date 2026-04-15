import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  const isAuthCall =
    req.url.includes('/api/auth/login') || req.url.includes('/api/auth/register');

  if (!token) {
    // No token: let guards handle navigation. Avoid forcing logout loops on public pages.
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(cloned).pipe(
    catchError((err: unknown) => {
      if (!isAuthCall && err instanceof HttpErrorResponse && err.status === 401) {
        authService.logout();
      }
      return throwError(() => err);
    })
  );
};

