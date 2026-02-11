import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  identifier: string;
  password: string;
}

export interface StaffLoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  companyName: string;
}

export interface StaffLoginResponse {
  token: string;
  staffId: string;
  fullName: string;
  department: string;
}

export interface User {
  identifier: string;
  companyName: string;
  role: string;
  staffId?: string;
  fullName?: string;
  department?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private isBrowser: boolean;

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    this.loadUserFromToken();
  }

  private loadUserFromToken(): void {
    if (!this.isBrowser) return;
    
    // Once localStorage'a bak, yoksa sessionStorage'a bak
    let token = localStorage.getItem('token');
    let companyName = localStorage.getItem('companyName');
    
    if (!token) {
      token = sessionStorage.getItem('token');
      companyName = sessionStorage.getItem('companyName');
    }
    
    if (token) {
      const user = this.decodeToken(token);
      if (companyName) {
        user.companyName = companyName;
      }
      this.currentUserSubject.next(user);
    }
  }

  login(request: LoginRequest, rememberMe: boolean = false): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request).pipe(
      tap((response) => {
        if (this.isBrowser) {
          // Beni hatirla seciliyse localStorage, degilse sessionStorage kullan
          const storage = rememberMe ? localStorage : sessionStorage;
          storage.setItem('token', response.token);
          storage.setItem('companyName', response.companyName);
          // Hangi storage kullanildigini kaydet
          localStorage.setItem('rememberMe', rememberMe ? 'true' : 'false');
        }
        const user = this.decodeToken(response.token);
        user.companyName = response.companyName;
        this.currentUserSubject.next(user);
      })
    );
  }

  staffLogin(request: StaffLoginRequest, rememberMe: boolean = false): Observable<StaffLoginResponse> {
    return this.http.post<StaffLoginResponse>(`${this.apiUrl}/auth/staff/login`, request).pipe(
      tap((response) => {
        if (this.isBrowser) {
          const storage = rememberMe ? localStorage : sessionStorage;
          storage.setItem('token', response.token);
          storage.setItem('staffId', response.staffId);
          storage.setItem('fullName', response.fullName);
          storage.setItem('department', response.department);
          localStorage.setItem('rememberMe', rememberMe ? 'true' : 'false');
        }
        const user = this.decodeToken(response.token);
        user.staffId = response.staffId;
        user.fullName = response.fullName;
        user.department = response.department;
        this.currentUserSubject.next(user);
      })
    );
  }

  logout(): void {
    // Çıkış yapmadan önce kullanıcının rolünü kontrol et
    const currentUser = this.currentUserSubject.value;
    const isAdminOrStaff = currentUser?.role === 'Admin' || currentUser?.role === 'Staff';
    
    if (this.isBrowser) {
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
    this.currentUserSubject.next(null);
    
    // Admin veya Staff ise staff-login'e, customer ise login'e yönlendir
    this.router.navigate([isAdminOrStaff ? '/staff-login' : '/login']);
  }

  getToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('token') || sessionStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      return Date.now() < expiry;
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === 'Admin';
  }

  isStaff(): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === 'Staff';
  }

  isCustomer(): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === 'Customer';
  }

  private decodeToken(token: string): User {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        identifier: payload.sub || payload.nameid,
        companyName: '',
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || 'Customer',
        staffId: payload.StaffId,
        fullName: payload.FullName
      };
    } catch {
      return { identifier: '', companyName: '', role: 'Customer' };
    }
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getStaffId(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('staffId') || sessionStorage.getItem('staffId');
  }
}
