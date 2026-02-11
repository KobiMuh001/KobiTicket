import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { AssetService } from '../../../core/services/asset.service';
import { AuthService, User } from '../../../core/services/auth.service';

interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  closedTickets: number;
  totalAssets: number;
}

interface Ticket {
  id: number;
  title: string;
  status: string;
  priority: string;
  createdAt: string;
}

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.scss']
})
export class CustomerDashboardComponent implements OnInit {
  stats: DashboardStats = {
    totalTickets: 0,
    openTickets: 0,
    inProgressTickets: 0,
    closedTickets: 0,
    totalAssets: 0
  };
  
  recentTickets: Ticket[] = [];
  isLoading = true;
  currentUser: User | null = null;

  constructor(
    private ticketService: TicketService,
    private assetService: AssetService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;

    // Load tickets
    this.ticketService.getMyTickets().subscribe({
      next: (response: any) => {
        const tickets = response.data || response || [];
        this.stats.totalTickets = tickets.length;
        this.stats.openTickets = tickets.filter((t: any) => t.status === 'Open' || t.status === 1).length;
        this.stats.inProgressTickets = tickets.filter((t: any) => t.status === 'InProgress' || t.status === 'Processing' || t.status === 2 || t.status === 3).length;
        this.stats.closedTickets = tickets.filter((t: any) => t.status === 'Closed' || t.status === 5 || t.status === 'Resolved' || t.status === 4).length;
        
        // Get recent tickets (last 5)
        this.recentTickets = tickets
          .sort((a: any, b: any) => new Date(b.createdDate || b.createdAt).getTime() - new Date(a.createdDate || a.createdAt).getTime())
          .slice(0, 5)
          .map((t: any) => ({
            id: t.id,
            title: t.title,
            status: this.getStatusText(t.status),
            priority: this.getPriorityText(t.priority),
            createdAt: t.createdDate || t.createdAt
          }));
          
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });

    // Load assets count
    this.assetService.getMyAssets().subscribe({
      next: (response: any) => {
        const assets = response.data || response || [];
        this.stats.totalAssets = assets.length;
      },
      error: () => {}
    });
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
