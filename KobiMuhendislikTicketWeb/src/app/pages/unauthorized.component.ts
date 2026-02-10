import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="unauthorized-page">
      <div class="content">
        <div class="icon">
          <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="12" cy="12" r="10"/>
            <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/>
          </svg>
        </div>
        <h1>Erişim Reddedildi</h1>
        <p>Bu sayfaya erişim yetkiniz bulunmamaktadır.</p>
        <div class="actions">
          <a [routerLink]="getHomeRoute()" class="btn btn-primary">Ana Sayfaya Dön</a>
          <button (click)="logout()" class="btn btn-secondary">Çıkış Yap</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .unauthorized-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #f5f7fa 0%, #e4e8eb 100%);
    }

    .content {
      text-align: center;
      padding: 3rem;
      background: white;
      border-radius: 16px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      max-width: 450px;
    }

    .icon {
      color: #dc2626;
      margin-bottom: 1.5rem;
    }

    h1 {
      font-size: 1.75rem;
      color: #1e293b;
      margin-bottom: 0.75rem;
    }

    p {
      color: #64748b;
      margin-bottom: 2rem;
    }

    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .btn {
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-weight: 500;
      text-decoration: none;
      cursor: pointer;
      border: none;
      font-size: 0.9rem;
      transition: all 0.2s;
    }

    .btn-primary {
      background: #0066cc;
      color: white;
    }

    .btn-primary:hover {
      background: #0052a3;
    }

    .btn-secondary {
      background: #f1f5f9;
      color: #475569;
    }

    .btn-secondary:hover {
      background: #e2e8f0;
    }
  `]
})
export class UnauthorizedComponent {
  constructor(private authService: AuthService) {}

  getHomeRoute(): string {
    if (this.authService.isAdmin()) return '/admin';
    if (this.authService.isStaff()) return '/staff';
    if (this.authService.isCustomer()) return '/customer';
    return '/login';
  }

  logout(): void {
    this.authService.logout();
  }
}
