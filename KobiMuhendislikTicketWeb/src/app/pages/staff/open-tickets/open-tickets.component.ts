import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffTicket } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';

@Component({
  selector: 'app-open-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './open-tickets.component.html',
  styleUrls: ['./open-tickets.component.scss']
})
export class OpenTicketsComponent implements OnInit {
  tickets: StaffTicket[] = [];
  filteredTickets: StaffTicket[] = [];
  isLoading = true;
  error: string | null = null;
  successMessage: string | null = null;
  
  selectedStatus: string = 'all';
  selectedPriority: string = 'all';
  statusOptions: Array<any> = [];
  priorityOptions: Array<any> = [];

  constructor(private staffService: StaffService, private paramSvc: SystemParameterService) {}

  ngOnInit(): void {
    this.loadTickets();
    this.loadLookups();
  }

  loadLookups(): void {
    this.paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const sData = res?.data?.data || res?.data || res || [];
        this.statusOptions = (Array.isArray(sData) ? sData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.statusOptions = []; }
    });

    this.paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const pData = res?.data?.data || res?.data || res || [];
        this.priorityOptions = (Array.isArray(pData) ? pData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.priorityOptions = []; }
    });
  }

  getStatusColor(status: number): string | null {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    return found?.color ?? null;
  }

  getPriorityColor(priority: number): string | null {
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    return found?.color ?? null;
  }

  loadTickets(): void {
    this.isLoading = true;
    this.staffService.getUnassignedTickets().subscribe({
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

  onPriorityFilterChange(event: Event): void {
    this.selectedPriority = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  onStatusFilterChange(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  claimTicket(ticketId: number | string): void {
    this.staffService.claimTicket(ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Ticket başarıyla alındı';
          this.loadTickets();
          setTimeout(() => this.successMessage = null, 3000);
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Ticket alınırken hata oluştu';
      }
    });
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

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const days = Math.floor(hours / 24);
    
    if (days > 0) return `${days} gün önce`;
    if (hours > 0) return `${hours} saat önce`;
    return 'Az önce';
  }
}
