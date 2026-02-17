import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, TenantProductItem } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-customer-assets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-assets.component.html',
  styleUrls: ['./customer-assets.component.scss']
})
export class CustomerAssetsComponent implements OnInit {
  products: TenantProductItem[] = [];
  filteredProducts: TenantProductItem[] = [];
  isLoading = true;
  searchQuery = '';
  tenantId: number | null = null;

  constructor(
    private productService: ProductService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.getTenantId();
    if (this.tenantId) {
      this.loadTenantProducts();
    }
  }

  private getTenantId(): void {
    const user = this.authService.getCurrentUser();
    if (user && user.identifier) {
      this.tenantId = parseInt(user.identifier, 10);
    }
  }

  loadTenantProducts(): void {
    if (!this.tenantId) {
      this.isLoading = false;
      return;
    }

    this.isLoading = true;

    this.productService.getTenantProducts(this.tenantId).subscribe({
      next: (response: any) => {
        const data = response.data || response || [];
        this.products = data.map((p: any) => ({
          productId: p.productId,
          productName: p.productName,
          description: p.description,
          warrantyEndDate: p.warrantyEndDate,
          acquisitionDate: p.acquisitionDate
        }));
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    if (!this.searchQuery) {
      this.filteredProducts = [...this.products];
    } else {
      const query = this.searchQuery.toLowerCase();
      this.filteredProducts = this.products.filter(product =>
        product.productName.toLowerCase().includes(query) ||
        product.description?.toLowerCase().includes(query)
      );
    }
  }

  getStatusClass(status: string): string {
    return 'status-active';
  }
}
