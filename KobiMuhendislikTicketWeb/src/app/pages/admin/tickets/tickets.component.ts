import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService, TicketListItem } from '../../../core/services/dashboard.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.scss']
})
export class TicketsComponent implements OnInit {
  tickets: TicketListItem[] = [];
  filteredTickets: TicketListItem[] = [];
  isLoading = true;
  errorMessage = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 1;
  
  // Filters
  searchTerm = '';
  statusFilter = '';
  priorityFilter = '';
  customerFilter = '';
  customers: string[] = [];

  // Notifications
  notifications: Notification[] = [];
  ticketNotifications: Map<number, number> = new Map(); // ticket id -> unread count
  private destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadTickets();
    this.subscribeToNotifications();
  }

  private subscribeToNotifications(): void {
    // Bildirimleri dinle
    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
        this.updateTicketNotificationCounts();
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

    // Satıra tıklayınca detay sayfasına git
    this.router.navigate(['/admin/tickets', ticketId]);
  }

  loadTickets(): void {
    this.isLoading = true;
    this.dashboardService.getAllTicketsPage(this.currentPage, this.pageSize).subscribe({
      next: (response: any) => {
        if (response.items) {
          this.tickets = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.extractCustomers();
          this.filteredTickets = this.tickets;
        } else if (Array.isArray(response)) {
          this.tickets = response;
          this.extractCustomers();
          this.filteredTickets = response;
          this.totalCount = response.length;
          this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        }
        // Sort: active tickets first, then resolved/closed at the end
        this.sortTickets();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ticketlar yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  extractCustomers(): void {
    const uniqueCustomers = new Set<string>();
    this.tickets.forEach(ticket => {
      if (ticket.tenantName) {
        uniqueCustomers.add(ticket.tenantName);
      }
    });
    this.customers = Array.from(uniqueCustomers).sort();
  }

  onPageChange(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.currentPage = newPage;
      this.loadTickets();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.onPageChange(this.currentPage + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.onPageChange(this.currentPage - 1);
    }
  }

  applyFilters(): void {
    this.filteredTickets = this.tickets.filter(ticket => {
      const ticketIdText = String(ticket.id ?? '');
      const matchesSearch = !this.searchTerm || 
        ticket.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticket.tenantName?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticketIdText.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesStatus = !this.statusFilter || ticket.status.toString() === this.statusFilter;
      const matchesPriority = !this.priorityFilter || ticket.priority.toString() === this.priorityFilter;
      const matchesCustomer = !this.customerFilter || ticket.tenantName === this.customerFilter;
      
      return matchesSearch && matchesStatus && matchesPriority && matchesCustomer;
    });

    
    this.sortTickets();
  }

  private sortTickets(): void {
    this.filteredTickets.sort((a, b) => {
      const aIsResolved = a.status === 4 || a.status === 5;
      const bIsResolved = b.status === 4 || b.status === 5;
      
      if (aIsResolved && !bIsResolved) return 1;
      if (!aIsResolved && bIsResolved) return -1;
      return 0;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.priorityFilter = '';
    this.customerFilter = '';
    this.filteredTickets = this.tickets;
  }

  getStatusText(status: number): string {
    const statusMap: Record<number, string> = {
      1: 'Açık',
      2: 'İşlemde',
      3: 'Beklemede',
      4: 'Çözüldü',
      5: 'Kapalı'
    };
    return statusMap[status] || 'Bilinmiyor';
  }

  getStatusClass(status: number): string {
    const classMap: Record<number, string> = {
      1: 'open',
      2: 'in-progress',
      3: 'waiting',
      4: 'resolved',
      5: 'closed'
    };
    return classMap[status] || 'open';
  }

  getPriorityText(priority: number): string {
    const priorityMap: Record<number, string> = {
      1: 'Düşük',
      2: 'Orta',
      3: 'Yüksek',
      4: 'Kritik'
    };
    return priorityMap[priority] || 'Bilinmiyor';
  }

  getPriorityClass(priority: number): string {
    const classMap: Record<number, string> = {
      1: 'low',
      2: 'medium',
      3: 'high',
      4: 'critical'
    };
    return classMap[priority] || 'low';
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '-';
      return date.toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return '-';
    }
  }

  formatTicketId(id: number | string | null | undefined, ticketCode?: string | null): string {
    // Backend'den gelen TicketCode'u kullan (T00001 formatı)
    if (ticketCode) return ticketCode;
    // Fallback: ID'den formatlama
    if (id === null || id === undefined) return '-';
    const numericId = typeof id === 'number' ? id : Number(id);
    return Number.isFinite(numericId) ? `T${numericId.toString().padStart(5, '0')}` : String(id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}