import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffTicket } from '../../../core/services/staff.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-tickets.component.html',
  styleUrls: ['./my-tickets.component.scss']
})
export class MyTicketsComponent implements OnInit, OnDestroy {
  tickets: StaffTicket[] = [];
  filteredTickets: StaffTicket[] = [];
  notifications: Notification[] = [];
  isLoading = true;
  error: string | null = null;
  successMessage: string | null = null;
  
  // Filters
  selectedStatus: string = 'all';
  selectedPriority: string = 'all';
  
  // SignalR & Notifications
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;

  constructor(
    private staffService: StaffService,
    private signalRService: SignalRService,
    private notificationService: NotificationService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.loadTickets();
    // SignalR initialization (commented for debugging)
    // setTimeout(() => {
    //   this.initSignalR();
    //   this.subscribeToNotifications();
    // }, 1000);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSignalR(): void {
    if (!this.isBrowser) {
      console.log('SignalR: Not in browser environment');
      return;
    }

    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token) {
      this.signalRService.startConnection(token).catch(err => {
        console.error('SignalR bağlantı hatası:', err);
      });
    }
  }

  private subscribeToNotifications(): void {
    // Bildirimleri dinle
    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
      });

    // SignalR yorum bildirimlerini dinle
    this.signalRService.commentReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe((comment: CommentMessage) => {
        // Bu yorum bana ait bir tickete mi ait?
        const ticket = this.tickets.find(t => t.id === comment.ticketId);
        if (ticket) {
          const notification: Notification = {
            id: `notif-${Date.now()}-${Math.random()}`,
            title: `${ticket.title} - Yeni Yorum`,
            message: `${comment.authorName}: ${comment.message.substring(0, 50)}...`,
            type: 'comment',
            isRead: false,
            ticketId: comment.ticketId,
            createdDate: new Date().toISOString()
          };
          this.notificationService.addNotificationDirectly(notification);
        }
      });
  }

  loadTickets(): void {
    this.isLoading = true;
    this.staffService.getMyTickets().subscribe({
      next: (res) => {
        if (res.success) {
          this.tickets = res.data;
          this.applyFilters();
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Ticketlar yüklenirken hata oluştu';
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredTickets = this.tickets.filter(ticket => {
      const statusMatch = this.selectedStatus === 'all' || ticket.status.toString() === this.selectedStatus;
      const priorityMatch = this.selectedPriority === 'all' || ticket.priority.toString() === this.selectedPriority;
      return statusMatch && priorityMatch;
    });
  }

  onStatusFilterChange(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  onPriorityFilterChange(event: Event): void {
    this.selectedPriority = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  releaseTicket(ticketId: string, event: Event): void {
    event.stopPropagation();
    if (confirm('Bu ticketı bırakmak istediğinize emin misiniz?')) {
      this.staffService.releaseTicket(ticketId).subscribe({
        next: (res) => {
          if (res.success) {
            this.successMessage = 'Ticket başarıyla bırakıldı';
            this.loadTickets();
            setTimeout(() => this.successMessage = null, 3000);
          }
        },
        error: (err) => {
          this.error = err.error?.message || 'Ticket bırakılırken hata oluştu';
        }
      });
    }
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

  // Notification handlers
  markNotificationAsRead(notificationId: string): void {
    this.notificationService.markAsRead(notificationId);
  }

  deleteNotification(notificationId: string): void {
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      const index = this.notifications.indexOf(notification);
      if (index > -1) {
        this.notifications.splice(index, 1);
      }
    }
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

  get unreadNotificationsCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }
}
