import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService, TicketListItem } from '../../../core/services/dashboard.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { StaffService } from '../../../core/services/staff.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Subject, firstValueFrom } from 'rxjs';
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
  allTicketsCache: TicketListItem[] | null = null;
  isLoading = true;
  errorMessage = '';

  // Staff Context
  staffId: number | null = null;
  staffName: string = '';

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
  // dynamic options (include DB color and key)
  statusOptions: { value: string; label: string; key?: string; color?: string | null }[] = [];
  priorityOptions: { value: string; label: string; key?: string; color?: string | null }[] = [];
  // desired stable order (matches legacy enum mapping)
  private statusKeyOrder = ['Open', 'Processing', 'WaitingForCustomer', 'Resolved', 'Closed'];
  private priorityKeyOrder = ['Low', 'Medium', 'High', 'Critical'];

  // Notifications
  notifications: Notification[] = [];
  ticketNotifications: Map<number, number> = new Map(); // ticket id -> unread count
  private destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private notificationService: NotificationService,
    private staffService: StaffService,
    private router: Router,
    private route: ActivatedRoute,
    private paramSvc: SystemParameterService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.staffId = +params['id'];
        this.loadStaffName();
      }
      this.loadTickets();
      this.subscribeToNotifications();
      this.loadDynamicFilters();
    });
  }

  private loadStaffName(): void {
    if (!this.staffId) return;
    this.staffService.getStaffById(this.staffId).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.staffName = res.data.fullName;
        }
      }
    });
  }

  // Fetch all paginated ticket pages from server and return combined array
  private async fetchAllTicketsPaginated(pageSize = 200): Promise<TicketListItem[]> {
    const all: TicketListItem[] = [];
    let page = 1;
    try {
      while (true) {
        // use the paged API to retrieve each page and accumulate
        const respRaw: any = await firstValueFrom(this.dashboardService.getAllTicketsPage(page, pageSize, this.staffId || undefined));
        const resp = (respRaw && respRaw.success && respRaw.data) ? respRaw.data : respRaw;

        let items: TicketListItem[] = [];
        if (resp == null) break;
        if (resp.items && Array.isArray(resp.items)) {
          items = resp.items;
        } else if (Array.isArray(resp)) {
          items = resp;
        }

        if (items.length) all.push(...items);

        // determine if we should continue
        const totalPages = resp && resp.totalPages ? resp.totalPages : Math.ceil((resp.totalCount ?? items.length) / pageSize || 1);
        if (!totalPages || page >= totalPages) break;
        page++;
      }
    } catch (e) {
      // bubble error up to caller
      throw e;
    }
    return all;
  }

  private loadDynamicFilters(): void {
    // Load TicketStatus and TicketPriority from system parameters and map to numeric option values
    this.paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        let list = res.data || res || [];
        // prefer SortOrder when present (admin-defined), then numericKey, then fallback to stable key order
        list = list.slice().sort((a: any, b: any) => {
          const sa = (a.sortOrder ?? a.numericKey ?? null);
          const sb = (b.sortOrder ?? b.numericKey ?? null);
          if (sa !== null && sb !== null) return sa - sb;
          if (sa !== null) return -1;
          if (sb !== null) return 1;
          const ia = this.statusKeyOrder.indexOf(a.key ?? a.Key ?? a.value ?? '');
          const ib = this.statusKeyOrder.indexOf(b.key ?? b.Key ?? b.value ?? '');
          return ia - ib;
        });
        this.statusOptions = list.map((p: any, i: number) => ({ value: p.numericKey != null ? String(p.numericKey) : '', label: (p.numericKey != null) ? (p.value || p.key || p.description) : '', key: p.numericKey ?? p.key, numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)), color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null }));
      },
      error: () => {
        this.statusOptions = [];
      }
    });

    this.paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        let list = res.data || res || [];
        // prefer SortOrder when present (admin-defined), then numericKey, then fallback to stable key order
        list = list.slice().sort((a: any, b: any) => {
          const sa = (a.sortOrder ?? a.numericKey ?? null);
          const sb = (b.sortOrder ?? b.numericKey ?? null);
          if (sa !== null && sb !== null) return sa - sb;
          if (sa !== null) return -1;
          if (sb !== null) return 1;
          const ia = this.priorityKeyOrder.indexOf(a.key ?? a.Key ?? a.value ?? '');
          const ib = this.priorityKeyOrder.indexOf(b.key ?? b.Key ?? b.value ?? '');
          return ia - ib;
        });
        this.priorityOptions = list.map((p: any, i: number) => ({ value: p.numericKey != null ? String(p.numericKey) : '', label: (p.numericKey != null) ? (p.value || p.key || p.description) : '', key: p.numericKey ?? p.key, numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)), color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null }));
      },
      error: () => {
        this.priorityOptions = [];
      }
    });
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

    const hasFilter = !!(this.searchTerm || this.statusFilter || this.priorityFilter || this.customerFilter);

    // When filters are active, fetch full list once and apply client-side filtering + pagination
    if (hasFilter) {
      if (this.allTicketsCache) {
        this.tickets = this.allTicketsCache;
        this.applyLocalFilteringAndPagination();
        this.isLoading = false;
        return;
      }

      // Fetch all pages from the paginated API to ensure we have the full dataset
      this.fetchAllTicketsPaginated().then((allTickets) => {
        this.tickets = allTickets || [];
        this.allTicketsCache = this.tickets;
        this.extractCustomers();
        this.applyLocalFilteringAndPagination();
        this.isLoading = false;
      }).catch(() => {
        this.errorMessage = 'Ticketlar yüklenirken hata oluştu.';
        this.isLoading = false;
      });
      return;
    }

    // No filters: use paginated server API
    this.allTicketsCache = null;
    this.dashboardService.getAllTicketsPage(this.currentPage, this.pageSize, this.staffId || undefined).subscribe({
      next: (responseRaw: any) => {
        const response = (responseRaw && responseRaw.success && responseRaw.data) ? responseRaw.data : responseRaw;

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
        // Sorting is now handled server-side in the backend query
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
    // Reset to first page and reload so filtering is applied to full dataset
    this.currentPage = 1;
    this.loadTickets();
  }

  private applyLocalFilteringAndPagination(): void {
    const termNorm = this.normalizeForSearch(this.searchTerm || '');

    let results = (this.tickets || []).filter(ticket => {
      const ticketIdText = String(ticket.id ?? '');
      const titleNorm = this.normalizeForSearch(ticket.title || '');
      const tenantNorm = this.normalizeForSearch(ticket.tenantName || '');
      const idNorm = this.normalizeForSearch(ticketIdText);

      const matchesSearch = !termNorm ||
        titleNorm.includes(termNorm) ||
        tenantNorm.includes(termNorm) ||
        idNorm.includes(termNorm);

      const matchesStatus = !this.statusFilter || ticket.status.toString() === this.statusFilter;
      const matchesPriority = !this.priorityFilter || ticket.priority.toString() === this.priorityFilter;
      const matchesCustomer = !this.customerFilter || (ticket.tenantName === this.customerFilter);

      return matchesSearch && matchesStatus && matchesPriority && matchesCustomer;
    });

    // Sort: active tickets first, then resolved/closed at the end
    results.sort((a, b) => {
      const aIsResolved = a.status === 4 || a.status === 5;
      const bIsResolved = b.status === 4 || b.status === 5;
      if (aIsResolved && !bIsResolved) return 1;
      if (!aIsResolved && bIsResolved) return -1;
      return 0;
    });

    this.totalCount = results.length;
    this.totalPages = Math.max(1, Math.ceil(this.totalCount / this.pageSize));

    const start = (this.currentPage - 1) * this.pageSize;
    this.filteredTickets = results.slice(start, start + this.pageSize);
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

  // Normalize strings for search: locale-aware lowercasing (Turkish) + remove combining diacritics
  private normalizeForSearch(value: any): string {
    try {
      const s = (value ?? '').toString();
      // Use Turkish locale to correctly map dotted/dotless I, then decompose and strip diacritics
      return s.toLocaleLowerCase('tr').normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    } catch {
      return (value ?? '').toString().toLowerCase();
    }
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.priorityFilter = '';
    this.customerFilter = '';
    this.currentPage = 1;
    this.loadTickets();
  }

  getStatusText(status: number): string {
    const found = this.statusOptions.find(s => s.value === String(status));
    if (found) return found.label;
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
    const found = this.priorityOptions.find(p => p.value === String(priority));
    if (found) return found.label;
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

  trackByTicketId(index: number, ticket: TicketListItem): number | string | null {
    return ticket?.id ?? index;
  }

  // Return DB-provided color (value2) for status/priority or null
  getStatusColor(status: number): string | null {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.value) === n || Number(o.key) === n || Number(o.numericKey) === n || String(o.value) === String(status) || String(o.key) === String(status) || o.label === status);
    return found?.color ?? null;
  }

  getPriorityColor(priority: number): string | null {
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.value) === n || Number(o.key) === n || Number(o.numericKey) === n || String(o.value) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    return found?.color ?? null;
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