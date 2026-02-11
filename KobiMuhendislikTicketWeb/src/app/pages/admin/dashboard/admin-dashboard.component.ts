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
  monthlyMaxValue = 20;
  weeklyMaxValue = 28;
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
    const updatedId = Number(updatedTicket.id);
    const index = this.recentTickets.findIndex(t => Number(t.id) === updatedId);
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

  formatTicketId(id: number | string | null | undefined, ticketCode?: string | null): string {
    // Backend'den gelen TicketCode'u kullan (T00001 formatı)
    if (ticketCode) return ticketCode;
    // Fallback: ID'den formatlama
    if (id === null || id === undefined) return '-';
    const numericId = typeof id === 'number' ? id : Number(id);
    return Number.isFinite(numericId) ? `T${numericId.toString().padStart(5, '0')}` : String(id);
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
        
        // Dinamik ölçekleme için maksimum değerleri hesapla
        this.calculateChartMaxValues();
        
        console.log('Monthly chart data:', this.monthlyChartData);
        console.log('Weekly chart data:', this.weeklyChartData);
      },
      error: (err) => {
        console.error('Chart data loading error:', err);
        // Hata durumunda default veri göster
        this.monthlyChartData = this.generateMonthlyData();
        this.weeklyChartData = this.generateWeeklyData();
        this.calculateChartMaxValues();
      }
    });
  }

  private generateMonthlyDataFromTickets(allTickets: any[]): ChartData[] {
    const data: ChartData[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    // Son 8 hafta (2 ay) hafta hafta
    for (let weekIndex = 7; weekIndex >= 0; weekIndex--) {
      const weekStartDate = new Date(today);
      weekStartDate.setDate(weekStartDate.getDate() - (weekIndex * 7));
      
      const weekEndDate = new Date(weekStartDate);
      weekEndDate.setDate(weekEndDate.getDate() + 6);
      
      let opened = 0;
      let resolved = 0;
      
      // Haftanin 7 gunu icin ticket'lari cevapla
      for (let dayOffset = 0; dayOffset < 7; dayOffset++) {
        const checkDate = new Date(weekStartDate);
        checkDate.setDate(checkDate.getDate() + dayOffset);
        const checkDatetime = checkDate.getTime();
        
        for (const ticket of allTickets) {
          try {
            if (!ticket.createdDate) continue;
            
            const createdDate = new Date(ticket.createdDate);
            createdDate.setHours(0, 0, 0, 0);
            
            if (createdDate.getTime() === checkDatetime) {
              opened++;
              if (ticket.status === 4) {
                resolved++;
              }
            }
          } catch (e) {
            console.warn('Error parsing ticket date:', ticket.createdDate, e);
          }
        }
      }
      
      // Label format: 01.02 - 07.02
      const startDay = String(weekStartDate.getDate()).padStart(2, '0');
      const startMonth = String(weekStartDate.getMonth() + 1).padStart(2, '0');
      const endDay = String(weekEndDate.getDate()).padStart(2, '0');
      const endMonth = String(weekEndDate.getMonth() + 1).padStart(2, '0');
      
      data.push({
        label: `${startDay}.${startMonth} - ${endDay}.${endMonth}`,
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
            opened++;
            if (ticket.status === 4) {
              resolved++;
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
    today.setHours(0, 0, 0, 0);
    
    // Son 8 hafta (2 ay) hafta hafta
    for (let weekIndex = 7; weekIndex >= 0; weekIndex--) {
      const opened = Math.floor(Math.random() * 20) + 5;
      const resolved = Math.floor(Math.random() * 15) + 3;
      
      // Label format: 01.02 - 07.02
      const weekStartDate = new Date(today);
      weekStartDate.setDate(weekStartDate.getDate() - (weekIndex * 7));
      
      const weekEndDate = new Date(weekStartDate);
      weekEndDate.setDate(weekEndDate.getDate() + 6);
      
      const startDay = String(weekStartDate.getDate()).padStart(2, '0');
      const startMonth = String(weekStartDate.getMonth() + 1).padStart(2, '0');
      const endDay = String(weekEndDate.getDate()).padStart(2, '0');
      const endMonth = String(weekEndDate.getMonth() + 1).padStart(2, '0');
      
      data.push({
        label: `${startDay}.${startMonth} - ${endDay}.${endMonth}`,
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

  private calculateChartMaxValues(): void {
    // 2 aylık grafik maksimum değerini hesapla
    let monthlyMax = 0;
    for (const item of this.monthlyChartData) {
      const max = Math.max(item.opened, item.resolved);
      monthlyMax = Math.max(monthlyMax, max);
    }
    // Rond up to nearest 5
    this.monthlyMaxValue = Math.ceil(monthlyMax / 5) * 5;
    if (this.monthlyMaxValue < 5) this.monthlyMaxValue = 5;

    // 7 günlük grafik maksimum değerini hesapla
    let weeklyMax = 0;
    for (const item of this.weeklyChartData) {
      const max = Math.max(item.opened, item.resolved);
      weeklyMax = Math.max(weeklyMax, max);
    }
    // Rond up to nearest 7
    this.weeklyMaxValue = Math.ceil(weeklyMax / 7) * 7;
    if (this.weeklyMaxValue < 7) this.weeklyMaxValue = 7;
  }

  getMonthlyGridLines(): number[] {
    const lines: number[] = [];
    for (let i = 0; i <= 5; i++) {
      lines.push(i);
    }
    return lines;
  }

  getWeeklyGridLines(): number[] {
    const lines: number[] = [];
    for (let i = 0; i <= 7; i++) {
      lines.push(i);
    }
    return lines;
  }

  getMonthlyScaleValue(index: number): number {
    return Math.round(index * this.monthlyMaxValue / 5);
  }

  getWeeklyScaleValue(index: number): number {
    return Math.round(index * this.weeklyMaxValue / 7);
  }

  getMonthlyPixelPerUnit(): number {
    return 150 / this.monthlyMaxValue;
  }

  getWeeklyPixelPerUnit(): number {
    return 150 / this.weeklyMaxValue;
  }
}
