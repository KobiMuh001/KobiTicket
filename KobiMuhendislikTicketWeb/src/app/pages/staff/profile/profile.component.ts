import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StaffService } from '../../../core/services/staff.service';

@Component({
  selector: 'app-staff-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class StaffProfileComponent implements OnInit {
  profile: any = null;
  workload: any = null;
  isLoading = true;
  isSaving = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Edit mode
  isEditMode = false;
  editFormData = {
    fullName: '',
    phone: ''
  };

  // Password change
  showPasswordModal = false;
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  passwordErrors: { [key: string]: string } = {};

  constructor(private staffService: StaffService) {}

  ngOnInit(): void {
    this.loadProfile();
    this.loadWorkload();
  }

  loadProfile(): void {
    this.staffService.getMyProfile().subscribe({
      next: (res) => {
        if (res.success) {
          this.profile = res.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadWorkload(): void {
    this.staffService.getMyWorkload().subscribe({
      next: (res) => {
        if (res.success) {
          this.workload = res.data;
        }
      }
    });
  }

  // Edit Mode Methods
  enterEditMode(): void {
    this.isEditMode = true;
    this.editFormData = {
      fullName: this.profile.fullName || '',
      phone: this.profile.phone || ''
    };
    this.error = null;
    this.successMessage = null;
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.error = null;
  }

  saveProfile(): void {
    if (!this.editFormData.fullName?.trim()) {
      this.error = 'Ad soyad boş olamaz.';
      return;
    }

    this.isSaving = true;
    this.error = null;
    this.successMessage = null;

    this.staffService.updateOwnProfile(this.editFormData).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Profil başarıyla güncellendi.';
          this.isEditMode = false;
          this.loadProfile();
          
          setTimeout(() => {
            this.successMessage = null;
          }, 3000);
        }
        this.isSaving = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Profil güncellenirken bir hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  // Password Change Methods
  openPasswordModal(): void {
    this.showPasswordModal = true;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.passwordErrors = {};
    this.error = null;
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.passwordErrors = {};
  }

  validatePassword(): void {
    this.passwordErrors = {};

    if (!this.passwordForm.currentPassword) {
      this.passwordErrors['currentPassword'] = 'Mevcut şifre gereklidir.';
    }

    if (!this.passwordForm.newPassword) {
      this.passwordErrors['newPassword'] = 'Yeni şifre gereklidir.';
      return;
    }

    if (this.passwordForm.newPassword.length < 6) {
      this.passwordErrors['newPassword'] = 'Şifre en az 6 karakter olmalıdır.';
    }

    if (!/[A-Z]/.test(this.passwordForm.newPassword)) {
      this.passwordErrors['newPassword'] = 'Şifre en az bir büyük harf içermelidir.';
    }

    if (!/[a-z]/.test(this.passwordForm.newPassword)) {
      this.passwordErrors['newPassword'] = 'Şifre en az bir küçük harf içermelidir.';
    }

    if (!/[0-9]/.test(this.passwordForm.newPassword)) {
      this.passwordErrors['newPassword'] = 'Şifre en az bir sayı içermelidir.';
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.passwordErrors['confirmPassword'] = 'Şifreler eşleşmiyor.';
    }
  }

  changePassword(): void {
    this.validatePassword();

    if (Object.keys(this.passwordErrors).length > 0) {
      return;
    }

    this.isSaving = true;
    this.error = null;

    const changePasswordDto = {
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword,
      confirmNewPassword: this.passwordForm.confirmPassword
    };

    this.staffService.changePassword(changePasswordDto).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Şifre başarıyla değiştirildi.';
          this.closePasswordModal();
          
          setTimeout(() => {
            this.successMessage = null;
          }, 3000);
        }
        this.isSaving = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Şifre değiştirilirken bir hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  hasPasswordError(field: string): boolean {
    return !!this.passwordErrors[field];
  }

  getPasswordError(field: string): string {
    return this.passwordErrors[field] || '';
  }

  hasUppercase(): boolean {
    return /[A-Z]/.test(this.passwordForm.newPassword);
  }

  hasLowercase(): boolean {
    return /[a-z]/.test(this.passwordForm.newPassword);
  }

  hasNumber(): boolean {
    return /[0-9]/.test(this.passwordForm.newPassword);
  }

  hasMinLength(): boolean {
    return this.passwordForm.newPassword.length >= 6;
  }

  passwordsMatch(): boolean {
    return this.passwordForm.newPassword === this.passwordForm.confirmPassword &&
           this.passwordForm.newPassword.length > 0;
  }

  hasPasswordErrors(): boolean {
    return Object.keys(this.passwordErrors).length > 0;
  }
}
