import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StaffService, StaffTicket } from '../../../core/services/staff.service';

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
  
  selectedPriority: string = 'all';

  constructor(private staffService: StaffService) {}

  ngOnInit(): void {
    this.loadTickets();
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
      return this.selectedPriority === 'all' || ticket.priority.toString() === this.selectedPriority;
    });
  }

  onPriorityFilterChange(event: Event): void {
    this.selectedPriority = (event.target as HTMLSelectElement).value;
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
