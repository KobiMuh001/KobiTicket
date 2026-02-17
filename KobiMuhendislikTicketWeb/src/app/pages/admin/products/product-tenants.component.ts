import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService, ProductTenants, UpdateProductTenantDto } from '../../../core/services/product.service';
import { TenantService } from '../../../core/services/tenant.service';

interface TenantOption {
  id: number;
  companyName: string;
}

@Component({
  selector: 'app-product-tenants',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-tenants.component.html',
  styleUrls: ['./product-tenants.component.scss']
})
export class ProductTenantsComponent implements OnInit {
  productId = 0;
  product: ProductTenants | null = null;
  isLoading = true;
  errorMessage = '';
  successMessage = '';
  isSaving = false;

  tenants: TenantOption[] = [];
  searchTenantQuery = '';
  selectedTenantName = '';
  selectedTenantId: number | null = null;
  warrantyEndDate = '';
  acquisitionDate = '';
  showDropdown = false;

  showEditModal = false;
  showAssignModal = false;
  editingTenantId: number | null = null;
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
    this.productId = Number(this.route.snapshot.paramMap.get('id'));

    if (!this.productId || Number.isNaN(this.productId)) {
      this.errorMessage = 'Geçersiz ürün id.';
      this.isLoading = false;
      return;
    }

    this.loadProductTenants();
    this.loadAllTenants();
  }

  loadAllTenants(): void {
    this.tenantService.getTenants(1, 500).subscribe({
      next: (response) => {
        const items = response?.data?.items || response?.data || [];
        this.tenants = (items as any[]).map(t => ({
          id: Number(t.id),
          companyName: t.companyName
        }));
      }
    });
  }

  loadProductTenants(): void {
    this.isLoading = true;
    this.productService.getProductTenants(this.productId).subscribe({
      next: (response) => {
        this.product = response?.data || null;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ürün firma bilgileri yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/products']);
  }

  assignTenant(): void {
    if (!this.product || !this.selectedTenantId) {
      this.errorMessage = 'Lütfen bir firma seçin.';
      return;
    }

    if (!this.warrantyEndDate) {
      this.errorMessage = 'Garanti bitiş tarihi zorunludur.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.assignProductToTenant(this.product.productId, this.selectedTenantId, {
      warrantyEndDate: this.warrantyEndDate,
      acquisitionDate: this.acquisitionDate || undefined
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Firma ürüne atandı.';
        this.selectedTenantId = null;
        this.warrantyEndDate = '';
        this.acquisitionDate = '';
        this.closeAssignModal();
        this.loadProductTenants();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Firma atama sırasında bir hata oluştu.';
      }
    });
  }

  removeTenant(tenantId: number): void {
    if (!this.product) return;

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.removeProductFromTenant(this.product.productId, tenantId).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Firma ürün ilişkisinden kaldırıldı.';
        this.loadProductTenants();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Kaldırma sırasında bir hata oluştu.';
      }
    });
  }

  openEditModal(tenantId: number): void {
    if (!this.product) return;

    const relation = this.product.tenants.find(t => t.tenantId === tenantId);
    if (!relation) return;

    this.editingTenantId = tenantId;
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
    this.editingTenantId = null;
  }

  openAssignModal(): void {
    this.selectedTenantId = null;
    this.warrantyEndDate = '';
    this.acquisitionDate = '';
    this.errorMessage = '';
    this.successMessage = '';
    this.showAssignModal = true;
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.selectedTenantId = null;
    this.selectedTenantName = '';
    this.warrantyEndDate = '';
    this.acquisitionDate = '';
    this.searchTenantQuery = '';
  }

  saveEditModal(): void {
    if (!this.product || !this.editingTenantId) return;

    if (!this.editData.warrantyEndDate) {
      this.errorMessage = 'Garanti bitiş tarihi zorunludur.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.updateProductTenant(this.product.productId, this.editingTenantId, {
      warrantyEndDate: this.editData.warrantyEndDate,
      acquisitionDate: this.editData.acquisitionDate || undefined
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Ürün-firma bilgisi güncellendi.';
        this.closeEditModal();
        this.loadProductTenants();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Güncelleme sırasında bir hata oluştu.';
      }
    });
  }

  private toDateInput(value?: string): string {
    if (!value) return '';

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getFilteredTenants(): TenantOption[] {
    if (!this.searchTenantQuery.trim()) {
      return this.tenants;
    }
    const query = this.searchTenantQuery.toLowerCase();
    return this.tenants.filter(t => 
      t.companyName.toLowerCase().includes(query)
    );
  }

  openDropdown(): void {
    this.showDropdown = true;
  }

  closeDropdown(): void {
    setTimeout(() => {
      this.showDropdown = false;
    }, 150);
  }

  selectTenant(tenantId: number, companyName: string): void {
    this.selectedTenantId = tenantId;
    this.selectedTenantName = companyName;
    this.searchTenantQuery = '';
    this.showDropdown = false;
  }
}
