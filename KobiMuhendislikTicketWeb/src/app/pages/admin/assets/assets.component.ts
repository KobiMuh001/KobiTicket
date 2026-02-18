import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, AdminTenantProductItem } from '../../../core/services/product.service';

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './assets.component.html',
  styleUrls: ['./assets.component.scss']
})
export class AssetsComponent implements OnInit {
  items: AdminTenantProductItem[] = [];
  filteredItems: AdminTenantProductItem[] = [];
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';

  showEditModal = false;
  editingItem: AdminTenantProductItem | null = null;
  editWarrantyEndDate = '';
  editAcquisitionDate = '';
  // remove-confirm modal state
  showRemoveConfirm = false;
  itemToRemove: AdminTenantProductItem | null = null;

  // Pagination
  currentPage = 1;
  itemsPerPage = 10;
  totalPages = 1;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems(): void {
    this.isLoading = true;
    this.productService.getAllTenantProductsForAdmin().subscribe({
      next: (response) => {
        this.items = response?.data || [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Müşteri ürünleri yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    let filtered = this.items;

    if (this.searchTerm) {
      const term = this.normalizeText(this.searchTerm);
      filtered = filtered.filter(x =>
        this.normalizeText(x.productName).includes(term) ||
        this.normalizeText(x.tenantName).includes(term) ||
        this.normalizeText(x.tenantEmail).includes(term)
      );
    }

    this.totalPages = Math.ceil(filtered.length / this.itemsPerPage);
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    this.filteredItems = filtered.slice(startIndex, startIndex + this.itemsPerPage);
  }

  openEditModal(item: AdminTenantProductItem): void {
    this.editingItem = item;
    this.editWarrantyEndDate = this.toDateInput(item.warrantyEndDate);
    this.editAcquisitionDate = this.toDateInput(item.acquisitionDate);
    this.errorMessage = '';
    this.successMessage = '';
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingItem = null;
    this.editWarrantyEndDate = '';
    this.editAcquisitionDate = '';
  }

  saveEditModal(): void {
    if (!this.editingItem) {
      return;
    }

    if (!this.editWarrantyEndDate) {
      this.errorMessage = 'Garanti bitiş tarihi zorunludur.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.updateProductTenant(this.editingItem.productId, this.editingItem.tenantId, {
      warrantyEndDate: this.editWarrantyEndDate,
      acquisitionDate: this.editAcquisitionDate || undefined
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Müşteri ürün bilgisi güncellendi.';
        this.closeEditModal();
        this.loadItems();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Güncelleme sırasında bir hata oluştu.';
      }
    });
  }

  // perform removal without browser confirm (used by modal confirm)
  removeItem(item: AdminTenantProductItem): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.removeProductFromTenant(item.productId, item.tenantId).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.successMessage = response?.message || 'Ürün müşteri ilişkisinden kaldırıldı.';
        this.loadItems();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error?.error?.message || 'Kaldırma sırasında bir hata oluştu.';
      }
    });
  }

  openRemoveConfirm(item: AdminTenantProductItem): void {
    this.itemToRemove = item;
    this.showRemoveConfirm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeRemoveConfirm(): void {
    this.showRemoveConfirm = false;
    this.itemToRemove = null;
  }

  confirmRemoveItem(): void {
    if (!this.itemToRemove) return;
    const item = this.itemToRemove;
    this.closeRemoveConfirm();
    this.removeItem(item);
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

  private normalizeText(value: string): string {
    return (value || '')
      .toLocaleLowerCase('tr-TR')
      .normalize('NFKD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim()
      .replace(/\s+/g, ' ');
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFilter();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.applyFilter();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);
    
    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }
}
