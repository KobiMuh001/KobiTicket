import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  createdDate: string;
  updatedDate?: string;
  assignedPerson?: string;
  imagePath?: string;
  tenantId: string;
  assetId?: string;
  tenant?: {
    companyName: string;
    email?: string;
    phoneNumber?: string;
  };
  asset?: {
    productName: string;
    serialNumber?: string;
  };
}

export interface TicketComment {
  id: string;
  message: string;
  author: string;
  isAdmin: boolean;
  createdDate: string;
}

export interface TicketHistory {
  id: string;
  action: string;
  performedBy: string;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAllTickets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/admin/all-tickets`);
  }

  getTicketById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/tickets/${id}`);
  }

  updateTicketStatus(id: string, status: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/tickets/${id}/status`, { newStatus: status });
  }

  updateTicketPriority(id: string, priority: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/tickets/${id}/priority`, { newPriority: priority });
  }

  assignTicket(id: string, personName: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/tickets/${id}/assign`, { personName });
  }

  addAdminComment(ticketId: string, message: string, author: string, isAdmin: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/tickets/${ticketId}/comments`, {
      message,
      author,
      isAdmin
    });
  }

  getComments(ticketId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/${ticketId}/comments`);
  }

  getHistory(ticketId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/${ticketId}/history`);
  }

  // Customer methods
  getMyTickets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/tickets/my-tickets`);
  }

  createTicket(ticket: { title: string; description: string; priority: number; assetId?: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/create-ticket`, ticket);
  }

  addComment(ticketId: string, message: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/${ticketId}/comments`, { message });
  }

  uploadTicketImage(ticketId: string, formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/tickets/${ticketId}/upload-image`, formData);
  }
}
