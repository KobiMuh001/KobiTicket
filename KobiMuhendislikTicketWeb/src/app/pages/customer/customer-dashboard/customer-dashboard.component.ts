import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { AssetService } from '../../../core/services/asset.service';
import { ProductService } from '../../../core/services/product.service';
import { AuthService, User } from '../../../core/services/auth.service';
import { TenantService } from '../../../core/services/tenant.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { environment } from '../../../../environments/environment';

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
  companyLogoUrl: string | null = null;
  private readonly apiOrigin = new URL(environment.apiUrl).origin;
  statusOptions: Array<any> = [];
  priorityOptions: Array<any> = [];

  constructor(
    private ticketService: TicketService,
    private assetService: AssetService,
    private productService: ProductService,
    private authService: AuthService,
    private tenantService: TenantService,
    private systemParamSvc: SystemParameterService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadCompanyLogo();
    this.loadLookups();
    this.loadDashboardData();
  }

  loadLookups(): void {
    if (!this.systemParamSvc) return;
    this.systemParamSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const sData = res?.data?.data || res?.data || res || [];
        this.statusOptions = (Array.isArray(sData) ? sData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.statusOptions = []; }
    });

    this.systemParamSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const pData = res?.data?.data || res?.data || res || [];
        this.priorityOptions = (Array.isArray(pData) ? pData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.priorityOptions = []; }
    });
  }

  getStatusColor(status: string | number): string | null {
    const s = String(status ?? '');
    const found = this.statusOptions.find((o: any) => String(o.sortOrder ?? o.id) === s || String(o.id) === s || String(o.key) === s || o.label === status || String(o.label) === s);
    return found?.color ?? null;
  }

  getPriorityColor(priority: string | number): string | null {
    const p = String(priority ?? '');
    const found = this.priorityOptions.find((o: any) => String(o.sortOrder ?? o.id) === p || String(o.id) === p || String(o.key) === p || o.label === priority || String(o.label) === p);
    return found?.color ?? null;
  }

  private loadCompanyLogo(): void {
    this.tenantService.getMyProfile().subscribe({
      next: (response: any) => {
        const data = response?.data || response;
        this.companyLogoUrl = this.toAbsoluteUrl(data?.logoUrl);
      },
      error: () => {
        this.companyLogoUrl = null;
      }
    });
  }

  private toAbsoluteUrl(path?: string | null): string | null {
    if (!path) {
      return null;
    }

    if (path.startsWith('http')) {
      return path;
    }

    return `${this.apiOrigin}${path}`;
  }

  loadDashboardData(): void {
    this.isLoading = true;

    // Load tickets
    this.ticketService.getMyTickets().subscribe({
      next: (response: any) => {
        const tickets = response.data || response || [];
        this.stats.totalTickets = tickets.length;
        this.stats.openTickets = tickets.filter((t: any) => String(t.status) === 'Open' || Number(t.status) === 1).length;
        // Treat any status that is NOT Resolved(4) and NOT Closed(5) and NOT Open(1) as in-progress (includes custom DB statuses)
        this.stats.inProgressTickets = tickets.filter((t: any) => {
          const s = String(t.status);
          const n = Number(t.status);
          if (s === 'Resolved' || s === 'Closed' || n === 4 || n === 5) return false;
          if (s === 'Open' || n === 1) return false;
          return true;
        }).length;
        this.stats.closedTickets = tickets.filter((t: any) => String(t.status) === 'Resolved' || String(t.status) === 'Closed' || Number(t.status) === 4 || Number(t.status) === 5).length;
        
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

    // Load products (treat tenant products as assets for the dashboard)
    const tenantId = this.currentUser?.identifier ? Number(this.currentUser.identifier) : null;
    if (tenantId) {
      this.productService.getTenantProducts(tenantId).subscribe({
        next: (response: any) => {
          const products = response.data || response || [];
          this.stats.totalAssets = products.length;
        },
        error: () => {
          // Fallback to old asset endpoint if product list fails
          this.assetService.getMyAssets().subscribe({
            next: (resp: any) => { const assets = resp.data || resp || []; this.stats.totalAssets = assets.length; },
            error: () => { this.stats.totalAssets = 0; }
          });
        }
      });
    } else {
      this.stats.totalAssets = 0;
    }
  }

  getStatusText(status: string | number): string {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) return found.label || 'Bilinmiyor';

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
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) return found.label || 'Normal';

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
    const n = Number(status as any);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'status-open';
        case 2: return 'status-progress';
        case 3: return 'status-waiting';
        case 4: return 'status-resolved';
        case 5: return 'status-closed';
        default: return '';
      }
    }
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
    const n = Number(priority as any);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'priority-low';
        case 2: return 'priority-medium';
        case 3: return 'priority-high';
        case 4: return 'priority-critical';
        default: return '';
      }
    }
    const classMap: { [key: string]: string } = {
      'Düşük': 'priority-low',
      'Orta': 'priority-medium',
      'Yüksek': 'priority-high',
      'Kritik': 'priority-critical'
    };
    return classMap[priority] || '';
  }
}
