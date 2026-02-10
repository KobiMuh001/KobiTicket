import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isAdmin()) {
    return true;
  }

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  } else {
    router.navigate(['/unauthorized']);
  }
  
  return false;
};

export const staffGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isStaff()) {
    return true;
  }

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  } else if (authService.isAdmin()) {
    router.navigate(['/admin']);
  } else {
    router.navigate(['/customer']);
  }
  
  return false;
};

export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // Already logged in, redirect to appropriate dashboard
  if (authService.isAdmin()) {
    router.navigate(['/admin']);
  } else if (authService.isStaff()) {
    router.navigate(['/staff']);
  } else {
    router.navigate(['/customer']);
  }
  
  return false;
};

export const customerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isCustomer()) {
    return true;
  }

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  } else if (authService.isAdmin()) {
    router.navigate(['/admin']);
  } else if (authService.isStaff()) {
    router.navigate(['/staff']);
  }
  
  return false;
};
