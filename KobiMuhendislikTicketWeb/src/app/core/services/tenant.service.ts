import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Tenant {
  id: string;
  companyName: string;
  taxNumber: string;
  email: string;
  username?: string;
  phoneNumber: string;
  logoUrl?: string;
  createdDate: string;
  ticketCount?: number;
  assetCount?: number;
  openTicketCount?: number;
}

export interface TenantDetail extends Tenant {
  tickets?: any[];
  assets?: any[];
}

export interface UpdateTenantDto {
  companyName?: string;
  email?: string;
  username?: string;
  phoneNumber?: string;
  logoUrl?: string;
}

export interface CreateTenantDto {
  companyName: string;
  taxNumber: string;
  email: string;
  username?: string;
  password: string;
  phoneNumber: string;
}

export interface ResetPasswordDto {
  newPassword: string;
}

export interface TenantsResponse {
  items: Tenant[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTenants(page: number = 1, pageSize: number = 20, search?: string): Observable<any> {
    let url = `${this.apiUrl}/admin/tenants?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get(url);
  }

  getTenantById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/tenants/${id}`);
  }

  updateTenant(id: string, dto: UpdateTenantDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/admin/tenants/${id}`, dto);
  }

  resetPassword(id: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/tenants/${id}/reset-password`, { newPassword });
  }

  deleteTenant(id: string, forceDelete: boolean = false): Observable<any> {
    return this.http.delete(`${this.apiUrl}/admin/tenants/${id}?forceDelete=${forceDelete}`);
  }

  createTenant(dto: CreateTenantDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/tenants/admin/create-tenant`, dto);
  }

  // Customer methods
  getMyProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/tenants/me`);
  }

  updateMyProfile(dto: { companyName?: string; email?: string; phoneNumber?: string; logoUrl?: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/tenants/me`, dto);
  }

  uploadMyLogo(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/tenants/me/upload-logo`, formData);
  }

  changePassword(dto: { currentPassword: string; newPassword: string; confirmPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/tenants/me/change-password`, {
      currentPassword: dto.currentPassword,
      newPassword: dto.newPassword,
      confirmNewPassword: dto.confirmPassword
    });
  }
}
