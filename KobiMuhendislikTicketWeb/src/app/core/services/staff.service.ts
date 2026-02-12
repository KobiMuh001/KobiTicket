import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Staff {
  id: number;
  fullName: string;
  email: string;
  phone?: string;
  department: string;
  isActive: boolean;
  maxConcurrentTickets: number;
  createdDate: string;
}

export interface StaffWorkload {
  staffId: number;
  id: number;
  fullName: string;
  department: string;
  isActive: boolean;
  maxConcurrentTickets: number;
  assignedTickets: number;
  openTickets: number;
  processingTickets: number;
  resolvedToday: number;
  resolvedThisWeek: number;
  isAvailable: boolean;
}

export interface CreateStaffDto {
  fullName: string;
  email: string;
  password: string;
  phone?: string;
  department: string;
  maxConcurrentTickets: number;
}

export interface UpdateStaffDto {
  fullName?: string;
  email?: string;
  phone?: string;
  department?: string;
  isActive?: boolean;
  maxConcurrentTickets?: number;
  newPassword?: string;
}

export interface StaffTicket {
  id: number;
  ticketCode?: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  assignedPerson?: string;
  createdDate: string;
  updatedDate?: string;
  tenantId: number;
  companyName?: string;
  assetId?: number;
  assetName?: string;
  imagePath?: string;
}

@Injectable({
  providedIn: 'root'
})
export class StaffService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAllStaff(activeOnly?: boolean): Observable<any> {
    let params: any = {};
    if (activeOnly !== undefined) {
      params.activeOnly = activeOnly.toString();
    }
    return this.http.get(`${this.apiUrl}/admin/staff`, { params });
  }

  getStaffById(id: number | string): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/staff/${id}`);
  }

  createStaff(staff: CreateStaffDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/staff`, staff);
  }

  updateStaff(id: number | string, staff: UpdateStaffDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/admin/staff/${id}`, staff);
  }

  deleteStaff(id: number | string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/admin/staff/${id}`);
  }

  getStaffWorkloads(): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/staff/workloads`);
  }

  resetStaffPassword(id: number | string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/staff/${id}/reset-password`, { newPassword });
  }

  // =====================
  // Staff Panel Methods
  // =====================

  // Get own profile
  getMyProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/profile`);
  }

  // Get own workload
  getMyWorkload(): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/workload`);
  }

  // Get tickets assigned to me
  getMyTickets(page: number = 1, pageSize: number = 20): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/tickets`, { 
      params: { page: page.toString(), pageSize: pageSize.toString() } 
    });
  }

  // Get unassigned tickets
  getUnassignedTickets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/tickets/unassigned`);
  }

  // Claim a ticket
  claimTicket(ticketId: number | string): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff/tickets/${ticketId}/claim`, {});
  }

  // Release a ticket
  releaseTicket(ticketId: number | string): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff/tickets/${ticketId}/release`, {});
  }

  // Get ticket detail
  getTicketDetail(ticketId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/tickets/${ticketId}`);
  }

  // Add comment to ticket
  addComment(ticketId: string, message: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff/tickets/${ticketId}/comments`, { message });
  }

  // Update ticket status
  updateTicketStatus(ticketId: string, newStatus: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/staff/tickets/${ticketId}/status`, { newStatus });
  }

  // Resolve ticket
  resolveTicket(ticketId: string, solutionNote: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff/tickets/${ticketId}/resolve`, { solutionNote });
  }

  // Get ticket history
  getTicketHistory(ticketId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/tickets/${ticketId}/history`);
  }

  // Get ticket comments
  getTicketComments(ticketId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/tickets/${ticketId}/comments`);
  }

  // Update own profile
  updateOwnProfile(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/staff/profile/update`, data);
  }

  // Change own password
  changePassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff/profile/change-password`, data);
  }
}
