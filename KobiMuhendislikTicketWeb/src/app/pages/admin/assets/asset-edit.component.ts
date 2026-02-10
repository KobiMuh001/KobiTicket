import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AssetService, AssetDetail, UpdateAssetDto } from '../../../core/services/asset.service';
import { TenantService } from '../../../core/services/tenant.service';

interface Tenant {
  id: string;
  companyName: string;
}

@Component({
  selector: 'app-asset-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './asset-edit.component.html',
  styleUrls: ['./asset-edit.component.scss']
})
export class AssetEditComponent implements OnInit {
  assetId = '';
  asset: AssetDetail | null = null;
  tenants: Tenant[] = [];
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Edit form
  editForm: UpdateAssetDto = {
    productName: '',
    serialNumber: '',
    status: 'Aktif',
    tenantId: '',
    warrantyEndDate: ''
  };

  statusOptions = ['Aktif', 'Pasif', 'Bakımda'];

  // Delete modal
  showDeleteModal = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private assetService: AssetService,
    private tenantService: TenantService
  ) {}

  ngOnInit(): void {
    this.assetId = this.route.snapshot.paramMap.get('id') || '';
    if (this.assetId) {
      this.loadAsset();
      this.loadTenants();
    }
  }

  loadAsset(): void {
    this.isLoading = true;
    this.assetService.getAssetById(this.assetId).subscribe({
      next: (response) => {
        this.asset = response.data;
        if (this.asset) {
          this.editForm = {
            productName: this.asset.productName,
            serialNumber: this.asset.serialNumber,
            status: this.asset.status,
            tenantId: this.asset.tenantId,
            warrantyEndDate: this.formatDateForInput(this.asset.warrantyEndDate)
          };
        }
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Varlık yüklenirken bir hata oluştu.';
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

  formatDateForInput(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  }

  saveAsset(): void {
    if (!this.editForm.productName || !this.editForm.serialNumber || !this.editForm.tenantId) {
      this.errorMessage = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.assetService.updateAsset(this.assetId, this.editForm).subscribe({
      next: () => {
        this.successMessage = 'Varlık başarıyla güncellendi.';
        this.isSaving = false;
        this.loadAsset();
      },
      error: (error) => {
        this.isSaving = false;
        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.error?.success === false && error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Varlık güncellenirken bir hata oluştu.';
        }
      }
    });
  }

  // Delete
  openDeleteModal(): void {
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }

  confirmDelete(): void {
    this.assetService.deleteAsset(this.assetId).subscribe({
      next: () => {
        this.router.navigate(['/admin/assets']);
      },
      error: () => {
        this.errorMessage = 'Varlık silinirken bir hata oluştu.';
        this.showDeleteModal = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/assets']);
  }
}
