import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TenantService, Tenant, UpdateTenantDto } from '../../../../core/services/tenant.service';

@Component({
  selector: 'app-tenant-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './tenant-edit.component.html',
  styleUrls: ['./tenant-edit.component.scss']
})
export class TenantEditComponent implements OnInit {
  tenant: Tenant | null = null;
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';
  
  // Edit Form
  editForm: UpdateTenantDto = {
    companyName: '',
    email: '',
    username: '',
    phoneNumber: ''
  };
  
  // Password Reset
  showPasswordModal = false;
  newPassword = '';
  confirmPassword = '';
  isResettingPassword = false;
  passwordError = '';

  // Delete Modal
  showDeleteModal = false;
  forceDelete = false;
  isDeleting = false;

  // Tabs
  activeTab = 'details';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private tenantService: TenantService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadTenant(id);
    }
  }

  loadTenant(id: string): void {
    this.isLoading = true;
    this.tenantService.getTenantById(id).subscribe({
      next: (response: any) => {
        this.tenant = response.data || response;
        if (this.tenant) {
          this.editForm = {
            companyName: this.tenant.companyName,
            email: this.tenant.email,
            username: this.tenant.username,
            phoneNumber: this.tenant.phoneNumber
          };
        }
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Müşteri bilgileri yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  saveChanges(): void {
    if (!this.tenant) return;
    
    // Email format kontrolü
    if (this.editForm.email) {
      const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      if (!emailRegex.test(this.editForm.email)) {
        this.errorMessage = 'Geçerli bir e-posta adresi girin. (örn: ornek@firma.com)';
        return;
      }
    }
    
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    this.tenantService.updateTenant(this.tenant.id, this.editForm).subscribe({
      next: () => {
        this.successMessage = 'Müşteri bilgileri başarıyla güncellendi.';
        this.isSaving = false;
        if (this.tenant) {
          this.tenant.companyName = this.editForm.companyName || this.tenant.companyName;
          this.tenant.email = this.editForm.email || this.tenant.email;
          this.tenant.username = this.editForm.username || this.tenant.username;
          this.tenant.phoneNumber = this.editForm.phoneNumber || this.tenant.phoneNumber;
        }
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Güncelleme sırasında hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  // Password Reset
  openPasswordModal(): void {
    this.newPassword = '';
    this.confirmPassword = '';
    this.passwordError = '';
    this.showPasswordModal = true;
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
  }

  resetPassword(): void {
    if (!this.tenant) return;
    
    if (this.newPassword.length < 6) {
      this.passwordError = 'Şifre en az 6 karakter olmalıdır.';
      return;
    }
    
    if (this.newPassword !== this.confirmPassword) {
      this.passwordError = 'Şifreler eşleşmiyor.';
      return;
    }
    
    this.isResettingPassword = true;
    this.passwordError = '';
    
    this.tenantService.resetPassword(this.tenant.id, this.newPassword).subscribe({
      next: () => {
        this.successMessage = 'Şifre başarıyla sıfırlandı.';
        this.closePasswordModal();
        this.isResettingPassword = false;
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.passwordError = error.error?.message || 'Şifre sıfırlanırken hata oluştu.';
        this.isResettingPassword = false;
      }
    });
  }

  // Delete
  openDeleteModal(): void {
    this.forceDelete = false;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }

  confirmDelete(): void {
    if (!this.tenant) return;
    
    this.isDeleting = true;
    
    this.tenantService.deleteTenant(this.tenant.id, this.forceDelete).subscribe({
      next: () => {
        this.router.navigate(['/admin/tenants']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Müşteri silinirken hata oluştu.';
        this.isDeleting = false;
        this.closeDeleteModal();
      }
    });
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '-';
      return date.toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return '-';
    }
  }

  formatPhoneNumber(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Sadece rakamları al
    
    if (value.length > 11) {
      value = value.substring(0, 11);
    }
    
    let formatted = '';
    
    if (value.length > 0) {
      formatted = value.substring(0, 1); // 0
    }
    if (value.length > 1) {
      formatted += ' (' + value.substring(1, 4); // (5XX
    }
    if (value.length > 4) {
      formatted += ') ' + value.substring(4, 7); // ) XXX
    }
    if (value.length > 7) {
      formatted += ' ' + value.substring(7, 9); // XX
    }
    if (value.length > 9) {
      formatted += ' ' + value.substring(9, 11); // XX
    }
    
    this.editForm.phoneNumber = formatted;
    input.value = formatted;
  }
}
