import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemParameterDto {
  id: number;
  group: string;
  key: string;
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
  key: string;
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

  create(dto: CreateSystemParameterDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/systemparameters/admin`, dto);
  }

  update(id: number, dto: UpdateSystemParameterDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/systemparameters/admin/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/systemparameters/admin/${id}`);
  }
}
