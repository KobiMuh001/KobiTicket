import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { Subscription, forkJoin } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface TicketComment {
  id: string;
  message: string;
  authorName: string;
  isAdminReply: boolean;
  createdDate: string;
}

@Component({
  selector: 'app-customer-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-ticket-detail.component.html',
  styleUrls: ['./customer-ticket-detail.component.scss']
})
export class CustomerTicketDetailComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('commentsContainer') private commentsContainer!: ElementRef;
  
  ticket: any = null;
  comments: TicketComment[] = [];
  ticketImages: string[] = [];
  isLoading = true;
  isSubmittingComment = false;
  showChat = false;
  newComment = '';
  errorMessage = '';
  baseUrl = environment.baseUrl;
  statusOptions: any[] = [];
  priorityOptions: any[] = [];
  selectedFiles: File[] = [];
  isUploadingFile = false;
  uploadSuccessMessage = '';
  showImagePreview = false;
  selectedImagePath: string | null = null;
  
  // SignalR
  private ticketId: string = '';
  private commentSubscription?: Subscription;
  private isSignalRConnected = false;
  private shouldScrollToBottom = false;
  private isBrowser: boolean;
  private refreshInterval: any;

  constructor(
    private ticketService: TicketService,
    private signalRService: SignalRService,
    private systemParameterService: SystemParameterService,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.ticketId = id;
      this.loadLookups();
      this.loadTicket(id);
      this.loadComments(id);
      this.loadImages(id);
      if (this.isBrowser) {
        this.initSignalR();
      }
      
      // Commentsları 2 saniyede bir otomatik yenile (polling)
      this.refreshInterval = setInterval(() => {
        console.log('Customer-Detail: Periodic comments refresh triggered');
        this.loadComments(id);
      }, 2000);
    }
  }

  loadLookups(): void {
    this.systemParameterService.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const data = res.data || res || [];
        this.statusOptions = (data || []).map((s: any) => ({
          id: s.id,
          key: s.key,
          label: s.value ?? s.description ?? s.key,
          sortOrder: s.sortOrder,
          color: s.value2 ?? s.color ?? null
        }));
        if (this.ticket) {
          this.ticket.statusText = this.getStatusText(this.ticket.status);
        }
      },
      error: () => {}
    });

    this.systemParameterService.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const data = res.data || res || [];
        this.priorityOptions = (data || []).map((p: any) => ({
          id: p.id,
          key: p.key,
          label: p.value ?? p.description ?? p.key,
          sortOrder: p.sortOrder,
          color: p.value2 ?? p.color ?? null
        }));
        if (this.ticket) {
          this.ticket.priorityText = this.getPriorityText(this.ticket.priority);
        }
      },
      error: () => {}
    });
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    if (this.ticketId) {
      this.signalRService.leaveTicketGroup(this.ticketId);
    }
    this.commentSubscription?.unsubscribe();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  private scrollToBottom(): void {
    try {
      if (this.commentsContainer) {
        this.commentsContainer.nativeElement.scrollTop = this.commentsContainer.nativeElement.scrollHeight;
      }
    } catch (err) {}
  }

  openImagePreview(imagePath: string): void {
    this.selectedImagePath = imagePath;
    this.showImagePreview = true;
  }

  private async initSignalR(): Promise<void> {
    if (!this.isBrowser) {
      console.log('SignalR: Not in browser environment');
      return;
    }

    // Check both localStorage and sessionStorage (token can be in either depending on "Remember Me" setting)
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    
    if (!token) {
      console.warn('SignalR: No token found. User may not be authenticated.');
      return;
    }

    try {
      console.log('SignalR: Customer - Attempting to start connection');
      await this.signalRService.startConnection(token);
      
      console.log('SignalR: Customer - Joining ticket group', this.ticketId);
      await this.signalRService.joinTicketGroup(this.ticketId);
      this.isSignalRConnected = true;
      console.log('SignalR: Customer - Successfully connected and joined group');

      this.commentSubscription = this.signalRService.commentReceived$.subscribe(
        (comment: CommentMessage) => {
          console.log('Customer-Detail: Received comment from SignalR:', comment);
          if (comment.ticketId === this.ticketId) {
            const exists = this.comments.some(c => c.id === comment.id);
            if (!exists) {
              console.log('Customer-Detail: Adding new comment to list');
              this.comments.push({
                id: comment.id,
                message: comment.message,
                authorName: comment.authorName,
                isAdminReply: comment.isAdminReply,
                createdDate: comment.createdDate
              });
              this.shouldScrollToBottom = true;
            } else {
              console.log('Customer-Detail: Comment already exists, skipping');
            }
          }
        }
      );
    } catch (error) {
      console.error('SignalR: Connection failed:', error);
      this.isSignalRConnected = false;
      console.warn('SignalR: Real-time updates disabled for customer');
    }
  }

  loadTicket(id: string): void {
    this.isLoading = true;
    
    this.ticketService.getMyTickets().subscribe({
      next: (response: any) => {
        const data = response.data || response || [];
        const found = data.find((t: any) => t.id.toString() === id);
        if (found) {
          this.ticket = {
            ...found,
            statusText: this.getStatusText(found.status),
            priorityText: this.getPriorityText(found.priority),
            assetName: found.productName || found.asset?.productName || found.asset?.name || found.assetName
          };
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Ticket yüklenirken bir hata oluştu.';
      }
    });
  }

  loadComments(ticketId: string): void {
    this.ticketService.getComments(ticketId).subscribe({
      next: (response: any) => {
        this.comments = response.data || response || [];
        this.shouldScrollToBottom = true;
      },
      error: () => {}
    });
  }

  loadImages(ticketId: string): void {
    this.ticketService.getTicketImages(ticketId).subscribe({
      next: (response: any) => {
        this.ticketImages = response.data || response || [];
      },
      error: () => {}
    });
  }

  addComment(): void {
    if (!this.newComment.trim() || !this.ticket) return;

    // Prevent adding comments when ticket is closed
    if (this.isTicketClosed()) {
      this.errorMessage = 'Bu destek talebi üzerinde artık yorum yapılamaz.';
      return;
    }

    this.isSubmittingComment = true;
    
    this.ticketService.addComment(this.ticket.id, this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.isSubmittingComment = false;
        this.shouldScrollToBottom = true;
        // Yorumu hemen yükle ve scroll et
        this.loadComments(this.ticketId);
      },
      error: () => {
        this.errorMessage = 'Yorum eklenirken bir hata oluştu.';
        this.isSubmittingComment = false;
      }
    });
  }

  toggleChat(): void {
    this.showChat = !this.showChat;
    if (this.showChat) {
      this.shouldScrollToBottom = true;
    }
  }

  getStatusText(status: string | number): string {
    const opt = this.statusOptions.find((o: any) =>
      Number(o.sortOrder ?? o.id) === Number(status) || o.key === status || String(o.id) === String(status)
    );
    if (opt) return opt.label;

    const statusMap: { [key: string]: string; [key: number]: string } = {
      'Open': 'Açık',
      'Processing': 'İşlemde',
      'InProgress': 'İşlemde',
      'WaitingForCustomer': 'Müşteri Bekleniyor',
      'Waiting': 'Müşteri Bekleniyor',
      'Resolved': 'Çözüldü',
      'Closed': 'Kapatıldı',
      1: 'Açık',
      2: 'İşlemde',
      3: 'Müşteri Bekleniyor',
      4: 'Çözüldü',
      5: 'Kapatıldı'
    };
    return statusMap[status] ?? 'Bilinmiyor';
  }

  getPriorityText(priority: string | number): string {
    const opt = this.priorityOptions.find((o: any) =>
      Number(o.sortOrder ?? o.id) === Number(priority) || o.key === priority || String(o.id) === String(priority)
    );
    if (opt) return opt.label;

    const priorityMap: { [key: string]: string; [key: number]: string } = {
      'Low': 'Düşük',
      'Medium': 'Orta',
      'High': 'Yüksek',
      'Critical': 'Kritik',
      1: 'Düşük',
      2: 'Orta',
      3: 'Yüksek',
      4: 'Kritik'
    };
    return priorityMap[priority] ?? 'Bilinmiyor';
  }

  getStatusClass(status: string | number): string {
    const statusText = this.getStatusText(status);
    const classMap: { [key: string]: string } = {
      'Açık': 'status-open',
      'İşlemde': 'status-progress',
      'Müşteri Bekleniyor': 'status-waiting',
      'Çözüldü': 'status-resolved',
      'Kapatıldı': 'status-closed'
    };
    return classMap[statusText] || '';
  }

  getPriorityClass(priority: string | number): string {
    const priorityText = this.getPriorityText(priority);
    const classMap: { [key: string]: string } = {
      'Düşük': 'priority-low',
      'Orta': 'priority-medium',
      'Yüksek': 'priority-high',
      'Kritik': 'priority-critical'
    };
    return classMap[priorityText] || '';
  }

  getStatusColor(status: string | number): string | null {
    const s = String(status ?? '');
    const found = this.statusOptions.find((o: any) => String(o.sortOrder ?? o.id) === s || String(o.id) === s || String(o.key) === s || o.label === status || String(o.label) === s);
    return found?.color ?? null;
  }

  getPriorityColor(priority: string | number): string | null {
    const p = String(priority ?? '');
    const found = this.priorityOptions.find((o: any) => String(o.sortOrder ?? o.id) === p || String(o.id) === p || String(o.key) === p || o.label === priority || String(o.label) === p);
    return found?.color ?? null;
  }

  isTicketClosed(): boolean {
    if (!this.ticket) return false;
    const statusText = this.getStatusText(this.ticket.status);
    return statusText === 'Kapatıldı' || statusText === 'Çözüldü';
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;

    const selected: File[] = Array.from(files);
    const invalidType = selected.find(file => !file.type.startsWith('image/'));
    if (invalidType) {
      this.errorMessage = 'Sadece resim dosyaları yüklenebilir.';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    const oversized = selected.find(file => file.size > 5 * 1024 * 1024);
    if (oversized) {
      this.errorMessage = 'Dosya boyutu 5MB\'den küçük olmalıdır.';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    this.selectedFiles = selected;
    this.uploadImages();
  }

  uploadImages(): void {
    if (this.selectedFiles.length === 0 || !this.ticket) return;

    this.isUploadingFile = true;
    this.errorMessage = '';

    const uploads = this.selectedFiles.map(file => {
      const formData = new FormData();
      formData.append('file', file);
      return this.ticketService.uploadTicketImage(this.ticket.id, formData);
    });

    forkJoin(uploads).subscribe({
      next: () => {
        this.uploadSuccessMessage = 'Resimler başarıyla yüklendi!';
        setTimeout(() => this.uploadSuccessMessage = '', 3000);
        this.selectedFiles = [];
        this.isUploadingFile = false;
        this.resetFileInput();
        this.loadImages(this.ticketId);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Resimler yüklenirken bir hata oluştu.';
        setTimeout(() => this.errorMessage = '', 3000);
        this.selectedFiles = [];
        this.isUploadingFile = false;
        this.resetFileInput();
      }
    });
  }

  triggerFileInput(): void {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  private resetFileInput(): void {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement | null;
    if (fileInput) {
      fileInput.value = '';
    }
  }
}
