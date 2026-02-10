import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';
import { AssetService } from '../../../core/services/asset.service';

interface Asset {
  id: string;
  name: string;
  serialNumber: string;
}

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
    priority: 2, // Orta default
    assetId: null as string | null
  };

  assets: Asset[] = [];
  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  selectedFile: File | null = null;
  selectedFileName: string = '';
  imagePreviewUrl: string | null = null;

  priorities = [
    { value: 1, label: 'Düşük' },
    { value: 2, label: 'Orta' },
    { value: 3, label: 'Yüksek' },
    { value: 4, label: 'Kritik' }
  ];

  constructor(
    private ticketService: TicketService,
    private assetService: AssetService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadAssets();
    
    // Check if assetId is passed in query params
    this.route.queryParams.subscribe(params => {
      if (params['assetId']) {
        this.ticket.assetId = params['assetId'];
      }
    });
  }

  loadAssets(): void {
    this.isLoading = true;
    this.assetService.getMyAssets().subscribe({
      next: (response: any) => {
        const data = response.data || response || [];
        this.assets = data.map((a: any) => ({
          id: a.id,
          name: a.productName || a.name,
          serialNumber: a.serialNumber
        }));
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.ticket.title || !this.ticket.description) {
      this.errorMessage = 'Lütfen başlık ve açıklama alanlarını doldurun.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const ticketData = {
      title: this.ticket.title,
      description: this.ticket.description,
      priority: this.ticket.priority,
      assetId: this.ticket.assetId || undefined
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
        this.errorMessage = err.error?.message || 'Talep oluşturulurken bir hata oluştu.';
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
