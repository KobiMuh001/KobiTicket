import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffTicket } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
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
  ticketNotifications: Map<number, number> = new Map(); // ticket id -> unread count
  isLoading = true;
  error: string | null = null;
  successMessage: string | null = null;
  
  // Pagination
  currentPage = 1;
  itemsPerPage = 20;
  totalPages = 1;
  
  // Filters
  selectedStatus: string = 'all';
  selectedPriority: string = 'all';
  selectedCustomer: string = 'all';
  customers: string[] = [];
  
  // SignalR & Notifications
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;
  statusOptions: Array<any> = [];
  priorityOptions: Array<any> = [];

  constructor(
    private staffService: StaffService,
    private signalRService: SignalRService,
    private paramSvc: SystemParameterService,
    private notificationService: NotificationService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.loadTickets();
    // Ativar SignalR para notificações em tempo real
    setTimeout(() => {
      this.initSignalR();
      this.subscribeToNotifications();
    }, 1000);
    this.loadLookups();
  }

  loadLookups(): void {
    // Load TicketStatus
    const paramSvc = (this as any).paramSvc as SystemParameterService | undefined;
    if (!paramSvc) return;
    paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const sData = res?.data?.data || res?.data || res || [];
        this.statusOptions = (Array.isArray(sData) ? sData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.statusOptions = []; }
    });
    paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const pData = res?.data?.data || res?.data || res || [];
        this.priorityOptions = (Array.isArray(pData) ? pData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.priorityOptions = []; }
    });
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
        this.updateTicketNotificationCounts();
      });

    // SignalR yorum bildirimlerini dinle
    this.signalRService.commentReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe((comment: CommentMessage) => {
        // Bu yorum bana ait bir tickete mi ait?
        const commentTicketId = Number(comment.ticketId);
        const ticket = this.tickets.find(t => t.id === commentTicketId);
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
          this.updateTicketNotificationCounts();
        }
      });
  }

  private updateTicketNotificationCounts(): void {
    this.ticketNotifications.clear();
    this.notifications.forEach(notif => {
      if (notif.ticketId && !notif.isRead) {
        const ticketId = Number(notif.ticketId);
        const count = this.ticketNotifications.get(ticketId) || 0;
        this.ticketNotifications.set(ticketId, count + 1);
      }
    });
  }

  getTicketNotificationCount(ticketId: number): number {
    return this.ticketNotifications.get(ticketId) || 0;
  }

  loadTickets(): void {
    this.isLoading = true;
    this.staffService.getMyTickets(this.currentPage, this.itemsPerPage).subscribe({
      next: (res) => {
        if (res.success) {
          this.tickets = res.data;
          this.extractCustomers();
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

  extractCustomers(): void {
    const uniqueCustomers = new Set<string>();
    this.tickets.forEach(ticket => {
      if (ticket.companyName) {
        uniqueCustomers.add(ticket.companyName);
      }
    });
    this.customers = Array.from(uniqueCustomers).sort();
  }

  applyFilters(): void {
    this.filteredTickets = this.tickets.filter(ticket => {
      const statusMatch = this.selectedStatus === 'all' || ticket.status.toString() === this.selectedStatus;
      const priorityMatch = this.selectedPriority === 'all' || ticket.priority.toString() === this.selectedPriority;
      const customerMatch = this.selectedCustomer === 'all' || ticket.companyName === this.selectedCustomer;
      return statusMatch && priorityMatch && customerMatch;
    });
    
    // Sort: active tickets first, then resolved/closed at the end
    this.filteredTickets.sort((a, b) => {
      const aIsResolved = a.status === 4 || a.status === 5;
      const bIsResolved = b.status === 4 || b.status === 5;
      
      if (aIsResolved && !bIsResolved) return 1;
      if (!aIsResolved && bIsResolved) return -1;
      return 0;
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

  onCustomerFilterChange(event: Event): void {
    this.selectedCustomer = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  // Release (leave) ticket flow now uses a centered confirmation modal.
  showReleaseConfirm = false;
  ticketToReleaseId: number | null = null;
  ticketToReleaseTitle: string | null = null;

  openReleaseConfirm(ticketId: number | string, event?: Event, title?: string): void {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.ticketToReleaseId = Number(ticketId);
    this.ticketToReleaseTitle = title || null;
    this.showReleaseConfirm = true;
  }

  closeReleaseConfirm(): void {
    this.ticketToReleaseId = null;
    this.ticketToReleaseTitle = null;
    this.showReleaseConfirm = false;
  }

  confirmRelease(): void {
    if (!this.ticketToReleaseId) return;
    const id = this.ticketToReleaseId;
    this.staffService.releaseTicket(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Ticket başarıyla bırakıldı';
          this.loadTickets();
          setTimeout(() => this.successMessage = null, 3000);
        }
        this.closeReleaseConfirm();
      },
      error: (err) => {
        this.error = err.error?.message || 'Ticket bırakılırken hata oluştu';
        this.closeReleaseConfirm();
      }
    });
  }

  getStatusText(status: number): string {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) return found.label || 'Bilinmiyor';

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
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'status-open';
        case 2: return 'status-processing';
        case 3: return 'status-waiting';
        case 4: return 'status-resolved';
        case 5: return 'status-closed';
        default: return '';
      }
    }
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
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) return found.label || 'Normal';

    switch (priority) {
      case 1: return 'Düşük';
      case 2: return 'Normal';
      case 3: return 'Yüksek';
      case 4: return 'Kritik';
      default: return 'Normal';
    }
  }

  getPriorityClass(priority: number): string {
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'priority-low';
        case 2: return 'priority-normal';
        case 3: return 'priority-high';
        case 4: return 'priority-critical';
        default: return 'priority-normal';
      }
    }
    switch (priority) {
      case 1: return 'priority-low';
      case 2: return 'priority-normal';
      case 3: return 'priority-high';
      case 4: return 'priority-critical';
      default: return 'priority-normal';
    }
  }

  getStatusColor(status: string | number): string | null {
    const s = String(status ?? '');
    const found = this.statusOptions.find((o: any) => String(o.sortOrder ?? o.id) === s || String(o.id) === s || String(o.key) === s || o.label === status || String(o.label) === s);
    return found?.color ?? null;
  }

  getPriorityColor(priority: string | number): string | null {
    const p = String(priority ?? '');
    const found = this.priorityOptions.find((o: any) => String(o.sortOrder ?? o.id) === p || String(o.id) === p || String(o.key) === p || o.label === priority || String(o.label) === p);
    return found?.color ?? null;
  }

  // Notification handlers
  markNotificationAsRead(notificationId: string): void {
    this.notificationService.markAsRead(notificationId).subscribe({
      next: () => {
        const notification = this.notifications.find(n => n.id === notificationId);
        if (notification) {
          notification.isRead = true;
          this.updateTicketNotificationCounts();
        }
      }
    });
  }

  deleteNotification(notificationId: string): void {
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      const index = this.notifications.indexOf(notification);
      if (index > -1) {
        this.notifications.splice(index, 1);
        this.updateTicketNotificationCounts();
      }
    }
  }

  onTicketSelect(ticketId: number): void {
    // Seçilen ticketa ait tüm okunmamış bildirimleri okundu olarak işaretle
    const ticketNotifications = this.notifications.filter(
      notif => notif.ticketId && Number(notif.ticketId) === ticketId && !notif.isRead
    );
    
    ticketNotifications.forEach(notif => {
      this.notificationService.markAsRead(notif.id).subscribe({
        next: () => {
          notif.isRead = true;
        }
      });
    });
    
    this.updateTicketNotificationCounts();
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

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTickets();
    }
  }

  nextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  previousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  get paginatedTickets(): StaffTicket[] {
    return this.filteredTickets;
  }
}