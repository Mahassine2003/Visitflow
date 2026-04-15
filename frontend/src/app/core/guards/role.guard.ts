import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const roles = route.data?.['roles'] as string[] | undefined;

  if (!roles || roles.length === 0) {
    return true;
  }

  const hasRole = roles.some((role) => auth.hasRole(role));

  if (hasRole) {
    return true;
  }

  return router.createUrlTree([auth.getDefaultRoute()], {
    queryParams: { deniedFrom: state.url },
  });
};
