import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StaffService, StaffTicket } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Subject, firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './my-tickets.component.html',
  styleUrls: ['./my-tickets.component.scss']
})
export class MyTicketsComponent implements OnInit, OnDestroy {
  tickets: StaffTicket[] = [];
  filteredTickets: StaffTicket[] = [];
  allTicketsCache: StaffTicket[] | null = null;
  notifications: Notification[] = [];
  ticketNotifications: Map<number, number> = new Map();
  isLoading = true;
  error: string | null = null;
  successMessage: string | null = null;


  currentPage = 1;
  itemsPerPage = 20;
  totalPages = 1;
  totalCount = 0;

  selectedStatus: string = '';
  selectedPriority: string = '';
  selectedCustomer: string = '';
  searchTerm: string = '';
  customers: string[] = [];


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

    setTimeout(() => {
      this.initSignalR();
      this.subscribeToNotifications();
    }, 1000);
    this.loadLookups();
  }


  private async fetchAllMyTicketsPaginated(pageSize = 200): Promise<StaffTicket[]> {
    const all: StaffTicket[] = [];
    let page = 1;
    try {
      while (true) {
        const resp: any = await firstValueFrom(this.staffService.getMyTickets(page, pageSize));
        if (!resp) break;

        let items: StaffTicket[] = [];
        if (resp.data && Array.isArray(resp.data)) items = resp.data;
        else if (Array.isArray(resp)) items = resp;
        if (items.length) all.push(...items);

        const totalPages = resp && resp.totalPages ? resp.totalPages : Math.ceil((resp.totalCount ?? items.length) / pageSize || 1);
        if (!totalPages || page >= totalPages) break;
        page++;
      }
    } catch (e) {
      throw e;
    }
    return all;
  }

  loadLookups(): void {

    const paramSvc = (this as any).paramSvc as SystemParameterService | undefined;
    if (!paramSvc) return;
    paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        let list = res?.data?.data || res?.data || res || [];
        list = (Array.isArray(list) ? list.slice() : []).sort((a: any, b: any) => {
          const sa = (a.sortOrder ?? a.numericKey ?? null);
          const sb = (b.sortOrder ?? b.numericKey ?? null);
          if (sa !== null && sb !== null) return sa - sb;
          if (sa !== null) return -1;
          if (sb !== null) return 1;
          return 0;
        });
        this.statusOptions = list.map((p: any, i: number) => ({
          id: p.id,
          value: p.numericKey != null ? String(p.numericKey) : '',
          label: (p.numericKey != null) ? (p.value || p.key || p.description) : '',
          key: p.numericKey ?? p.key,
          numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)),
          sortOrder: p.sortOrder ?? i + 1,
          color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null
        }));
      },
      error: () => { this.statusOptions = []; }
    });
    paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        let list = res?.data?.data || res?.data || res || [];
        list = (Array.isArray(list) ? list.slice() : []).sort((a: any, b: any) => {
          const sa = (a.sortOrder ?? a.numericKey ?? null);
          const sb = (b.sortOrder ?? b.numericKey ?? null);
          if (sa !== null && sb !== null) return sa - sb;
          if (sa !== null) return -1;
          if (sb !== null) return 1;
          return 0;
        });
        this.priorityOptions = list.map((p: any, i: number) => ({
          id: p.id,
          value: p.numericKey != null ? String(p.numericKey) : '',
          label: (p.numericKey != null) ? (p.value || p.key || p.description) : '',
          key: p.numericKey ?? p.key,
          numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)),
          sortOrder: p.sortOrder ?? i + 1,
          color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null
        }));
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

    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
        this.updateTicketNotificationCounts();
      });


    this.signalRService.commentReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe((comment: CommentMessage) => {

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

    // Ticket güncellemelerini dinle
    this.signalRService.ticketUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((updatedTicket: any) => {
        console.log('Real-time ticket update received:', updatedTicket);
        this.updateTicketInList(updatedTicket);
      });

    // Yeni bildirimler geldiğinde listeyi yenile (özellikle yeni atamalar için)
    this.notificationService.notificationsList$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notifications: Notification[]) => {
        // Eğer yeni bir bildirim varsa ve tipi 'TicketAssigned' ise listeyi yenile
        const hasNewAssignment = notifications.some((n: Notification) => !n.isRead && n.type === 'TicketAssigned');
        if (hasNewAssignment) {
          console.log('New ticket assignment detected, refreshing list...');
          this.loadTickets();
        }
      });
  }

  private updateTicketInList(updatedTicket: any): void {
    const ticketId = Number(updatedTicket.id);

    // Cache'i kontrol et
    if (this.allTicketsCache) {
      const cacheIndex = this.allTicketsCache.findIndex(t => t.id === ticketId);
      if (cacheIndex !== -1) {
        this.allTicketsCache[cacheIndex] = {
          ...this.allTicketsCache[cacheIndex],
          ...updatedTicket,
          companyName: updatedTicket.tenantName // Backend DTO mapping
        };
      }
    }

    // Mevcut listeyi kontrol et
    const index = this.tickets.findIndex(t => t.id === ticketId);
    if (index !== -1) {
      this.tickets[index] = {
        ...this.tickets[index],
        ...updatedTicket,
        companyName: updatedTicket.tenantName // Backend DTO mapping
      };
      this.applyFilters();
    }
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

  private joinTicketGroups(): void {
    if (!this.isBrowser) return;

    // Yüklenen tüm ticketlar için SignalR grubuna katıl
    this.tickets.forEach(ticket => {
      this.signalRService.joinTicketGroup(ticket.id.toString()).catch(err => {
        console.error(`Error joining ticket group ${ticket.id}:`, err);
      });
    });
  }

  loadTickets(): void {
    this.isLoading = true;
    const hasFilter = !!(this.selectedStatus || this.selectedPriority || this.selectedCustomer || this.searchTerm);

    if (hasFilter) {
      if (this.allTicketsCache) {
        this.tickets = this.allTicketsCache;
        this.applyFilters();
        this.isLoading = false;
        return;
      }

      this.fetchAllMyTicketsPaginated().then(all => {
        this.tickets = all || [];
        this.allTicketsCache = this.tickets;
        this.extractCustomers();
        this.joinTicketGroups(); // SignalR gruplarına katıl
        this.applyFilters();
        this.isLoading = false;
      }).catch(() => {
        this.error = 'Ticketlar yüklenirken hata oluştu';
        this.isLoading = false;
      });
      return;
    }

    this.allTicketsCache = null;
    this.staffService.getMyTickets(this.currentPage, this.itemsPerPage).subscribe({
      next: (res) => {
        let items: StaffTicket[] = [];
        let totalCount = 0;
        let totalPages = 1;

        const payload = (res && res.success !== undefined) ? res.data ?? null : res;

        if (Array.isArray(payload)) {
          items = payload;
          totalCount = items.length;
          totalPages = Math.max(1, Math.ceil(totalCount / this.itemsPerPage));
        } else if (payload && Array.isArray(payload.items)) {
          items = payload.items;
          totalCount = payload.totalCount ?? items.length;
          totalPages = payload.totalPages ?? Math.max(1, Math.ceil(totalCount / this.itemsPerPage));
        } else if (res && Array.isArray(res.items)) {
          items = res.items;
          totalCount = res.totalCount ?? items.length;
          totalPages = res.totalPages ?? Math.max(1, Math.ceil(totalCount / this.itemsPerPage));
        } else if (Array.isArray(res)) {
          items = res;
          totalCount = items.length;
          totalPages = Math.max(1, Math.ceil(totalCount / this.itemsPerPage));
        } else if (res && res.data && Array.isArray(res.data)) {
          items = res.data;
          totalCount = items.length;
          totalPages = Math.max(1, Math.ceil(totalCount / this.itemsPerPage));
        }

        this.tickets = items;
        this.extractCustomers();
        this.joinTicketGroups(); // SignalR gruplarına katıl
        this.applyFilters();


        this.totalCount = totalCount;
        this.totalPages = totalPages;

        if (!res?.totalCount && !res?.totalPages && totalPages === 1 && Array.isArray(items) && items.length === this.itemsPerPage) {
          this.totalPages = this.currentPage + 1;
        }

        if (this.totalPages < this.currentPage) {
          this.totalPages = this.currentPage;
        }


        if (!this.totalCount && this.totalPages > 1) {
          this.totalCount = this.totalPages * this.itemsPerPage;
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
    const termNorm = this.normalizeForSearch(this.searchTerm || '');

    const results = this.tickets.filter(ticket => {
      const ticketIdText = String(ticket.id ?? '');
      const titleNorm = this.normalizeForSearch(ticket.title || '');
      const tenantNorm = this.normalizeForSearch(ticket.companyName || '');
      const idNorm = this.normalizeForSearch(ticketIdText);

      const matchesSearch = !termNorm ||
        titleNorm.includes(termNorm) ||
        tenantNorm.includes(termNorm) ||
        idNorm.includes(termNorm);

      const statusMatch = !this.selectedStatus || ticket.status.toString() === this.selectedStatus;
      const priorityMatch = !this.selectedPriority || ticket.priority.toString() === this.selectedPriority;
      const customerMatch = !this.selectedCustomer || ticket.companyName === this.selectedCustomer;

      return matchesSearch && statusMatch && priorityMatch && customerMatch;
    });


    // Global sorting by status is now handled on the backend



    if (this.allTicketsCache) {
      this.totalCount = results.length;
      this.totalPages = Math.max(1, Math.ceil(this.totalCount / this.itemsPerPage));
      const start = (this.currentPage - 1) * this.itemsPerPage;
      this.filteredTickets = results.slice(start, start + this.itemsPerPage);
    } else {

      this.filteredTickets = results;
      this.totalCount = this.filteredTickets.length;
    }
  }

  onStatusFilterChange(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.currentPage = 1;
    this.loadTickets();
  }

  onPriorityFilterChange(event: Event): void {
    this.selectedPriority = (event.target as HTMLSelectElement).value;
    this.currentPage = 1;
    this.loadTickets();
  }

  onCustomerFilterChange(event: Event): void {
    this.selectedCustomer = (event.target as HTMLSelectElement).value;
    this.currentPage = 1;
    this.loadTickets();
  }


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
    const found = this.statusOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
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
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
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
    const classMap: Record<number, string> = {
      1: 'low',
      2: 'medium',
      3: 'high',
      4: 'critical'
    };
    return classMap[priority] || 'low';
  }

  getStatusColor(status: string | number): string | null {
    const s = String(status ?? '');
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === s || String(o.key) === s || o.label === status || String(o.label) === s);
    return found?.color ?? null;
  }

  getPriorityColor(priority: string | number): string | null {
    const p = String(priority ?? '');
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === p || String(o.key) === p || o.label === priority || String(o.label) === p);
    return found?.color ?? null;
  }


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

  normalizeForSearch(value: any): string {
    try {
      const s = (value ?? '').toString();
      return s.toLocaleLowerCase('tr').normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    } catch {
      return (value ?? '').toString().toLowerCase();
    }
  }

  trackByTicketId(index: number, ticket: StaffTicket): number | string | null {
    return ticket?.id ?? index;
  }

  formatTicketId(id: number | string | null | undefined, ticketCode?: string | null): string {
    if (ticketCode) return ticketCode;
    if (id === null || id === undefined) return '-';
    const numericId = typeof id === 'number' ? id : Number(id);
    return Number.isFinite(numericId) ? `T${numericId.toString().padStart(5, '0')}` : String(id);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedPriority = '';
    this.selectedCustomer = '';
    this.currentPage = 1;
    this.loadTickets();
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

  onPageChange(page: number): void {
    this.goToPage(page);
  }

  get paginatedTickets(): StaffTicket[] {
    return this.filteredTickets;
  }
}