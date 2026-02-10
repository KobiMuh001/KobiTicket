import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, LoginRequest, StaffLoginRequest } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  activeTab: 'user' | 'staff' = 'user';
  
  loginData: LoginRequest = {
    identifier: '',
    password: ''
  };
  
  staffLoginData: StaffLoginRequest = {
    email: '',
    password: ''
  };
  
  isLoading = false;
  errorMessage = '';
  infoMessage = '';
  showPassword = false;
  rememberMe = false;
  returnUrl = '/admin';

  // Rate Limiting - Brute Force koruması
  private failedAttempts = 0;
  private lockoutUntil: Date | null = null;
  private readonly MAX_ATTEMPTS = 5;
  private readonly LOCKOUT_DURATION_MS = 60000; // 1 dakika

  // Güvenli yönlendirme için izin verilen rotalar
  private allowedRoutes = ['/admin', '/customer', '/staff'];

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    const requestedUrl = this.route.snapshot.queryParams['returnUrl'];
    this.returnUrl = this.sanitizeReturnUrl(requestedUrl);
  }

  ngOnInit(): void {
    // Token expired veya unauthorized durumlarını kontrol et
    const queryParams = this.route.snapshot.queryParams;
    if (queryParams['expired'] === 'true') {
      this.infoMessage = 'Oturumunuz sona erdi. Lütfen tekrar giriş yapın.';
    } else if (queryParams['unauthorized'] === 'true') {
      this.infoMessage = 'Yetkilendirme hatası. Lütfen tekrar giriş yapın.';
    }
  }

  // Rate limiting kontrolü
  private isLockedOut(): boolean {
    if (this.lockoutUntil && new Date() < this.lockoutUntil) {
      const remainingSeconds = Math.ceil((this.lockoutUntil.getTime() - new Date().getTime()) / 1000);
      this.errorMessage = `Çok fazla başarısız deneme. ${remainingSeconds} saniye sonra tekrar deneyin.`;
      return true;
    }
    // Lockout süresi dolmuşsa sıfırla
    if (this.lockoutUntil && new Date() >= this.lockoutUntil) {
      this.failedAttempts = 0;
      this.lockoutUntil = null;
    }
    return false;
  }

  private handleFailedLogin(): void {
    this.failedAttempts++;
    if (this.failedAttempts >= this.MAX_ATTEMPTS) {
      this.lockoutUntil = new Date(Date.now() + this.LOCKOUT_DURATION_MS);
      this.errorMessage = `Çok fazla başarısız deneme. 60 saniye sonra tekrar deneyin.`;
    }
  }

  private resetFailedAttempts(): void {
    this.failedAttempts = 0;
    this.lockoutUntil = null;
  }

  // Open Redirect açığını önlemek için URL doğrulama
  private sanitizeReturnUrl(url: string | undefined): string {
    if (!url) return '/admin';
    
    // Sadece iç rotaları kabul et (/ ile başlamalı, // ile başlamamalı)
    if (!url.startsWith('/') || url.startsWith('//')) {
      return '/admin';
    }
    
    // Protokol içeren URL'leri reddet (javascript:, http:, https: vb.)
    if (url.includes(':')) {
      return '/admin';
    }
    
    // İzin verilen rota önekleriyle başlayıp başlamadığını kontrol et
    const isAllowed = this.allowedRoutes.some(route => url.startsWith(route));
    return isAllowed ? url : '/admin';
  }

  switchTab(tab: 'user' | 'staff'): void {
    this.activeTab = tab;
    this.errorMessage = '';
  }

  onSubmit(): void {
    if (this.activeTab === 'user') {
      this.submitUserLogin();
    } else {
      this.submitStaffLogin();
    }
  }

  submitUserLogin(): void {
    // Rate limiting kontrolü
    if (this.isLockedOut()) return;

    if (!this.loginData.identifier || !this.loginData.password) {
      this.errorMessage = 'Lütfen tüm alanları doldurunuz.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData, this.rememberMe).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.resetFailedAttempts(); // Başarılı giriş - sayacı sıfırla
        // Token'dan role bilgisini al
        const token = response.token;
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || 'Customer';
          
          if (role === 'Admin') {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/customer']);
          }
        } catch {
          this.router.navigate(['/customer']);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.handleFailedLogin(); // Başarısız giriş - sayacı artır
        if (!this.lockoutUntil) {
          this.errorMessage = error.error?.message || 'Giriş bilgileri hatalı!';
        }
      }
    });
  }

  submitStaffLogin(): void {
    // Rate limiting kontrolü
    if (this.isLockedOut()) return;

    if (!this.staffLoginData.email || !this.staffLoginData.password) {
      this.errorMessage = 'Lütfen tüm alanları doldurunuz.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.staffLogin(this.staffLoginData, this.rememberMe).subscribe({
      next: () => {
        this.isLoading = false;
        this.resetFailedAttempts(); // Başarılı giriş - sayacı sıfırla
        this.router.navigate(['/staff']);
      },
      error: (error) => {
        this.isLoading = false;
        this.handleFailedLogin(); // Başarısız giriş - sayacı artır
        if (!this.lockoutUntil) {
          this.errorMessage = error.error?.message || 'Staff giriş bilgileri hatalı!';
        }
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
