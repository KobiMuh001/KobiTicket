import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { forkJoin } from 'rxjs';

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

  statusOptions: any[] = [];
  priorityOptions: any[] = [];

  constructor(private ticketService: TicketService, private systemParameterService: SystemParameterService) {}

  ngOnInit(): void {
    this.loadLookups();
  }

  loadLookups(): void {
    forkJoin([
      this.systemParameterService.getByGroup('TicketStatus'),
      this.systemParameterService.getByGroup('TicketPriority')
    ]).subscribe({
      next: ([sRes, pRes]: any) => {
        const sData = sRes.data || sRes || [];
        const pData = pRes.data || pRes || [];

        // Sort status list by admin-provided sortOrder, then numericKey as fallback
        const sList = (sData || []).slice().sort((a: any, b: any) => {
          const sa = (a.sortOrder ?? a.numericKey ?? null);
          const sb = (b.sortOrder ?? b.numericKey ?? null);
          if (sa !== null && sb !== null) return sa - sb;
          if (sa !== null) return -1;
          if (sb !== null) return 1;
          return 0;
        });

        this.statusOptions = sList.map((s: any) => ({
          id: s.id,
          key: s.key,
          numericKey: s.numericKey ?? (typeof s.key === 'number' ? s.key : (Number.isFinite(Number(s.key)) ? Number(s.key) : null)),
          label: (s.numericKey != null) ? (s.value ?? s.description ?? s.key) : '',
          sortOrder: s.sortOrder,
          color: (s.numericKey != null) ? (s.value2 ?? s.color ?? null) : null
        }));

        // Sort priority list by admin-provided sortOrder, then numericKey as fallback
        const pList = (pData || []).slice().sort((a: any, b: any) => {
          const pa = (a.sortOrder ?? a.numericKey ?? null);
          const pb = (b.sortOrder ?? b.numericKey ?? null);
          if (pa !== null && pb !== null) return pa - pb;
          if (pa !== null) return -1;
          if (pb !== null) return 1;
          return 0;
        });

        this.priorityOptions = pList.map((p: any) => ({
          id: p.id,
          key: p.key,
          numericKey: p.numericKey ?? (typeof p.key === 'number' ? p.key : (Number.isFinite(Number(p.key)) ? Number(p.key) : null)),
          label: (p.numericKey != null) ? (p.value ?? p.description ?? p.key) : '',
          sortOrder: p.sortOrder,
          color: (p.numericKey != null) ? (p.value2 ?? p.color ?? null) : null
        }));

        this.loadTickets();
      },
      error: () => {
        // fallback: still load tickets even if lookups fail
        this.loadTickets();
      }
    });
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
    const n = Number(status);
    const opt = this.statusOptions.find((o: any) =>
      Number(o.numericKey ?? o.sortOrder ?? o.id) === n || o.key === status || String(o.id) === String(status)
    );
    if (opt) return opt.label;

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
    const n = Number(priority);
    const opt = this.priorityOptions.find((o: any) =>
      Number(o.numericKey ?? o.sortOrder ?? o.id) === n || o.key === priority || String(o.id) === String(priority)
    );
    if (opt) return opt.label;

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
}
