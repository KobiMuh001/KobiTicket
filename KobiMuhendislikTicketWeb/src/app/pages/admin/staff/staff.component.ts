import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StaffService, Staff, StaffWorkload, CreateStaffDto } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './staff.component.html',
  styleUrls: ['./staff.component.scss']
})
export class StaffComponent implements OnInit {
  staffList: Staff[] = [];
  workloads: StaffWorkload[] = [];
  filteredStaff: Staff[] = [];
  isLoading = true;
  errorMessage = '';
  searchTerm = '';
  activeFilter: 'all' | 'active' | 'inactive' = 'all';

  // Modal states
  showCreateModal = false;
  showDeleteModal = false;
  staffToDelete: Staff | null = null;
  isSubmitting = false;

  // Create form
  newStaff: CreateStaffDto = {
    fullName: '',
    email: '',
    password: '',
    phone: '',
    departmentId: 1,
    maxConcurrentTickets: 10
  };

  defaultMaxConcurrentTickets = 10;

  // department options now hold { id, label } so we can send numeric departmentId
  departmentOptions: { id: number; label: string }[] = [
    { id: 1, label: 'Teknik Destek' },
    { id: 2, label: 'Satış' },
    { id: 3, label: 'Muhasebe' },
    { id: 4, label: 'Yönetim' },
    { id: 5, label: 'Diğer' }
  ];

  // Pagination
  currentPage = 1;
  itemsPerPage = 10;
  totalPages = 1;

  constructor(private staffService: StaffService, private paramSvc: SystemParameterService) {}

  ngOnInit(): void {
    this.loadStaff();
    this.loadWorkloads();
    this.loadDefaultMaxConcurrentTickets();
    this.loadDepartments();
  }

  private loadDepartments(): void {
    // Try common group names, fallback to existing static list
    this.paramSvc.getByGroup('Department').subscribe({
      next: (res: any) => {
        const data = res?.data || res || [];
        if (Array.isArray(data) && data.length) {
          this.departmentOptions = data.map((d: any) => ({
            id: Number(d.id ?? d.key ?? 0) || 0,
            label: (d.value ?? d.description ?? d.key ?? String(d.id)).toString()
          }));
          return;
        }
        // try plural
        this.paramSvc.getByGroup('Departments').subscribe({
          next: (res2: any) => {
            const data2 = res2?.data || res2 || [];
            if (Array.isArray(data2) && data2.length) {
              this.departmentOptions = data2.map((d: any) => ({
                id: Number(d.id ?? d.key ?? 0) || 0,
                label: (d.value ?? d.description ?? d.key ?? String(d.id)).toString()
              }));
            }
          },
          error: () => {}
        });
      },
      error: () => {
        // keep defaults
      }
    });
  }

  private loadDefaultMaxConcurrentTickets(): void {
    this.paramSvc.getByGroup('General').subscribe({
      next: (res: any) => {
        const data = res?.data?.data || res?.data || res || [];
        const list = Array.isArray(data) ? data : [];
        const found = list.find((p: any) => (p.key || p.Key || '').toLowerCase() === 'defaultticketlimit'.toLowerCase());
        if (found && found.value != null) {
          const v = Number(found.value);
          if (!Number.isNaN(v) && v > 0) {
            this.defaultMaxConcurrentTickets = v;
          }
        }
      },
      error: () => {
        // keep default
      }
    });
  }

  loadStaff(): void {
    this.isLoading = true;
    // activeOnly parametresi göndermiyoruz - tüm personelleri getir
    this.staffService.getAllStaff().subscribe({
      next: (response) => {
        this.staffList = response.data || [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Personel listesi yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  loadWorkloads(): void {
    this.staffService.getStaffWorkloads().subscribe({
      next: (response) => {
        this.workloads = response.data || [];
      },
      error: () => {
        // Production'da hata detayları gizlenir
      }
    });
  }

  getWorkload(staffId: number | string): StaffWorkload | undefined {
    const id = typeof staffId === 'number' ? staffId : Number(staffId);
    return this.workloads.find(w => Number(w.id ?? 0) === id);
  }

  applyFilter(): void {
    let filtered = this.staffList;

    // Active filter
    if (this.activeFilter === 'active') {
      filtered = filtered.filter(s => s.isActive);
    } else if (this.activeFilter === 'inactive') {
      filtered = filtered.filter(s => !s.isActive);
    }

    // Search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(s =>
        s.fullName.toLowerCase().includes(term) ||
        s.email.toLowerCase().includes(term) ||
        s.department.toLowerCase().includes(term)
      );
    }

    this.totalPages = Math.ceil(filtered.length / this.itemsPerPage);
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    this.filteredStaff = filtered.slice(startIndex, startIndex + this.itemsPerPage);
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFilter();
  }

  onFilterChange(): void {
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
    this.newStaff = {
      fullName: '',
      email: '',
      password: '',
      phone: '',
      departmentId: this.departmentOptions && this.departmentOptions.length ? this.departmentOptions[0].id : 1,
      maxConcurrentTickets: this.defaultMaxConcurrentTickets
    };
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  formatPhoneNumber(event: any): void {
    let value = event.target.value.replace(/\D/g, '');
    
    if (value.length > 0) {
      if (value.startsWith('0')) {
        value = value.substring(0, 11);
      } else {
        value = '0' + value.substring(0, 10);
      }
      
      let formatted = '';
      if (value.length > 0) formatted += value.substring(0, 1);
      if (value.length > 1) formatted += ' (' + value.substring(1, 4);
      if (value.length > 4) formatted += ') ' + value.substring(4, 7);
      if (value.length > 7) formatted += ' ' + value.substring(7, 9);
      if (value.length > 9) formatted += ' ' + value.substring(9, 11);
      
      this.newStaff.phone = formatted;
    } else {
      this.newStaff.phone = '';
    }
  }

  createStaff(): void {
    if (!this.newStaff.fullName || !this.newStaff.email || !this.newStaff.password) {
      this.errorMessage = 'Lütfen ad soyad, e-posta ve şifre alanlarını doldurun.';
      return;
    }

    // Email validation
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(this.newStaff.email)) {
      this.errorMessage = 'Lütfen geçerli bir e-posta adresi girin.';
      return;
    }

    // Password validation
    if (this.newStaff.password.length < 6) {
      this.errorMessage = 'Şifre en az 6 karakter olmalıdır.';
      return;
    }

    this.isSubmitting = true;
    this.staffService.createStaff(this.newStaff).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.loadStaff();
        this.loadWorkloads();
        this.isSubmitting = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Personel oluşturulurken bir hata oluştu.';
        this.isSubmitting = false;
      }
    });
  }

  // Delete Modal
  openDeleteModal(staff: Staff): void {
    this.staffToDelete = staff;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.staffToDelete = null;
  }

  confirmDelete(): void {
    if (!this.staffToDelete) return;

    this.isSubmitting = true;
    this.staffService.deleteStaff(this.staffToDelete.id).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.staffToDelete = null;
        this.loadStaff();
        this.loadWorkloads();
        this.isSubmitting = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Personel silinirken bir hata oluştu.';
        this.isSubmitting = false;
      }
    });
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'status-active' : 'status-inactive';
  }

  getAvailabilityClass(workload: StaffWorkload | undefined): string {
    if (!workload) return '';
    return workload.isAvailable ? 'available' : 'busy';
  }
}
