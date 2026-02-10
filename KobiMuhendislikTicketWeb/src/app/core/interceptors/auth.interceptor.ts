import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

/**
 * Token geçerliliğini kontrol eder
 */
function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expiry = payload.exp * 1000;
    // 30 saniye buffer ekle - token neredeyse dolmuşsa expired say
    return Date.now() > (expiry - 30000);
  } catch {
    return true;
  }
}

/**
 * Expired token ve storage'ı temizler
 */
function clearAuthData(): void {
  localStorage.removeItem('token');
  localStorage.removeItem('companyName');
  localStorage.removeItem('rememberMe');
  localStorage.removeItem('staffId');
  localStorage.removeItem('fullName');
  localStorage.removeItem('department');
  sessionStorage.removeItem('token');
  sessionStorage.removeItem('companyName');
  sessionStorage.removeItem('staffId');
  sessionStorage.removeItem('fullName');
  sessionStorage.removeItem('department');
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const platformId = inject(PLATFORM_ID);
  const router = inject(Router);
  
  if (isPlatformBrowser(platformId)) {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    
    if (token) {
      // Token expired kontrolü - login endpoint'i hariç
      if (!req.url.includes('/auth/') && isTokenExpired(token)) {
        clearAuthData();
        router.navigate(['/login'], { 
          queryParams: { expired: 'true' } 
        });
        return throwError(() => new Error('Token expired'));
      }

      const clonedReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      
      return next(clonedReq).pipe(
        catchError((error: HttpErrorResponse) => {
          // 401 Unauthorized - token geçersiz veya expired
          if (error.status === 401) {
            clearAuthData();
            router.navigate(['/login'], { 
              queryParams: { unauthorized: 'true' } 
            });
          }
          // 403 Forbidden - yetkisiz erişim
          if (error.status === 403) {
            router.navigate(['/unauthorized']);
          }
          return throwError(() => error);
        })
      );
    }
  }
  
  return next(req);
};
