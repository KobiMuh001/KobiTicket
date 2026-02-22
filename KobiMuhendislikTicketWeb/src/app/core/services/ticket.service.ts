import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Ticket {
  id: number;
  ticketCode?: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  createdDate: string;
  updatedDate?: string;
  assignedPerson?: string;
  assignedStaffId?: number;
  imagePath?: string;
  tenantId: number;
  productId?: number;
  tenant?: {
    companyName: string;
    email?: string;
    phoneNumber?: string;
  };
  product?: {
    name: string;
    description?: string;
  };
}

export interface TicketComment {
  id: number;
  message: string;
  author: string;
  isAdmin: boolean;
  createdDate: string;
}

export interface TicketHistory {
  id: number;
  action: string;
  performedBy: string;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getAllTickets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/admin/all-tickets`);
  }

  getTicketById(id: number | string): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/tickets/${id}`);
  }

  updateTicketStatus(id: number | string, status: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/tickets/${id}/status`, { newStatus: status });
  }

  updateTicketPriority(id: number | string, priority: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/tickets/${id}/priority`, { newPriority: priority });
  }

  assignTicket(id: number | string, staffId: number, note?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/tickets/${id}/assign-to-staff`, { staffId, note });
  }

  addAdminComment(ticketId: number | string, message: string, author: string, isAdmin: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/tickets/${ticketId}/comments`, {
      message,
      author,
      isAdmin
    });
  }

  getComments(ticketId: number | string): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/${ticketId}/comments`);
  }

  getHistory(ticketId: number | string): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/${ticketId}/history`);
  }

  // Customer methods
  getMyTickets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/my-tickets`);
  }

  getTicketImages(ticketId: number | string): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/${ticketId}/images`);
  }

  createTicket(ticket: { title: string; description: string; priority: number; productId?: number }): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/create-ticket`, ticket);
  }

  addComment(ticketId: number | string, message: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/${ticketId}/comments`, { message });
  }

  uploadTicketImage(ticketId: number | string, formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/${ticketId}/upload-image`, formData);
  }
}
