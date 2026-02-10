import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TenantService } from '../../../core/services/tenant.service';

interface Profile {
  id: number;
  companyName: string;
  contactName: string;
  email: string;
  phoneNumber: string;
  address: string;
  createdAt: string;
}

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-profile.component.html',
  styleUrls: ['./customer-profile.component.scss']
})
export class CustomerProfileComponent implements OnInit {
  profile: Profile | null = null;
  isLoading = true;
  isEditing = false;
  isSaving = false;
  isChangingPassword = false;
  isChangingPasswordView = false;
  
  editForm = {
    phoneNumber: '',
    address: ''
  };

  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  successMessage = '';
  errorMessage = '';
  passwordSuccess = '';
  passwordError = '';

  constructor(private tenantService: TenantService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    
    this.tenantService.getMyProfile().subscribe({
      next: (response: any) => {
        const data = response.data || response;
        this.profile = {
          id: data.id,
          companyName: data.companyName,
          contactName: data.contactName,
          email: data.email,
          phoneNumber: data.phoneNumber,
          address: data.address,
          createdAt: data.createdDate || data.createdAt
        };
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Profil bilgileri yüklenirken bir hata oluştu.';
      }
    });
  }

  startEditing(): void {
    if (this.profile) {
      this.editForm = {
        phoneNumber: this.profile.phoneNumber || '',
        address: this.profile.address || ''
      };
      this.isEditing = true;
      this.successMessage = '';
      this.errorMessage = '';
    }
  }

  cancelEditing(): void {
    this.isEditing = false;
    this.errorMessage = '';
  }

  saveProfile(): void {
    this.isSaving = true;
    this.errorMessage = '';

    // DTO mapping - frontend property adlarını backend DTO'suyla eşleştir
    const updateDto = {
      phoneNumber: this.editForm.phoneNumber
    };

    this.tenantService.updateMyProfile(updateDto).subscribe({
      next: () => {
        if (this.profile) {
          this.profile.phoneNumber = this.editForm.phoneNumber;
          this.profile.address = this.editForm.address;
        }
        this.successMessage = 'Profil bilgileriniz başarıyla güncellendi.';
        this.isEditing = false;
        this.isSaving = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Güncelleme sırasında bir hata oluştu.';
        this.isSaving = false;
      }
    });
  }

  formatPhoneNumber(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Sadece rakamları al
    
    // Maksimum 11 karakter
    if (value.length > 11) {
      value = value.substring(0, 11);
    }
    
    // Format: 0(XXX) XXX XX XX
    let formatted = '';
    if (value.length > 0) {
      formatted = value.substring(0, 1); // 0
    }
    if (value.length > 1) {
      formatted += '(' + value.substring(1, 4); // (XXX
    }
    if (value.length >= 4) {
      formatted += ') '; // )
    }
    if (value.length > 4) {
      formatted += value.substring(4, 7); // XXX
    }
    if (value.length > 7) {
      formatted += ' ' + value.substring(7, 9); // XX
    }
    if (value.length > 9) {
      formatted += ' ' + value.substring(9, 11); // XX
    }
    
    this.editForm.phoneNumber = formatted;
  }

  validatePassword(): void {
    const password = this.passwordForm.newPassword;
    
    // Şifre validasyon kuralları
    if (password.length < 8) {
      this.passwordError = 'Şifre en az 8 karakter olmalıdır.';
      return;
    }
    
    if (!/[A-Z]/.test(password)) {
      this.passwordError = 'Şifre en az bir büyük harf içermelidir.';
      return;
    }
    
    if (!/[a-z]/.test(password)) {
      this.passwordError = 'Şifre en az bir küçük harf içermelidir.';
      return;
    }
    
    if (!/[0-9]/.test(password)) {
      this.passwordError = 'Şifre en az bir rakam içermelidir.';
      return;
    }
    
    this.passwordError = '';
  }

  changePassword(): void {
    this.passwordError = '';
    this.passwordSuccess = '';

    // Validasyonlar
    if (!this.passwordForm.currentPassword) {
      this.passwordError = 'Mevcut şifrenizi girin.';
      return;
    }

    if (!this.passwordForm.newPassword) {
      this.passwordError = 'Yeni şifrenizi girin.';
      return;
    }

    if (!this.passwordForm.confirmPassword) {
      this.passwordError = 'Şifre doğrulamasını girin.';
      return;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.passwordError = 'Yeni şifreler eşleşmiyor.';
      return;
    }

    if (this.passwordForm.currentPassword === this.passwordForm.newPassword) {
      this.passwordError = 'Yeni şifre eski şifreden farklı olmalıdır.';
      return;
    }

    // Backend validasyonunu çalıştır
    this.validatePassword();
    if (this.passwordError) {
      return;
    }

    this.isChangingPassword = true;

    this.tenantService.changePassword({
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword,
      confirmPassword: this.passwordForm.confirmPassword
    }).subscribe({
      next: () => {
        this.successMessage = 'Şifreniz başarıyla değiştirildi.';
        this.isChangingPassword = false;
        this.isChangingPasswordView = false;
        this.resetPasswordForm();
        
        // Başarı mesajını 3 saniye sonra temizle
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (err) => {
        this.passwordError = err.error?.message || 'Şifre değiştirilirken bir hata oluştu.';
        this.isChangingPassword = false;
      }
    });
  }

  resetPasswordForm(): void {
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.passwordError = '';
    this.passwordSuccess = '';
  }

  startChangingPassword(): void {
    this.isChangingPasswordView = true;
    this.passwordError = '';
    this.passwordSuccess = '';
    this.resetPasswordForm();
  }

  cancelChangingPassword(): void {
    this.isChangingPasswordView = false;
    this.resetPasswordForm();
  }
}
