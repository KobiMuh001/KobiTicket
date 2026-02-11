import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffWorkload, StaffTicket } from '../../../core/services/staff.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { SignalRService, StaffNotification, TicketUpdateMessage } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './staff-dashboard.component.html',
  styleUrls: ['./staff-dashboard.component.scss']
})
export class StaffDashboardComponent implements OnInit, OnDestroy {
  workload: StaffWorkload | null = null;
  myTickets: StaffTicket[] = [];
  unassignedTickets: StaffTicket[] = [];
  notifications: Notification[] = [];
  isLoading = true;
  error: string | null = null;
  showNotificationsDropdown = false;
  toastNotification: { message: string; type: string } | null = null;
  private destroy$ = new Subject<void>();
  private refreshInterval: any;

  constructor(
    private staffService: StaffService,
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.subscribeToNotifications();
    this.initializeSignalR();
    
    // Dashboard'ı 30 saniyede bir yenile (polling)
    this.refreshInterval = setInterval(() => {
      console.log('Staff Dashboard: Periodic refresh triggered');
      this.loadDashboardData();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    this.signalRService.stopNotificationConnection().catch(err => {
      console.log('Error stopping SignalR notification connection:', err);
    });
    this.signalRService.stopConnection().catch(err => {
      console.log('Error stopping SignalR main connection:', err);
    });
    this.destroy$.next();
    this.destroy$.complete();
  }

  private subscribeToNotifications(): void {
    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
      });
  }

  private initializeSignalR(): void {
    const token = this.authService.getToken();
    if (!token) {
      console.log('No token available for SignalR connection');
      return;
    }

    // Notification hub'ı bağla
    this.signalRService.startNotificationConnection(token)
      .then(() => {
        console.log('SignalR notification connection established for staff');
        this.subscribeToRealtimeNotifications();
      })
      .catch(err => {
        console.error('Failed to establish SignalR notification connection:', err);
      });

    // Main connection hub'ı bağla (TicketUpdated event'leri için)
    this.signalRService.startConnection(token)
      .then(() => {
        console.log('SignalR main connection established for staff');
        this.subscribeToTicketUpdates();
      })
      .catch(err => {
        console.error('Failed to establish SignalR main connection:', err);
      });
  }

  private subscribeToRealtimeNotifications(): void {
    this.signalRService.staffNotificationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notification: StaffNotification) => {
        console.log('Real-time notification received:', notification);
        // Yeni bildirimi service üzerinden ekle (BehaviorSubject otomatik günceller)
        const newNotification: Notification = {
          id: notification.id,
          title: notification.title,
          message: notification.message,
          type: notification.type,
          isRead: notification.isRead,
          ticketId: notification.ticketId,
          createdDate: notification.createdDate
        };
        this.notificationService.addStaffNotificationDirectly(newNotification);
        
        // Toast göster
        this.showToast(notification.title, 'info');
      });
  }

  private subscribeToTicketUpdates(): void {
    // Ticket güncellemelerini dinle (yeni ticket oluşturuldu, status değişti vs.)
    this.signalRService.ticketUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((updatedTicket: TicketUpdateMessage) => {
        console.log('Ticket update received:', updatedTicket);
        // Anlık olarak paneli yenile
        this.loadDashboardData();
      });
  }

  private showToast(message: string, type: string = 'info'): void {
    this.toastNotification = { message, type };
    setTimeout(() => {
      this.toastNotification = null;
    }, 4000); // 4 saniye sonra kaybolsun
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    // Load notifications
    this.notificationService.getStaffNotifications(20).subscribe({
      next: (res) => {
        if (res.success) {
          this.notifications = res.data;
        }
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });

    // Load workload
    this.staffService.getMyWorkload().subscribe({
      next: (res) => {
        if (res.success) {
          this.workload = res.data;
        }
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });

    // Load my tickets
    this.staffService.getMyTickets().subscribe({
      next: (res) => {
        if (res.success) {
          this.myTickets = res.data.slice(0, 5); 
        }
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });

    // Load unassigned tickets
    this.staffService.getUnassignedTickets().subscribe({
      next: (res) => {
        if (res.success) {
          this.unassignedTickets = res.data.slice(0, 5); // İlk 5
        }
        this.isLoading = false;
      },
      error: () => {
        // Production'da hata detayları gizlenir
        this.isLoading = false;
      }
    });
  }

  claimTicket(ticketId: number | string): void {
    this.staffService.claimTicket(ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadDashboardData();
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Ticket alınırken hata oluştu';
      }
    });
  }

  getStatusText(status: number): string {
    switch (status) {
      case 1: return 'Açık';
      case 2: return 'İşlemde';
      case 3: return 'Müşteri Bekliyor';
      case 4: return 'Çözüldü';
      case 5: return 'Kapalı';
      default: return 'Bilinmiyor';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-open';
      case 2: return 'status-processing';
      case 3: return 'status-waiting';
      case 4: return 'status-resolved';
      case 5: return 'status-closed';
      default: return '';
    }
  }

  getPriorityText(priority: number): string {
    switch (priority) {
      case 1: return 'Düşük';
      case 2: return 'Normal';
      case 3: return 'Yüksek';
      case 4: return 'Kritik';
      default: return 'Normal';
    }
  }

  getPriorityClass(priority: number): string {
    switch (priority) {
      case 1: return 'priority-low';
      case 2: return 'priority-normal';
      case 3: return 'priority-high';
      case 4: return 'priority-critical';
      default: return 'priority-normal';
    }
  }

  getWorkloadPercentage(): number {
    if (!this.workload || !this.workload.maxConcurrentTickets) {
      return 0;
    }
    return Math.round((this.workload.assignedTickets / this.workload.maxConcurrentTickets) * 100);
  }

  get unreadNotificationsCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  markNotificationAsRead(notificationId: string): void {
    this.notificationService.markAsRead(notificationId).subscribe({
      next: () => {
        const notification = this.notifications.find(n => n.id === notificationId);
        if (notification) {
          notification.isRead = true;
        }
      }
    });
  }

  deleteNotification(notificationId: string): void {
    this.notificationService.deleteNotification(notificationId).subscribe({
      next: () => {
        const notification = this.notifications.find(n => n.id === notificationId);
        if (notification) {
          const index = this.notifications.indexOf(notification);
          if (index > -1) {
            this.notifications.splice(index, 1);
          }
        }
      }
    });
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

  toggleNotificationsDropdown(): void {
    this.showNotificationsDropdown = !this.showNotificationsDropdown;
  }

  closeNotificationsDropdown(): void {
    this.showNotificationsDropdown = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const notificationsWidget = document.querySelector('.notifications-widget');
    if (notificationsWidget && !notificationsWidget.contains(target)) {
      this.closeNotificationsDropdown();
    }
  }
}
