import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, StaffLoginRequest } from '../../core/services/auth.service';

@Component({
  selector: 'app-staff-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff-login.component.html',
  styleUrls: ['./staff-login.component.scss']
})
export class StaffLoginComponent implements OnInit {
  staffLoginData: StaffLoginRequest = {
    email: '',
    password: ''
  };
  
  isLoading = false;
  errorMessage = '';
  infoMessage = '';
  showPassword = false;
  rememberMe = false;
  returnUrl = '/staff/dashboard';

  // Rate Limiting - Brute Force koruması
  private failedAttempts = 0;
  private lockoutUntil: Date | null = null;
  private readonly MAX_ATTEMPTS = 5;
  private readonly LOCKOUT_DURATION_MS = 60000; // 1 dakika

  // Güvenli yönlendirme için izin verilen rotalar
  private allowedRoutes = ['/staff/dashboard', '/staff', '/admin/dashboard', '/admin'];

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
    if (!url) return '/staff/dashboard';
    
    // Sadece iç rotaları kabul et (/ ile başlamalı, // ile başlamamalı)
    if (!url.startsWith('/') || url.startsWith('//')) {
      return '/staff/dashboard';
    }
    
    // Protokol içeren URL'leri reddet (javascript:, http:, https: vb.)
    if (url.includes(':')) {
      return '/staff/dashboard';
    }
    
    // İzin verilen rota önekleriyle başlayıp başlamadığını kontrol et
    const isAllowed = this.allowedRoutes.some(route => url.startsWith(route));
    return isAllowed ? url : '/staff/dashboard';
  }

  onSubmit(): void {
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
        
        // Hem admin hem staff giriş yapabilir
        const user = this.authService.getCurrentUser();
        if (user?.role === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else if (user?.role === 'Staff') {
          this.router.navigate(['/staff/dashboard']);
        } else {
          // Geçersiz role
          this.errorMessage = 'Geçersiz kullanıcı rolü';
          this.authService.logout();
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

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  goToCustomerLogin(): void {
    this.router.navigate(['/login']);
  }
}
