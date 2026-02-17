import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductListItem {
  id: number;
  name: string;
  description?: string;
  tenantCount: number;
}

export interface ProductTenantItem {
  tenantId: number;
  companyName: string;
  email: string;
  username?: string;
  warrantyEndDate?: string;
  acquisitionDate?: string;
}

export interface ProductTenants {
  productId: number;
  productName: string;
  description?: string;
  tenants: ProductTenantItem[];
}

export interface CreateProductDto {
  name: string;
  description?: string;
}

export interface UpdateProductDto {
  name: string;
  description?: string;
}

export interface AssignProductTenantDto {
  warrantyEndDate: string;
  acquisitionDate?: string;
}

export interface UpdateProductTenantDto {
  warrantyEndDate: string;
  acquisitionDate?: string;
}

export interface TenantProductItem {
  productId: number;
  productName: string;
  description?: string;
  warrantyEndDate?: string;
  acquisitionDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProducts(): Observable<any> {
    return this.http.get(`${this.apiUrl}/products/admin/all`);
  }

  createProduct(dto: CreateProductDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/products/admin`, dto);
  }

  updateProduct(productId: number, dto: UpdateProductDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/products/admin/${productId}`, dto);
  }

  deleteProduct(productId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/products/admin/${productId}`);
  }

  getProductTenants(productId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/products/admin/${productId}/tenants`);
  }

  assignProductToTenant(productId: number, tenantId: number, dto: AssignProductTenantDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/products/admin/${productId}/tenants/${tenantId}`, dto);
  }

  updateProductTenant(productId: number, tenantId: number, dto: UpdateProductTenantDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/products/admin/${productId}/tenants/${tenantId}`, dto);
  }

  removeProductFromTenant(productId: number, tenantId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/products/admin/${productId}/tenants/${tenantId}`);
  }

  getTenantProducts(tenantId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/products/tenant/${tenantId}`);
  }
}
