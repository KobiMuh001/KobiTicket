import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { ProductService, TenantProductItem } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-customer-ticket-create',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-ticket-create.component.html',
  styleUrls: ['./customer-ticket-create.component.scss']
})
export class CustomerTicketCreateComponent implements OnInit {
  ticket = {
    title: '',
    description: '',
    priority: 2 as number | string, // Orta default (will be replaced by DB value if available)
    productId: null as number | null
  };

  products: TenantProductItem[] = [];
  tenantId: number | null = null;
  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  titleError = '';
  selectedFile: File | null = null;
  selectedFileName: string = '';
  imagePreviewUrl: string | null = null;

  priorities: { value: number | string; label: string }[] = [
    { value: 1, label: 'Düşük' },
    { value: 2, label: 'Orta' },
    { value: 3, label: 'Yüksek' },
    { value: 4, label: 'Kritik' }
  ];

  // Will be populated from DB (TicketPriority group). Keep default above for fallback.
  priorityOptions: { value: number | string; label: string }[] = [];

  constructor(
    private ticketService: TicketService,
    private productService: ProductService,
    private authService: AuthService,
    private systemParameterService: SystemParameterService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.getTenantId();
    this.loadLookups();
    if (this.tenantId) {
      this.loadProducts();
    }
    
    // Check if productId is passed in query params
    this.route.queryParams.subscribe(params => {
      if (params['productId']) {
        this.ticket.productId = parseInt(params['productId'], 10);
      }
    });
  }

  loadLookups(): void {
    this.systemParameterService.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const data = res.data || res || [];
        this.priorityOptions = (data || []).map((p: any) => ({
          value: Number(p.sortOrder ?? p.id),
          label: p.value ?? p.description ?? p.key
        }));

        if (this.priorityOptions.length) {
          // Replace the displayed priorities (used by template) with DB-driven ones
          this.priorities = this.priorityOptions.map(p => ({ value: p.value, label: p.label }));

          // If current default priority isn't present in options, set to first option
          const hasDefault = this.priorities.some(pr => Number(pr.value) === Number(this.ticket.priority));
          if (!hasDefault) {
            this.ticket.priority = Number(this.priorities[0].value);
          }
        }
      },
      error: () => {}
    });
  }

  private getTenantId(): void {
    const user = this.authService.getCurrentUser();
    if (user && user.identifier) {
      this.tenantId = parseInt(user.identifier, 10);
    }
  }

  loadProducts(): void {
    if (!this.tenantId) {
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.productService.getTenantProducts(this.tenantId).subscribe({
      next: (response: any) => {
        this.products = response.data || response || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    this.titleError = '';
    if (!this.ticket.title || !this.ticket.description) {
      this.errorMessage = 'Lütfen başlık ve açıklama alanlarını doldurun.';
      return;
    }

    // Title length validation: at least 3 characters
    if ((this.ticket.title || '').trim().length < 3) {
      this.titleError = 'Başlık en az 3 karakter olmalıdır.';
      this.errorMessage = this.titleError;
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const ticketData = {
      title: this.ticket.title,
      description: this.ticket.description,
      priority: Number(this.ticket.priority),
      productId: this.ticket.productId || undefined
    };

    this.ticketService.createTicket(ticketData).subscribe({
      next: (response: any) => {
        const ticketId = response.ticketId;
        
        // Eğer resim seçildiyse, ticket oluşturduktan sonra yükle
        if (this.selectedFile && ticketId) {
          this.uploadImage(ticketId);
        } else {
          this.successMessage = 'Destek talebiniz başarıyla oluşturuldu!';
          this.isSubmitting = false;
          
          setTimeout(() => {
            this.router.navigate(['/customer/tickets']);
          }, 1500);
        }
      },
      error: (err) => {
        // Try to extract field-specific validation messages from backend
        const backend = err.error || {};
        let message = 'Talep oluşturulurken bir hata oluştu.';

        const fieldMap: { [key: string]: string } = {
          title: 'Başlık',
          description: 'Açıklama',
          productId: 'Ürün',
          priority: 'Öncelik'
        };

        if (backend.errors && typeof backend.errors === 'object') {
          const parts: string[] = [];
          Object.keys(backend.errors).forEach(key => {
            const msgs = backend.errors[key];
            const label = fieldMap[key] || key;
            if (Array.isArray(msgs)) {
              parts.push(`${label}: ${msgs.join(', ')}`);
            } else if (typeof msgs === 'string') {
              parts.push(`${label}: ${msgs}`);
            }
          });
          if (parts.length) message = parts.join(' | ');
        } else if (backend.message) {
          message = backend.message;
        } else if (err.error && typeof err.error === 'string') {
          message = err.error;
        }

        this.errorMessage = message;
        this.isSubmitting = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    
    if (file) {
      // Dosya boyutu kontrolü (5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage = 'Dosya boyutu 5MB\'ı geçemez.';
        return;
      }

      // Dosya türü kontrolü
      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        this.errorMessage = 'Yalnızca JPG, PNG, GIF ve WebP dosyaları yüklenebilir.';
        return;
      }

      this.selectedFile = file;
      this.selectedFileName = file.name;
      this.errorMessage = '';

      // Resim önizlemesi oluştur
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreviewUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  clearFile(): void {
    this.selectedFile = null;
    this.selectedFileName = '';
    this.imagePreviewUrl = null;
  }

  uploadImage(ticketId: string): void {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.ticketService.uploadTicketImage(ticketId, formData).subscribe({
      next: () => {
        this.successMessage = 'Destek talebiniz ve resim başarıyla yüklendi!';
        this.isSubmitting = false;
        this.clearFile();
        
        setTimeout(() => {
          this.router.navigate(['/customer/tickets']);
        }, 1500);
      },
      error: (err) => {
        // Ticket oluşturuldu fakat resim yüklenemedi
        this.successMessage = 'Destek talebiniz oluşturuldu ancak resim yüklenemedi.';
        this.isSubmitting = false;
        
        setTimeout(() => {
          this.router.navigate(['/customer/tickets']);
        }, 1500);
      }
    });
  }
}
