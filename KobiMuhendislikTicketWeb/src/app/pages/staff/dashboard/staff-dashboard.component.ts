import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffWorkload, StaffTicket } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';

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
  isLoading = true;
  error: string | null = null;
  private refreshInterval: any;
  statusOptions: Array<any> = [];
  priorityOptions: Array<any> = [];

  constructor(
    private staffService: StaffService,
    private paramSvc: SystemParameterService
  ) {}

  ngOnInit(): void {
    // Load DB-driven lookup options (best-effort)
    this.paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const sData = res?.data?.data || res?.data || res || [];
        this.statusOptions = (Array.isArray(sData) ? sData : []).map((p: any, i: number) => ({
          id: p.id,
          key: p.key,
          numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)),
          label: (p.numericKey != null) ? (p.value || p.key || p.description) : '',
          sortOrder: p.sortOrder ?? i + 1,
          color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null
        }));
      },
      error: () => { this.statusOptions = []; }
    });
    this.paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const pData = res?.data?.data || res?.data || res || [];
        this.priorityOptions = (Array.isArray(pData) ? pData : []).map((p: any, i: number) => ({
          id: p.id,
          key: p.key,
          numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)),
          label: (p.numericKey != null) ? (p.value || p.key || p.description) : '',
          sortOrder: p.sortOrder ?? i + 1,
          color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null
        }));
      },
      error: () => { this.priorityOptions = []; }
    });

    this.loadDashboardData();
    
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
  }

  loadDashboardData(): void {
    this.isLoading = true;

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
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) {
      const num = Number(found.numericKey ?? found.sortOrder ?? found.id);
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
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.numericKey ?? o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) {
      const num = Number(found.numericKey ?? found.sortOrder ?? found.id);
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

  getWorkloadPercentage(): number {
    if (!this.workload || !this.workload.maxConcurrentTickets) {
      return 0;
    }
    return Math.round((this.workload.assignedTickets / this.workload.maxConcurrentTickets) * 100);
  }

  onTicketSelect(ticketId: number): void {
    // Placeholder: called when a ticket row/card is clicked from the dashboard.
    // Currently navigation is handled by routerLink; this method can be
    // extended to mark notifications as read or perform analytics.
    console.log('Dashboard: ticket selected', ticketId);
  }
}
