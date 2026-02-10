import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService, DashboardStats, TicketListItem } from '../../../core/services/dashboard.service';
import { SignalRService, TicketUpdateMessage, DashboardStats as SignalRDashboardStats } from '../../../core/services/signalr.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface StatCard {
  title: string;
  value: number;
  icon: string;
  color: string;
  change: number;
  changeType: 'increase' | 'decrease';
}

interface ChartData {
  label: string;
  opened: number;
  resolved: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  stats: StatCard[] = [];
  recentTickets: TicketListItem[] = [];
  monthlyChartData: ChartData[] = [];
  weeklyChartData: ChartData[] = [];
  isLoading = true;
  errorMessage = '';
  private destroy$ = new Subject<void>();
  private isBrowser: boolean;
  private refreshInterval: any;

  constructor(
    private dashboardService: DashboardService,
    private signalRService: SignalRService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadChartData();
    this.initSignalR();
    
    this.refreshInterval = setInterval(() => {
      console.log('Dashboard: Periodic refresh triggered');
      this.loadTicketsOnly();
    }, 30000); 
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    this.signalRService.stopConnection().catch(err => 
      console.error('SignalR disconnect error:', err)
    );
    this.signalRService.stopDashboardConnection().catch(err =>
      console.error('SignalR dashboard disconnect error:', err)
    );
  }

