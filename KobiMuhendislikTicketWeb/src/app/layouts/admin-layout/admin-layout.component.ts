import { Component, OnInit, OnDestroy, HostListener, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { SignalRService } from '../../core/services/signalr.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;
  
  isSidebarCollapsed = false;
  currentUser: User | null = null;
  
  // Notification properties
  showNotificationDropdown = false;
  notifications: Notification[] = [];
  unreadCount = 0;
  
  menuItems = [
    {
      title: 'Dashboard',
      icon: 'dashboard',
      route: '/admin/dashboard',
      active: true
    },
    {
      title: 'Talepler',
      icon: 'ticket',
      route: '/admin/tickets',
      active: false
    },
    {
      title: 'Müşteriler',
      icon: 'users',
      route: '/admin/tenants',
      active: false
    },
    {
      title: 'Ürünler',
      icon: 'products',
      route: '/admin/products',
      active: false
    },
    {
      title: 'Müşteri Ürün',
      icon: 'assets',
      route: '/admin/assets',
      active: false
    },
    {
      title: 'Personel',
      icon: 'staff',
      route: '/admin/staff',
      active: false
    },   
    {
      title: 'Ayarlar',
      icon: 'settings',
      route: '/admin/settings',
      active: false
    }
  ];

  // Logout confirmation modal state
  showLogoutConfirm = false;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    
    // Bildirimleri başlat
    this.notificationService.initializeAdminNotifications();
    
    // SignalR bağlantısını başlat
    if (this.isBrowser) {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      if (token) {
        // Notification hub bağlantısı
        this.signalRService.startNotificationConnection(token).then(() => {
          console.log('Admin Layout: SignalR Notification Hub bağlantısı kuruldu');
          
          // Bağlantı kurulduktan sonra event listener ekle
          const hub = this.signalRService.getNotificationHub();
          hub?.off('AdminNotificationReceived');
          hub?.on('AdminNotificationReceived', (notification: any) => {
            console.log('Admin Layout: Yeni bildirim alındı', notification);
            // Staff metodu kullan çünkü backend formatını handle ediyor
            this.notificationService.addStaffNotificationDirectly(notification);
          });
        }).catch(err => {
          console.error('Admin Layout: SignalR Notification Hub bağlantı hatası:', err);
        });
      }
    }
    
    // Notification service'teki değişiklikleri dinle
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
      });
    
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });
  }

  ngOnDestroy(): void {
    this.notificationService.stopPolling();
    this.signalRService.stopNotificationConnection().catch(err => {
      console.log('Admin Layout: Error stopping SignalR notification connection:', err);
    });
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Bildirimleri yükle (artık otomatik yapılıyor ama opsiyonel)
  loadNotifications(): void {
    // Artık initializeAdminNotifications tarafından yapılıyor
  }

  // Bildirim dropdown'ını aç/kapat
  toggleNotificationDropdown(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showNotificationDropdown = !this.showNotificationDropdown;
  }

  // Dropdown dışına tıklandığında kapat
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.notifications-widget')) {
      this.showNotificationDropdown = false;
    }
  }

  // Bildirimi okundu olarak işaretle ve ticket'a git
  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }
    
    if (notification.ticketId) {
      this.router.navigate(['/admin/tickets', notification.ticketId]);
    }
    
    this.showNotificationDropdown = false;
  }

  // Tüm bildirimleri okundu olarak işaretle
  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  // Bildirimi okundu olarak isaretle (buton)
  markNotificationAsRead(notificationId: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.notificationService.markAsRead(notificationId).subscribe();
  }

  // Bildirimi sil (buton)
  deleteNotification(notificationId: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.notificationService.deleteNotification(notificationId).subscribe();
  }

  // Bildirim tipine göre ikon
  getNotificationIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'NewTicket': 'ticket',
      'TicketComment': 'comment',
      'TicketStatusChanged': 'status',
      'TicketAssigned': 'assign',
      'TicketPriorityChanged': 'priority',
      'TicketResolved': 'resolved',
      'General': 'general'
    };
    return icons[type] || 'general';
  }

  // Zaman formatlama
  formatTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    
    if (diffMins < 1) return 'Şimdi';
    if (diffMins < 60) return `${diffMins} dakika önce`;
    if (diffHours < 24) return `${diffHours} saat önce`;
    if (diffDays < 7) return `${diffDays} gün önce`;
    
    return date.toLocaleDateString('tr-TR');
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

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
