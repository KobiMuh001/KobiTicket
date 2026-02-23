import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID, HostListener } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { SignalRService } from '../../core/services/signalr.service';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-staff-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './staff-layout.component.html',
  styleUrls: ['./staff-layout.component.scss']
})
export class StaffLayoutComponent implements OnInit, OnDestroy {
  isSidebarCollapsed = false;
  currentUser: User | null = null;
  unreadCount$: Observable<number>;
  unreadCount: number = 0;
  notifications: Notification[] = [];
  showNotificationsDropdown = false;
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;
  
  menuItems = [
    {
      title: 'Dashboard',
      icon: 'dashboard',
      route: '/staff/dashboard'
    },
    {
      title: 'Taleplerim',
      icon: 'my-tickets',
      route: '/staff/my-tickets'
    },
    {
      title: 'Açık Talepler',
      icon: 'open-tickets',
      route: '/staff/open-tickets'
    },
    {
      title: 'Profilim',
      icon: 'profile',
      route: '/staff/profile'
    }
  ];

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.unreadCount$ = this.notificationService.unreadCount$;
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    
    // Unread count'u subscribe et
    this.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });

    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(list => {
        this.notifications = list;
      });
    
    // Bildirimleri başlat
    this.notificationService.initializeStaffNotifications();
    
    // SignalR bağlantısını başlat
    if (this.isBrowser) {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      if (token) {
        // Notification hub bağlantısı
        this.signalRService.startNotificationConnection(token).then(() => {
          console.log('Staff Layout: SignalR Notification Hub bağlantısı kuruldu');
        }).catch(err => {
          console.error('Staff Layout: SignalR Notification Hub bağlantı hatası:', err);
        });
      }
      
      // SignalR bildirimlerini dinle
      this.signalRService.staffNotificationReceived$
        .pipe(takeUntil(this.destroy$))
        .subscribe(notification => {
          console.log('Staff Layout: Yeni bildirim alındı', notification);
          this.notificationService.addStaffNotificationDirectly(notification);
        });
    }
  }

  ngOnDestroy(): void {
    this.notificationService.stopPolling();
    this.signalRService.stopNotificationConnection().catch(err => {
      console.log('Staff Layout: Error stopping SignalR notification connection:', err);
    });
    this.destroy$.next();
    this.destroy$.complete();
  }

  get unreadNotificationsCount(): number {
    return this.unreadCount;
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
      this.router.navigate(['/staff/tickets', notification.ticketId]);
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

  // Logout confirmation modal state and handlers
  showLogoutConfirm = false;

  openLogoutConfirm(): void {
    this.showLogoutConfirm = true;
  }

  closeLogoutConfirm(): void {
    this.showLogoutConfirm = false;
  }

  confirmLogout(): void {
    this.showLogoutConfirm = false;
    this.authService.logout();
  }
}
