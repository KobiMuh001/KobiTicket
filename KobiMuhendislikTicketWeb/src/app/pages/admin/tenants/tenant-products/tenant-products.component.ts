import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  ProductListItem,
  ProductService,
  TenantProductItem,
  UpdateProductTenantDto
} from '../../../../core/services/product.service';
import { TenantService } from '../../../../core/services/tenant.service';

@Component({
  selector: 'app-tenant-products',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './tenant-products.component.html',
  styleUrls: ['./tenant-products.component.scss']
})
export class TenantProductsComponent implements OnInit {
  tenantId = 0;
  tenantName = '';
  products: TenantProductItem[] = [];
  allProducts: ProductListItem[] = [];

  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  showAssignModal = false;
  selectedProductId: number | null = null;
  warrantyEndDate = '';
  acquisitionDate = '';

  showEditModal = false;
  editingProductId: number | null = null;
  editData: UpdateProductTenantDto = {
    warrantyEndDate: '',
    acquisitionDate: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private tenantService: TenantService
  ) {}

  ngOnInit(): void {
    this.tenantId = Number(this.route.snapshot.paramMap.get('id'));

    if (!this.tenantId || Number.isNaN(this.tenantId)) {
      this.errorMessage = 'Geçersiz müşteri id.';
      this.isLoading = false;
      return;
    }

    this.loadTenant();
    this.loadAllProducts();
    this.loadTenantProducts();
  }

  loadTenant(): void {
    this.tenantService.getTenantById(String(this.tenantId)).subscribe({
      next: (response) => {
        const tenant = response?.data || response;
        this.tenantName = tenant?.companyName || '';
      }
    });
  }

  loadAllProducts(): void {
    this.productService.getProducts().subscribe({
      next: (response) => {
        this.allProducts = response?.data || [];
      }
    });
  }

  loadTenantProducts(): void {
    this.isLoading = true;
    this.productService.getTenantProductsForAdmin(this.tenantId).subscribe({
      next: (response) => {
        this.products = response?.data || [];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Müşterinin ürünleri yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/tenants', this.tenantId]);
  }

  openAssignModal(): void {
    this.selectedProductId = null;
    this.warrantyEndDate = '';
    this.acquisitionDate = '';
    this.errorMessage = '';
    this.successMessage = '';
    this.showAssignModal = true;
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.selectedProductId = null;
    this.warrantyEndDate = '';
    this.acquisitionDate = '';
  }

  assignProduct(): void {
    if (!this.selectedProductId) {
      this.errorMessage = 'Lütfen ürün seçin.';
      return;
    }

    if (!this.warrantyEndDate) {
      this.errorMessage = 'Garanti bitiş tarihi zorunludur.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.assignProductToTenant(this.selectedProductId, this.tenantId, {
      warrantyEndDate: this.warrantyEndDate,
      acquisitionDate: this.acquisitionDate || undefined
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Ürün müşteriye atandı.';
        this.closeAssignModal();
        this.loadTenantProducts();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Ürün atama sırasında hata oluştu.';
      }
    });
  }

  openEditModal(productId: number): void {
    const relation = this.products.find(p => p.productId === productId);
    if (!relation) {
      return;
    }

    this.editingProductId = productId;
    this.editData = {
      warrantyEndDate: this.toDateInput(relation.warrantyEndDate),
      acquisitionDate: this.toDateInput(relation.acquisitionDate)
    };
    this.errorMessage = '';
    this.successMessage = '';
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingProductId = null;
  }

  saveEditModal(): void {
    if (!this.editingProductId) {
      return;
    }

    if (!this.editData.warrantyEndDate) {
      this.errorMessage = 'Garanti bitiş tarihi zorunludur.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.updateProductTenant(this.editingProductId, this.tenantId, {
      warrantyEndDate: this.editData.warrantyEndDate,
      acquisitionDate: this.editData.acquisitionDate || undefined
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Ürün bilgileri güncellendi.';
        this.closeEditModal();
        this.loadTenantProducts();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Güncelleme sırasında hata oluştu.';
      }
    });
  }

  removeProduct(productId: number): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.removeProductFromTenant(productId, this.tenantId).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Ürün müşteriden kaldırıldı.';
        this.loadTenantProducts();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Ürün kaldırılırken hata oluştu.';
      }
    });
  }

  private toDateInput(value?: string): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
