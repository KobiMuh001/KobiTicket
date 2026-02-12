import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssetService, Asset, CreateAssetDto } from '../../../core/services/asset.service';
import { TenantService } from '../../../core/services/tenant.service';

interface Tenant {
  id: string;
  companyName: string;
}

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './assets.component.html',
  styleUrls: ['./assets.component.scss']
})
export class AssetsComponent implements OnInit {
  assets: Asset[] = [];
  filteredAssets: Asset[] = [];
  tenants: Tenant[] = [];
  isLoading = true;
  errorMessage = '';
  searchTerm = '';

  // Modal states
  showCreateModal = false;
  showDeleteModal = false;
  assetToDelete: Asset | null = null;
  isSubmitting = false;

  // Create form
  newAsset: CreateAssetDto = {
    productName: '',
    serialNumber: '',
    tenantId: '',
    warrantyEndDate: ''
  };

  // Pagination
  currentPage = 1;
  itemsPerPage = 10;
  totalPages = 1;

  constructor(
    private assetService: AssetService,
    private tenantService: TenantService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAssets();
    this.loadTenants();
  }

  loadAssets(): void {
    this.isLoading = true;
    this.assetService.getAssets().subscribe({
      next: (response) => {
        this.assets = response.data || [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Varlıklar yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  loadTenants(): void {
    this.tenantService.getTenants().subscribe({
      next: (response) => {
        this.tenants = response.data?.items || response.data || [];
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });
  }

  applyFilter(): void {
    let filtered = this.assets;

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(a => 
        a.productName.toLowerCase().includes(term) ||
        a.serialNumber.toLowerCase().includes(term) ||
        a.tenantName.toLowerCase().includes(term)
      );
    }

    this.totalPages = Math.ceil(filtered.length / this.itemsPerPage);
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    this.filteredAssets = filtered.slice(startIndex, startIndex + this.itemsPerPage);
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

  // Create Modal
  openCreateModal(): void {
    this.newAsset = {
      productName: '',
      serialNumber: '',
      tenantId: '',
      warrantyEndDate: this.getDefaultWarrantyDate()
    };
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  getDefaultWarrantyDate(): string {
    const date = new Date();
    date.setFullYear(date.getFullYear() + 2);
    return date.toISOString().split('T')[0];
  }

  createAsset(): void {
    if (!this.newAsset.productName || !this.newAsset.serialNumber || !this.newAsset.tenantId) {
      this.errorMessage = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.assetService.createAsset(this.newAsset).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.loadAssets();
        this.isSubmitting = false;
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.error?.success === false && error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Varlık oluşturulurken bir hata oluştu.';
        }
      }
    });
  }

  // Delete Modal
  openDeleteModal(asset: Asset): void {
    this.assetToDelete = asset;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.assetToDelete = null;
  }

  confirmDelete(): void {
    if (!this.assetToDelete) return;

    this.isSubmitting = true;
    this.assetService.deleteAsset(this.assetToDelete.id).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.assetToDelete = null;
        this.loadAssets();
        this.isSubmitting = false;
      },
      error: () => {
        this.errorMessage = 'Varlık silinirken bir hata oluştu.';
        this.isSubmitting = false;
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'aktif': return 'status-active';
      case 'pasif': return 'status-inactive';
      case 'bakımda': return 'status-maintenance';
      default: return 'status-active';
    }
  }

  isWarrantyExpired(asset: Asset): boolean {
    const warrantyDate = new Date(asset.warrantyEndDate);
    const now = new Date();
    return warrantyDate < now;
  }

  isWarrantyExpiringSoon(asset: Asset): boolean {
    const warrantyDate = new Date(asset.warrantyEndDate);
    const now = new Date();
    if (warrantyDate < now) return false; // Zaten süresi dolmuş
    
    const threeMonthsLater = new Date();
    threeMonthsLater.setMonth(threeMonthsLater.getMonth() + 3);
    return warrantyDate <= threeMonthsLater;
  }

  onAssetSelect(assetId: string): void {
    if (!assetId) return;
    this.router.navigate(['/admin/assets', assetId]);
  }
}
