import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TenantService, Tenant, CreateTenantDto } from '../../../core/services/tenant.service';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './tenants.component.html',
  styleUrls: ['./tenants.component.scss']
})
export class TenantsComponent implements OnInit {
  tenants: Tenant[] = [];
  filteredTenants: Tenant[] = [];
  isLoading = true;
  errorMessage = '';

  // Filters
  searchTerm = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  // Delete Modal
  showDeleteModal = false;
  tenantToDelete: Tenant | null = null;
  forceDelete = false;
  isDeleting = false;

  // Create Modal
  showCreateModal = false;
  isCreating = false;
  createForm: CreateTenantDto = {
    companyName: '',
    taxNumber: '',
    email: '',
    username: '',
    password: '',
    phoneNumber: ''
  };
  createError = '';

  constructor(private tenantService: TenantService) { }

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.isLoading = true;
    this.tenantService.getTenants(this.currentPage, this.pageSize, this.searchTerm).subscribe({
      next: (response: any) => {
        let tenants: Tenant[] = [];
        if (response.data) {
          if (response.data.items) {
            tenants = Array.isArray(response.data.items) ? response.data.items : response.data.items.$values || [];
            this.totalCount = response.data.totalCount || tenants.length;
          } else if (Array.isArray(response.data)) {
            tenants = response.data;
            this.totalCount = tenants.length;
          } else if (response.data.$values) {
            tenants = response.data.$values;
            this.totalCount = tenants.length;
          }
        } else if (Array.isArray(response)) {
          tenants = response;
          this.totalCount = tenants.length;
        }
        this.tenants = tenants;
        this.filteredTenants = tenants;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Müşteriler yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    if (!this.searchTerm) {
      this.filteredTenants = this.tenants;
      return;
    }

    const search = this.searchTerm.toLowerCase();
    this.filteredTenants = this.tenants.filter(tenant =>
      tenant.companyName?.toLowerCase().includes(search) ||
      tenant.email?.toLowerCase().includes(search) ||
      tenant.username?.toLowerCase().includes(search) ||
      tenant.taxNumber?.toLowerCase().includes(search) ||
      tenant.phoneNumber?.toLowerCase().includes(search)
    );
  }

  searchTenants(): void {
    this.currentPage = 1;
    this.loadTenants();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadTenants();
  }

  // Delete Modal
  openDeleteModal(tenant: Tenant): void {
    this.tenantToDelete = tenant;
    this.forceDelete = false;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.tenantToDelete = null;
    this.forceDelete = false;
  }

  confirmDelete(): void {
    if (!this.tenantToDelete) return;

    this.isDeleting = true;
    this.tenantService.deleteTenant(this.tenantToDelete.id, this.forceDelete).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.loadTenants();
        this.isDeleting = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Müşteri silinirken hata oluştu.';
        this.isDeleting = false;
      }
    });
  }

  // Create Modal
  openCreateModal(): void {
    this.createForm = {
      companyName: '',
      taxNumber: '',
      email: '',
      username: '',
      password: '',
      phoneNumber: ''
    };
    this.createError = '';
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  createTenant(): void {
    if (!this.createForm.companyName || !this.createForm.taxNumber || !this.createForm.email || !this.createForm.password) {
      this.createError = 'Lütfen zorunlu alanları doldurun.';
      return;
    }

    // Password validation: min 8 chars, at least one uppercase and one lowercase
    const password = this.createForm.password || '';
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    if (password.length < 8 || !hasUpper || !hasLower) {
      this.createError = 'Şifre en az 8 karakter olmalı, büyük ve küçük harf içermelidir.';
      return;
    }
    // Tax number must be exactly 10 digits
    const taxDigits = (this.createForm.taxNumber || '').toString().replace(/\D/g, '');
    if (taxDigits.length !== 10) {
      this.createError = 'Vergi numarası 10 haneli bir sayı olmalıdır.';
      return;
    }
    // store cleaned digits
    this.createForm.taxNumber = taxDigits;


    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(this.createForm.email)) {
      this.createError = 'Geçerli bir e-posta adresi girin. (örn: ornek@firma.com)';
      return;
    }

    this.isCreating = true;
    this.createError = '';

    this.tenantService.createTenant(this.createForm).subscribe({
      next: () => {
        this.closeCreateModal();
        this.loadTenants();
        this.isCreating = false;
      },
      error: (error) => {
        this.createError = error.error?.message || 'Müşteri oluşturulurken hata oluştu.';
        this.isCreating = false;
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
        year: 'numeric'
      });
    } catch {
      return '-';
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadTenants();
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

    this.createForm.phoneNumber = formatted;
    input.value = formatted;
  }

  formatTaxNumber(event: Event): void {
    const input = event.target as HTMLInputElement;
    // Keep only digits and limit to 10
    const digits = (input.value || '').toString().replace(/\D/g, '').slice(0, 10);
    this.createForm.taxNumber = digits;
    // reflect immediately in the input so user cannot type past 10 digits
    try { input.value = digits; } catch (e) { }
  }
}
