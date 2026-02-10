import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { Subject, interval } from 'rxjs';
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
      title: 'Ticketlar',
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
      title: 'Varlıklar',
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

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    
    // Bildirimleri yükle
    this.loadNotifications();
    
    // Her 30 saniyede bir bildirimleri kontrol et
    interval(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadNotifications();
      });
    
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
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Bildirimleri yükle
  loadNotifications(): void {
    this.notificationService.getNotifications(10).subscribe({
      error: () => {
        
      }
    });
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
    if (!target.closest('.notification-container')) {
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

  logout(): void {
    this.authService.logout();
  }
}
