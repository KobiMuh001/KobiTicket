import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffWorkload, StaffTicket } from '../../../core/services/staff.service';

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

  constructor(
    private staffService: StaffService
  ) {}

  ngOnInit(): void {
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
}
