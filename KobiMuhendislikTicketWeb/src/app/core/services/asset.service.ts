import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Asset {
  id: string;
  productName: string;
  serialNumber: string;
  status: string;
  warrantyEndDate: string;
  isUnderWarranty: boolean;
  tenantId: string;
  tenantName: string;
  ticketCount: number;
  createdDate: string;
}

export interface AssetDetail {
  id: string;
  productName: string;
  serialNumber: string;
  status: string;
  warrantyEndDate: string;
  isUnderWarranty: boolean;
  daysUntilWarrantyExpires: number;
  createdDate: string;
  updatedDate?: string;
  tenantId: string;
  tenantName: string;
  tenantEmail: string;
  totalTickets: number;
  openTickets: number;
  resolvedTickets: number;
}

export interface CreateAssetDto {
  productName: string;
  serialNumber: string;
  tenantId: string;
  warrantyEndDate?: string;
}

export interface UpdateAssetDto {
  productName: string;
  serialNumber: string;
  status: string;
  tenantId: string;
  warrantyEndDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class AssetService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAssets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/assets/admin/all`);
  }

  getAssetById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/assets/admin/${id}`);
  }

  createAsset(asset: CreateAssetDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/assets/admin/add-asset`, asset);
  }

  updateAsset(id: string, asset: UpdateAssetDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/assets/admin/${id}`, asset);
  }

  deleteAsset(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/assets/admin/${id}`);
  }

  // Customer methods
  getMyAssets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/assets/my-assets`);
  }
}
