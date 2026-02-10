import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService, TicketListItem } from '../../../core/services/dashboard.service';

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

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading = true;
    this.dashboardService.getAllTicketsPage(this.currentPage, this.pageSize).subscribe({
      next: (response: any) => {
        if (response.items) {
          this.tickets = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.filteredTickets = this.tickets;
        } else if (Array.isArray(response)) {
          this.tickets = response;
          this.filteredTickets = response;
          this.totalCount = response.length;
          this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        }
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ticketlar yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
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
      const matchesSearch = !this.searchTerm || 
        ticket.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticket.tenantName?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticket.id.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesStatus = !this.statusFilter || ticket.status.toString() === this.statusFilter;
      const matchesPriority = !this.priorityFilter || ticket.priority.toString() === this.priorityFilter;
      
      return matchesSearch && matchesStatus && matchesPriority;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.priorityFilter = '';
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
}
