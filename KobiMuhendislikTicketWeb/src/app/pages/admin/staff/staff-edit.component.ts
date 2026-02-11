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
    return Math.min(100, (this.workload.openTickets / max) * 100);
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
}
