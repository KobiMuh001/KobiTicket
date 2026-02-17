import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';

interface Ticket {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  createdAt: string;
  updatedAt: string;
  assetName?: string;
}

@Component({
  selector: 'app-customer-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-tickets.component.html',
  styleUrls: ['./customer-tickets.component.scss']
})
export class CustomerTicketsComponent implements OnInit {
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  isLoading = true;
  
  // Filters
  searchQuery = '';
  statusFilter = '';
  priorityFilter = '';

  constructor(private ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading = true;
    
    this.ticketService.getMyTickets().subscribe({
      next: (response: any) => {
        const data = response.data || response || [];
        this.tickets = data.map((t: any) => ({
          id: t.id,
          title: t.title,
          description: t.description,
          status: this.getStatusText(t.status),
          priority: this.getPriorityText(t.priority),
          createdAt: t.createdDate || t.createdAt,
          updatedAt: t.updatedDate || t.updatedAt,
          assetName: t.productName || t.asset?.productName || t.asset?.name || t.assetName
        })).sort((a: any, b: any) => {
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
        this.applyFilters();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredTickets = this.tickets.filter(ticket => {
      const matchesSearch = !this.searchQuery || 
        ticket.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        ticket.description?.toLowerCase().includes(this.searchQuery.toLowerCase());
      
      const matchesStatus = !this.statusFilter || ticket.status === this.statusFilter;
      const matchesPriority = !this.priorityFilter || ticket.priority === this.priorityFilter;

      return matchesSearch && matchesStatus && matchesPriority;
    });
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.statusFilter = '';
    this.priorityFilter = '';
    this.applyFilters();
  }

  getStatusText(status: string | number): string {
    const statusMap: { [key: string]: string; [key: number]: string } = {
      'Open': 'Açık',
      'Processing': 'İşlemde',
      'InProgress': 'İşlemde',
      'WaitingForCustomer': 'Müşteri Bekleniyor',
      'Waiting': 'Müşteri Bekleniyor',
      'Resolved': 'Çözüldü',
      'Closed': 'Kapatıldı',
      1: 'Açık',
      2: 'İşlemde',
      3: 'Müşteri Bekleniyor',
      4: 'Çözüldü',
      5: 'Kapatıldı'
    };
    return statusMap[status] || status.toString();
  }

  getPriorityText(priority: string | number): string {
    const priorityMap: { [key: string]: string; [key: number]: string } = {
      'Low': 'Düşük',
      'Medium': 'Orta',
      'High': 'Yüksek',
      'Critical': 'Kritik',
      1: 'Düşük',
      2: 'Orta',
      3: 'Yüksek',
      4: 'Kritik'
    };
    return priorityMap[priority] || priority.toString();
  }

  getStatusClass(status: string): string {
    const classMap: { [key: string]: string } = {
      'Açık': 'status-open',
      'İşlemde': 'status-progress',
      'Müşteri Bekleniyor': 'status-waiting',
      'Çözüldü': 'status-resolved',
      'Kapatıldı': 'status-closed'
    };
    return classMap[status] || '';
  }

  getPriorityClass(priority: string): string {
    const classMap: { [key: string]: string } = {
      'Düşük': 'priority-low',
      'Orta': 'priority-medium',
      'Yüksek': 'priority-high',
      'Kritik': 'priority-critical'
    };
    return classMap[priority] || '';
  }
}
