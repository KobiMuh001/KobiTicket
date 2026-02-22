import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemParameterDto {
  group: string;
  // Key for legacy use (may be numeric as string); prefer `numericKey` for business logic
  key?: string | number | null;
  numericKey?: number | null;
  value?: string;
  value2?: string;
  description?: string;
  isActive: boolean;
  dataType: string;
  sortOrder?: number;
  createdDate: string;
}

export interface CreateSystemParameterDto {
  group: string;
  // numeric key (enum value). If omitted, server will assign next numeric key.
  key?: number;
  value?: string;
  value2?: string;
  description?: string;
  isActive?: boolean;
  dataType?: string;
  sortOrder?: number;
}

export interface UpdateSystemParameterDto {
  value?: string;
  value2?: string;
  description?: string;
  isActive?: boolean;
  dataType?: string;
  sortOrder?: number;
}

@Injectable({
  providedIn: 'root'
})
export class SystemParameterService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getByGroup(group: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/systemparameters/group/${group}`);
  }

  getGroups(): Observable<any> {
    return this.http.get(`${this.apiUrl}/systemparameters/groups`);
  }

  getById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/systemparameters/admin/${id}`);
  }

  getByGroupAndKey(group: string, numericKey: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/systemparameters/admin/group/${encodeURIComponent(group)}/key/${numericKey}`);
  }

  create(dto: CreateSystemParameterDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/systemparameters/admin`, dto);
  }

  update(id: number, dto: UpdateSystemParameterDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/systemparameters/admin/${id}`, dto);
  }

  updateByGroupAndKey(group: string, numericKey: number, dto: UpdateSystemParameterDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/systemparameters/admin/group/${encodeURIComponent(group)}/key/${numericKey}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/systemparameters/admin/${id}`);
  }

  deleteByGroupAndKey(group: string, numericKey: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/systemparameters/admin/group/${encodeURIComponent(group)}/key/${numericKey}`);
  }

  // Reorder items within a group. `orderedNumericKeys` is the numericKey sequence in desired order.
  reorderGroup(group: string, orderedNumericKeys: number[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/systemparameters/admin/group/${encodeURIComponent(group)}/reorder`, orderedNumericKeys);
  }
}
