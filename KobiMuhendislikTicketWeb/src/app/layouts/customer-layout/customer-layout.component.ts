import { Component, OnInit, OnDestroy, HostListener, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';
import { TenantService } from '../../core/services/tenant.service';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { SignalRService } from '../../core/services/signalr.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-layout.component.html',
  styleUrls: ['./customer-layout.component.scss']
})
export class CustomerLayoutComponent implements OnInit, OnDestroy {
  isSidebarCollapsed = false;
  currentUser: User | null = null;
  customerLogoUrl: string | null = null;
  private readonly apiOrigin = new URL(environment.apiUrl).origin;

  notifications: Notification[] = [];
  showNotificationsDropdown = false;
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;
  
  menuItems = [
    {
      title: 'Ana Sayfa',
      icon: 'home',
      route: '/customer/dashboard'
    },
    {
      title: 'Ticketlarım',
      icon: 'ticket',
      route: '/customer/tickets'
    },
    {
      title: 'Varlıklarım',
      icon: 'assets',
      route: '/customer/assets'
    },
    {
      title: 'Profilim',
      icon: 'profile',
      route: '/customer/profile'
    }
  ];

  constructor(
    private authService: AuthService,
    private tenantService: TenantService,
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user?.role === 'Customer') {
          this.loadCustomerLogo();
        } else {
          this.customerLogoUrl = null;
        }
      });

    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(list => {
        // Sadece customer için istenen tipleri göster
        this.notifications = (list || []).filter(n =>
          n.type === 'TicketStatusChanged' || n.type === 'TicketComment'
        );
      });

    // Customer bildirimlerini başlat (polling)
    this.notificationService.initializeCustomerNotifications();

    // SignalR real-time customer bildirimleri
    if (this.isBrowser) {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      if (token) {
        this.signalRService.startNotificationConnection(token).then(() => {
          const hub = this.signalRService.getNotificationHub();
          hub?.off('CustomerNotificationReceived');
          hub?.on('CustomerNotificationReceived', (notification: any) => {
            this.notificationService.addStaffNotificationDirectly(notification);
          });
        }).catch(err => {
          console.error('Customer Layout: SignalR Notification Hub bağlantı hatası:', err);
        });
      }
    }
  }

  ngOnDestroy(): void {
    this.notificationService.stopPolling();
    this.signalRService.stopNotificationConnection().catch(() => {});
    this.destroy$.next();
    this.destroy$.complete();
  }

  get unreadNotificationsCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  toggleNotificationsDropdown(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showNotificationsDropdown = !this.showNotificationsDropdown;
  }

  closeNotificationsDropdown(): void {
    this.showNotificationsDropdown = false;
  }

  markNotificationAsRead(notificationId: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.notificationService.markAsRead(notificationId).subscribe();
  }

  deleteNotification(notificationId: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.notificationService.deleteNotification(notificationId).subscribe();
  }

  markAllNotificationsAsRead(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.notificationService.markAllAsRead().subscribe();
  }

  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }

    if (notification.ticketId) {
      this.router.navigate(['/customer/tickets', notification.ticketId]);
    }

    this.closeNotificationsDropdown();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const notificationsWidget = document.querySelector('.notifications-widget');
    if (notificationsWidget && !notificationsWidget.contains(target)) {
      this.closeNotificationsDropdown();
    }
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  logout(): void {
    this.authService.logout();
  }

  private loadCustomerLogo(): void {
    this.tenantService.getMyProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const data = response?.data || response;
          this.customerLogoUrl = this.toAbsoluteUrl(data?.logoUrl);
        },
        error: () => {
          this.customerLogoUrl = null;
        }
      });
  }

  private toAbsoluteUrl(path?: string | null): string | null {
    if (!path) {
      return null;
    }

    if (path.startsWith('http')) {
      return path;
    }

    return `${this.apiOrigin}${path}`;
  }
}
