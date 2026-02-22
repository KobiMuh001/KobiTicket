import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StaffService, Staff, UpdateStaffDto, StaffWorkload } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';

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
    departmentId: undefined,
    isActive: true,
    maxConcurrentTickets: 5
  };

  departmentOptions: { id: number; label: string }[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private staffService: StaffService,
    private paramSvc: SystemParameterService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.staffId = id;
      this.loadDepartments();
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
          departmentId: (staffData as any).departmentId ?? undefined,
          isActive: staffData.isActive,
          maxConcurrentTickets: staffData.maxConcurrentTickets
        };
        // If departmentOptions already loaded, resolve numeric id for current staff
        if (Array.isArray(this.departmentOptions) && this.departmentOptions.length) {
          // prefer numeric departmentId from server if present
          const srvDeptId = (staffData as any).departmentId;
          if (srvDeptId !== undefined && srvDeptId !== null) {
            this.formData.departmentId = Number(srvDeptId);
          } else if (staffData.department) {
            const match = this.departmentOptions.find(o => (o.label || '').toString() === (staffData.department || '').toString());
            if (match) this.formData.departmentId = match.id;
          }
        }
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

  private loadDepartments(): void {
    this.paramSvc.getByGroup('Department').subscribe({
      next: (res: any) => {
        const data = res?.data || res || [];
        if (Array.isArray(data) && data.length) {
          this.departmentOptions = data.map((d: any) => ({
            id: (typeof d.numericKey === 'number' && !Number.isNaN(d.numericKey)) ? d.numericKey : (Number(d.id ?? d.key ?? 0) || 0),
            label: (d.value ?? d.description ?? d.key ?? String(d.numericKey ?? d.id ?? '')).toString()
          }));
          // If staff already loaded, try to resolve its current department to numeric id
          if (this.staff && this.staff.department) {
            const match = this.departmentOptions.find(o => o.label === this.staff!.department);
            if (match) this.formData.departmentId = match.id;
          }
          return;
        }
      },
      error: () => {
        // keep empty list
      }
    });
  }

  saveStaff(): void {
    if (!this.isFormValid()) return;

    // Phone validation: if provided, must be exactly 11 digits (Turkish format)
    const phoneDigits = (this.formData.phone || '').toString().replace(/\D/g, '');
    if (phoneDigits && phoneDigits.length !== 11) {
      this.error = 'Lütfen geçerli ve eksiksiz bir telefon numarası girin.';
      return;
    }

    // Ensure phone is consistently formatted before sending (if provided)
    if (phoneDigits && phoneDigits.length === 11) {
      const v = phoneDigits;
      this.formData.phone = v.substring(0, 1) + ' (' + v.substring(1, 4) + ') ' + v.substring(4, 7) + ' ' + v.substring(7, 9) + ' ' + v.substring(9, 11);
    }

    this.isSaving = true;
    this.error = null;
    this.successMessage = null;

    // Ensure we send numeric departmentId when available
    const payload: any = { ...this.formData };
    if (this.formData.departmentId !== undefined && this.formData.departmentId !== null) payload.departmentId = this.formData.departmentId;

    this.staffService.updateStaff(this.staffId, payload).subscribe({
      next: () => {
        this.successMessage = 'Personel başarıyla güncellendi.';
        this.isSaving = false;
        // Reload staff to get fresh DTO (including DepartmentId)
        this.loadStaff();
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

    // Only allow up to 11 digits (Turkish format: leading 0 + 10 digits)
    let digits = (input.value || '').toString().replace(/\D/g, '').slice(0, 11);

    // If user entered 10 digits without leading zero, prepend a 0 to form 11
    if (digits.length === 10 && digits[0] !== '0') {
      digits = '0' + digits;
    }

    if (digits.length === 0) {
      this.formData.phone = '';
      try { input.value = ''; } catch (e) {}
      return;
    }

    // Build formatted string progressively but never exceed 11 digits
    const v = digits;
    let formatted = '';
    if (v.length > 0) formatted += v.substring(0, 1);
    if (v.length > 1) formatted += ' (' + v.substring(1, Math.min(4, v.length));
    if (v.length > 4) formatted += ') ' + v.substring(4, Math.min(7, v.length));
    if (v.length > 7) formatted += ' ' + v.substring(7, Math.min(9, v.length));
    if (v.length > 9) formatted += ' ' + v.substring(9, Math.min(11, v.length));

    this.formData.phone = formatted;

    // Force the input element's displayed value to the formatted value
    try {
      if (input) {
        input.value = formatted;
        // Move caret to the end so user cannot continue typing past the format
        const endPos = input.value.length;
        if (typeof input.setSelectionRange === 'function') {
          input.setSelectionRange(endPos, endPos);
        }
      }
    } catch (e) {
      // ignore if not applicable
    }
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