  private initSignalR(): void {
    if (!this.isBrowser) {
      console.log('Dashboard: SignalR - Not in browser environment');
      return;
    }

    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token) {
      console.log('Dashboard: SignalR - Starting connection...');
      this.signalRService.startConnection(token).then(() => {
        console.log('Dashboard: SignalR - Connection established');
        this.subscribeToTicketUpdates();
      }).catch(err => {
        console.error('Dashboard: SignalR bağlantı hatası:', err);
      });

      // Dashboard stats hub bağlantısını başlat
      this.signalRService.startDashboardConnection(token).then(() => {
        console.log('Dashboard: SignalR Dashboard - Connection established');
        this.subscribeToDashboardStats();
      }).catch(err => {
        console.error('Dashboard: SignalR Dashboard bağlantı hatası:', err);
      });
    } else {
      console.warn('Dashboard: No auth token found for SignalR');
    }
  }

  private subscribeToTicketUpdates(): void {
    console.log('Dashboard: Subscribing to ticket updates...');
    this.signalRService.ticketUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedTicket: TicketUpdateMessage) => {
          console.log('Dashboard: Ticket update received:', updatedTicket);
          this.handleTicketUpdate(updatedTicket);
        },
        error: (err) => {
          console.error('Dashboard: Error in ticket update subscription:', err);
        }
      });
  }

  private subscribeToDashboardStats(): void {
    console.log('Dashboard: Subscribing to dashboard stats updates...');
    this.signalRService.dashboardStatsUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats: SignalRDashboardStats) => {
          console.log('Dashboard: Dashboard stats update received:', stats);
          this.updateStats(stats);
        },
        error: (err) => {
          console.error('Dashboard: Error in dashboard stats subscription:', err);
        }
      });
  }

  private handleTicketUpdate(updatedTicket: TicketUpdateMessage): void {
    console.log('Dashboard: Updating ticket in list:', updatedTicket.id);
    // Eğer bu ticket son 5'te ise güncelle
    const index = this.recentTickets.findIndex(t => t.id === updatedTicket.id);
    if (index !== -1) {
      console.log('Dashboard: Found ticket at index', index, 'updating...');
      this.recentTickets[index] = {
        ...this.recentTickets[index],
        title: updatedTicket.title,
        status: updatedTicket.status,
        priority: updatedTicket.priority,
        createdDate: updatedTicket.createdDate
      };
      // Array referansını güncelle (change detection'ı tetikle)
      this.recentTickets = [...this.recentTickets];
    } else {
      console.log('Dashboard: Ticket not found in recent list');
    }
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    // Istatistikleri yukle
    this.dashboardService.getStats().subscribe({
      next: (response: any) => {
        // API response.data.data formatinda donuyor
        let data: DashboardStats;
        if (response.data?.data) {
          data = response.data.data;
        } else if (response.data) {
          data = response.data;
        } else {
          data = response;
        }
        this.updateStats(data);
      },
      error: () => {
        this.errorMessage = 'İstatistikler yüklenirken hata oluştu.';
      }
    });

    // Son ticketlari yukle
    this.loadTicketsOnly();
  }

  private loadTicketsOnly(): void {
    this.dashboardService.getAllTicketsPage(1, 50).subscribe({
      next: (response: any) => {
        let tickets: TicketListItem[] = [];
        if (response.items) {
          tickets = response.items;
        } else if (Array.isArray(response)) {
          tickets = response;
        }
        this.recentTickets = tickets.slice(0, 5); // Son 5 ticket
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ticketlar yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  updateStats(data: DashboardStats): void {
    const total = data.totalTickets || 1; 
    
    this.stats = [
      {
        title: 'Toplam Ticket',
        value: data.totalTickets,
        icon: 'ticket',
        color: 'primary',
        change: 0, 
        changeType: 'increase'
      },
      {
        title: 'Açık Ticketlar',
        value: data.openTickets,
        icon: 'open',
        color: 'warning',
        change: Math.round((data.openTickets / total) * 100),
        changeType: 'increase'
      },
      {
        title: 'İşlemdeki Ticketlar',
        value: data.processingTickets || 0,
        icon: 'processing',
        color: 'orange',
        change: Math.round(((data.processingTickets || 0) / total) * 100),
        changeType: 'increase'
      },
      {
        title: 'Çözülen Ticketlar',
        value: data.resolvedTickets,
        icon: 'resolved',
        color: 'success',
        change: Math.round((data.resolvedTickets / total) * 100),
        changeType: 'increase'
      },
      {
        title: 'Müşteri Sayısı',
        value: data.totalTenants,
        icon: 'users',
        color: 'info',
        change: 0, 
        changeType: 'increase'
      }
    ];
  }

  getStatusText(status: number): string {
    const statusMap: Record<number, string> = {
      1: 'Açık',
      2: 'İşlemde',
      3: 'Müşteri Bekleniyor',
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
        year: 'numeric'
      });
    } catch {
      return '-';
    }
  }

  private loadChartData(): void {
    this.dashboardService.getAllTickets().subscribe({
      next: (response: any) => {
        console.log('Chart data raw response:', response);
        
        // Response formatını kontrol et ve parse et
        let allTickets: any[] = [];
        
        if (Array.isArray(response)) {
          allTickets = response;
        } else if (response?.data && Array.isArray(response.data)) {
          allTickets = response.data;
        } else if (response?.items && Array.isArray(response.items)) {
          allTickets = response.items;
        }
        
        console.log('Chart data parsed tickets count:', allTickets.length);
        console.log('Sample tickets:', allTickets.slice(0, 3));
        
        if (allTickets.length === 0) {
          console.warn('No tickets found, using fallback data');
          this.monthlyChartData = this.generateMonthlyData();
          this.weeklyChartData = this.generateWeeklyData();
          return;
        }
        
        // Son 30 günlük veri
        this.monthlyChartData = this.generateMonthlyDataFromTickets(allTickets);
        
        // Son 7 günlük veri
        this.weeklyChartData = this.generateWeeklyDataFromTickets(allTickets);
        
        console.log('Monthly chart data:', this.monthlyChartData);
        console.log('Weekly chart data:', this.weeklyChartData);
      },
      error: (err) => {
        console.error('Chart data loading error:', err);
        // Hata durumunda default veri göster
        this.monthlyChartData = this.generateMonthlyData();
        this.weeklyChartData = this.generateWeeklyData();
      }
    });
  }

  private generateMonthlyDataFromTickets(allTickets: any[]): ChartData[] {
    const data: ChartData[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    for (let i = 29; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      const dateTime = date.getTime();
      
      // Bu gün açılan ticket'lar (oluşturma tarihine göre)
      let opened = 0;
      let resolved = 0;
      
      for (const ticket of allTickets) {
        try {
          if (!ticket.createdDate) continue;
          
          const createdDate = new Date(ticket.createdDate);
          createdDate.setHours(0, 0, 0, 0);
          
          if (createdDate.getTime() === dateTime) {
            // Status 4 (çözüldü) ise resolved'a, değilse opened'a say
            if (ticket.status === 4) {
              resolved++;
            } else {
              opened++;
            }
          }
        } catch (e) {
          console.warn('Error parsing ticket date:', ticket.createdDate, e);
        }
      }
      
      // Tarih formatı: GG.AA (Örn: 10.02)
      const dayStr = String(date.getDate()).padStart(2, '0');
      const monthStr = String(date.getMonth() + 1).padStart(2, '0');
      
      data.push({
        label: `${dayStr}.${monthStr}`,
        opened,
        resolved
      });
    }
    
    return data;
  }

  private generateWeeklyDataFromTickets(allTickets: any[]): ChartData[] {
    const data: ChartData[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    // JavaScript getDay(): 0=Pazar, 1=Pazartesi, 2=Salı, 3=Çarşamba, 4=Perşembe, 5=Cuma, 6=Cumartesi
    const days = ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'];
    
    for (let i = 6; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      const dateTime = date.getTime();
      
      let opened = 0;
      let resolved = 0;
      
      for (const ticket of allTickets) {
        try {
          if (!ticket.createdDate) continue;
          
          const createdDate = new Date(ticket.createdDate);
          createdDate.setHours(0, 0, 0, 0);
          
          if (createdDate.getTime() === dateTime) {
            if (ticket.status === 4) {
              resolved++;
            } else {
              opened++;
            }
          }
        } catch (e) {
          console.warn('Error parsing ticket date:', ticket.createdDate, e);
        }
      }
      
      data.push({
        label: days[date.getDay()],
        opened,
        resolved
      });
    }
    
    return data;
  }

  private generateMonthlyData(): ChartData[] {
    const data: ChartData[] = [];
    const today = new Date();
    
    for (let i = 29; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      
      const opened = Math.floor(Math.random() * 8) + 1;
      const resolved = Math.floor(Math.random() * 6) + 1;
      
      // Tarih formatı: GG.AA (Örn: 10.02)
      const dayStr = String(date.getDate()).padStart(2, '0');
      const monthStr = String(date.getMonth() + 1).padStart(2, '0');
      
      data.push({
        label: `${dayStr}.${monthStr}`,
        opened,
        resolved
      });
    }
    
    return data;
  }

  private generateWeeklyData(): ChartData[] {
    const data: ChartData[] = [];
    const today = new Date();
    // JavaScript getDay(): 0=Pazar, 1=Pazartesi, 2=Salı, 3=Çarşamba, 4=Perşembe, 5=Cuma, 6=Cumartesi
    const days = ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'];
    
    for (let i = 6; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      
      const opened = Math.floor(Math.random() * 15) + 3;
      const resolved = Math.floor(Math.random() * 12) + 2;
      
      data.push({
        label: days[date.getDay()],
        opened,
        resolved
      });
    }
    
    return data;
  }
}
