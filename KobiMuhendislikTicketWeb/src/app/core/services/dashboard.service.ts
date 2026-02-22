import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  processingTickets: number;
  resolvedTickets: number;
  totalAssets: number;
  totalTenants: number;
  criticalTicketCount: number;
  topFailingAssets: AssetTicketCount[];
}

export interface AssetTicketCount {
  productName: string;
  ticketCount: number;
}

export interface DashboardResponse {
  success: boolean;
  message: string;
  data: DashboardStats;
  timestamp: string;
}

export interface Ticket {
  id: number;
  ticketCode?: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  createdDate: string;
  tenantId: number;
  tenant?: {
    companyName: string;
  };
  asset?: {
    productName: string;
  };
}

export interface TicketListItem {
  id: number;
  ticketCode?: string;
  title: string;
  status: number;
  priority: number;
  tenantName: string;
  tenantId: number;
  createdDate: string;
  updatedDate?: string;
  assignedPerson?: string;
  assignedStaffId?: number;
  productName?: string;
  assetName?: string;
  productId?: number;
  commentCount?: number;
  isOverdue?: boolean;
}

export interface PaginatedTickets {
  items: TicketListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getStats(): Observable<DashboardResponse> {
    return this.http.get<DashboardResponse>(`${this.apiUrl}/dashboard/stats`);
  }

  getAllTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/tickets/admin/all-tickets`);
  }

  getAllTicketsPage(page: number = 1, pageSize: number = 20): Observable<PaginatedTickets> {
    return this.http.get<PaginatedTickets>(`${this.apiUrl}/tickets/admin/all-tickets?page=${page}&pageSize=${pageSize}`);
  }
}
