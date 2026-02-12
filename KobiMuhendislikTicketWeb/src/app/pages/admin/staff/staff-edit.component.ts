import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StaffService, Staff, UpdateStaffDto, StaffWorkload } from '../../../core/services/staff.service';

@Component({
  selector: 'app-staff-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './staff-edit.component.html',
  styleUrls: ['./staff-edit.component.scss']
})
export class StaffEditComponent implements OnInit {
  staffId: string = '';
  staff: Staff | null = null;
  workload: StaffWorkload | null = null;
  isLoading = true;
  isSaving = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Password reset modal
  showPasswordModal = false;
  newPassword: string = '';
  confirmPassword: string = '';

  // Form data
  formData: UpdateStaffDto = {
    fullName: '',
    email: '',
    phone: '',
    department: '',
    isActive: true,
    maxConcurrentTickets: 5
  };

  departmentOptions = [
    'Teknik Destek',
    'Satış',
    'Muhasebe',
    'Yönetim',
    'Diğer'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private staffService: StaffService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.staffId = id;
      this.loadStaff();
    }
  }

  loadStaff(): void {
    this.isLoading = true;
    this.error = null;

    // Load staff details and workloads in parallel
    this.staffService.getStaffById(this.staffId).subscribe({
      next: (response) => {
        const staffData = response.data;
        this.staff = staffData;
        this.formData = {
          fullName: staffData.fullName,
          email: staffData.email,
          phone: staffData.phone || '',
          department: staffData.department || '',
          isActive: staffData.isActive,
          maxConcurrentTickets: staffData.maxConcurrentTickets
        };
        this.loadWorkload();
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Personel bilgileri yüklenemedi.';
        this.isLoading = false;
      }
    });
  }

  loadWorkload(): void {
    this.staffService.getStaffWorkloads().subscribe({
      next: (response: any) => {
        const data = response?.data?.items || response?.data?.$values || response?.data || response;
        const workloads: StaffWorkload[] = Array.isArray(data) ? data : [];
        const staffId = Number(this.staffId);
        this.workload = workloads.find(w => Number(w.staffId ?? w.id) === staffId) || null;
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });
  }

  saveStaff(): void {
    if (!this.isFormValid()) return;

    this.isSaving = true;
    this.error = null;
    this.successMessage = null;

    this.staffService.updateStaff(this.staffId, this.formData).subscribe({
      next: (updatedStaff) => {
        this.staff = updatedStaff;
        this.successMessage = 'Personel başarıyla güncellendi.';
        this.isSaving = false;
        
        // Auto-hide success message
        setTimeout(() => {
          this.successMessage = null;
        }, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Personel güncellenirken bir hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  isFormValid(): boolean {
    return !!(
      this.formData.fullName?.trim() &&
      this.formData.email?.trim() &&
      (this.formData.maxConcurrentTickets ?? 0) > 0
    );
  }

  getInitials(name?: string | null): string {
    if (!name) return '--';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  getCapacityPercentage(): number {
    if (!this.workload) return 0;
    const max = this.formData.maxConcurrentTickets ?? 1;
    return Math.min(100, (this.workload.assignedTickets / max) * 100);
  }

  getCurrentLoad(): number {
    if (!this.workload) return 0;
    return this.workload.assignedTickets || 0;
  }

  getCapacityClass(): string {
    const percentage = this.getCapacityPercentage();
    if (percentage >= 100) return 'full';
    if (percentage >= 75) return 'warning';
    return '';
  }

  formatPhoneNumber(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, '');
    
    if (value.length > 10) {
      value = value.substring(0, 10);
    }
    
    if (value.length >= 7) {
      value = `(${value.substring(0, 3)}) ${value.substring(3, 6)} ${value.substring(6)}`;
    } else if (value.length >= 4) {
      value = `(${value.substring(0, 3)}) ${value.substring(3)}`;
    } else if (value.length >= 1) {
      value = `(${value}`;
    }
    
    this.formData.phone = value;
  }

  dismissError(): void {
    this.error = null;
  }

  dismissSuccess(): void {
    this.successMessage = null;
  }

  resetPassword(): void {
    this.newPassword = '';
    this.confirmPassword = '';
    this.showPasswordModal = true;
  }

  confirmResetPassword(): void {
    if (!this.newPassword || !this.confirmPassword) {
      this.error = 'Lütfen şifreyi iki kez giriniz.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Şifreler eşleşmiyor.';
      return;
    }

    if (this.newPassword.length < 6) {
      this.error = 'Şifre en az 6 karakter olmalıdır.';
      return;
    }

    // Büyük harf kontrolü
    if (!/[A-Z]/.test(this.newPassword)) {
      this.error = 'Şifre en az bir büyük harf içermelidir.';
      return;
    }

    // Küçük harf kontrolü
    if (!/[a-z]/.test(this.newPassword)) {
      this.error = 'Şifre en az bir küçük harf içermelidir.';
      return;
    }

    // Sayı kontrolü
    if (!/[0-9]/.test(this.newPassword)) {
      this.error = 'Şifre en az bir sayı içermelidir.';
      return;
    }

    this.isSaving = true;
    this.error = null;

    this.staffService.resetStaffPassword(this.staffId, this.newPassword).subscribe({
      next: () => {
        this.successMessage = 'Şifre başarıyla sıfırlandı.';
        this.showPasswordModal = false;
        this.isSaving = false;
        
        setTimeout(() => {
          this.successMessage = null;
        }, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Şifre sıfırlanırken bir hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  cancelResetPassword(): void {
    this.showPasswordModal = false;
    this.newPassword = '';
    this.confirmPassword = '';
  }

  // Password validation helper methods
  hasUppercase(): boolean {
    return /[A-Z]/.test(this.newPassword);
  }

  hasLowercase(): boolean {
    return /[a-z]/.test(this.newPassword);
  }

  hasNumber(): boolean {
    return /[0-9]/.test(this.newPassword);
  }

  hasMinLength(): boolean {
    return this.newPassword.length >= 6;
  }
}