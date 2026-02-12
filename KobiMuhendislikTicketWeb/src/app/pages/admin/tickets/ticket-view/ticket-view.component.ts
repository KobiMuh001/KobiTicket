import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../../core/services/ticket.service';
import { SignalRService, CommentMessage } from '../../../../core/services/signalr.service';
import { environment } from '../../../../../environments/environment';
import { Subscription } from 'rxjs';

interface TicketComment {
  id: string;
  message: string;
  authorName: string;
  isAdminReply: boolean;
  isStaff?: boolean;
  createdDate: string;
}

interface TicketHistoryItem {
  description: string;
  actionBy: string;
  createdDate: string;
}

interface TicketDetail {
  id: number;
  ticketCode?: string;
  title: string;
  description: string;
  status: string | number;
  priority: string | number;
  assignedPerson?: string;
  imagePath?: string;
  imagePaths?: string[];
  createdDate: string;
  updatedDate?: string;
  tenantId: number;
  tenantName: string;
  tenantEmail?: string;
  tenantPhone?: string;
  assetId?: number;
  assetName?: string;
  assetSerialNumber?: string;
  assetUnderWarranty?: boolean;
  comments: TicketComment[];
  history: TicketHistoryItem[];
}

@Component({
  selector: 'app-ticket-view',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-view.component.html',
  styleUrls: ['./ticket-view.component.scss']
})
export class TicketViewComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('commentsContainer') private commentsContainer!: ElementRef;
  
  ticket: TicketDetail | null = null;
  comments: TicketComment[] = [];
  history: TicketHistoryItem[] = [];
  loading = true;
  error = '';

  newComment = '';
  addingComment = false;

  activeTab: 'details' | 'comments' | 'history' = 'details';
  private shouldScrollToBottom = false;
  private commentSubscription?: Subscription;
  isSignalRConnected = false;
  private isBrowser: boolean;
  baseUrl = environment.baseUrl;
  showImagePreview = false;
  selectedImagePath: string | null = null;

  statusDisplayMap: { [key: string]: string; [key: number]: string } = {
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

  priorityDisplayMap: { [key: string]: string; [key: number]: string } = {
    'Low': 'Düşük',
    'Medium': 'Orta',
    'High': 'Yüksek',
    'Critical': 'Kritik',
    1: 'Düşük',
    2: 'Orta',
    3: 'Yüksek',
    4: 'Kritik'
  };

  priorityClassMap: { [key: string]: string; [key: number]: string } = {
    'Low': 'low',
    'Medium': 'normal',
    'High': 'high',
    'Critical': 'critical',
    1: 'low',
    2: 'normal',
    3: 'high',
    4: 'critical'
  };

  statusStringToNumber: { [key: string]: number } = {
    'Open': 1,
    'Processing': 2,
    'WaitingForCustomer': 3,
    'Waiting': 3,
    'Resolved': 4,
    'Closed': 5
  };

  priorityStringToNumber: { [key: string]: number } = {
    'Low': 1,
    'Medium': 2,
    'High': 3,
    'Critical': 4
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ticketService: TicketService,
    private signalRService: SignalRService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadTicket(id);
      if (this.isBrowser) {
        this.initSignalR(id);
      }
    }
  }

  ngOnDestroy(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.signalRService.leaveTicketGroup(id);
    }
    this.commentSubscription?.unsubscribe();
  }

  openImagePreview(imagePath: string): void {
    this.selectedImagePath = imagePath;
    this.showImagePreview = true;
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom && this.activeTab === 'comments') {
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

  public scrollToCommentsBottom(): void {
    setTimeout(() => this.scrollToBottom(), 0);
  }

  loadTicket(id: string): void {
    this.loading = true;
    this.ticketService.getTicketById(id).subscribe({
      next: (response: any) => {
        const data = response?.data?.data || response?.data || response;
        
        if (typeof data.status === 'string') {
          data.status = this.statusStringToNumber[data.status] || 1;
        }
        if (typeof data.priority === 'string') {
          data.priority = this.priorityStringToNumber[data.priority] || 1;
        }
        
        this.ticket = data;
        
        // Process comments to mark staff messages
        let comments = data.comments?.$values || data.comments || [];
        comments = comments.map((c: TicketComment) => {
          // If isAdminReply is true but authorName is not "Admin", it's a staff message
          if (c.isAdminReply && c.authorName !== 'Admin') {
            return { ...c, isStaff: true };
          }
          return c;
        });
        this.comments = comments;
        
        this.history = data.history?.$values || data.history || [];
        this.loading = false;
        this.shouldScrollToBottom = true;
      },
      error: () => {
        this.error = 'Talep yüklenirken bir hata oluştu';
        this.loading = false;
      }
    });
  }

  formatTicketId(id: number | string | null | undefined, ticketCode?: string | null): string {
    // Backend'den gelen TicketCode'u kullan (T00001 formatı)
    if (ticketCode) return ticketCode;
    // Fallback: ID'den formatlama
    if (id === null || id === undefined) return '-';
    const numericId = typeof id === 'number' ? id : Number(id);
    return Number.isFinite(numericId) ? `T${numericId.toString().padStart(5, '0')}` : String(id);
  }

  getStatusLabel(status: string | number): string {
    return this.statusDisplayMap[status] ?? 'Bilinmiyor';
  }

  getStatusClass(status: string | number): string {
    const statusMap: { [key: string]: string; [key: number]: string } = {
      'Open': 'open',
      'Processing': 'processing',
      'InProgress': 'processing',
      'WaitingForCustomer': 'waiting',
      'Waiting': 'waiting',
      'Resolved': 'resolved',
      'Closed': 'closed',
      1: 'open',
      2: 'processing',
      3: 'waiting',
      4: 'resolved',
      5: 'closed'
    };
    return statusMap[status] ?? '';
  }

  getPriorityLabel(priority: string | number): string {
    return this.priorityDisplayMap[priority] ?? 'Normal';
  }

  getPriorityClass(priority: string | number): string {
    return this.priorityClassMap[priority] ?? 'normal';
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
  }

  formatFullDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleString('tr-TR');
  }

  goBack(): void {
    this.router.navigate(['/admin/tickets']);
  }

  goToEdit(): void {
    if (this.ticket) {
      this.router.navigate(['/admin/tickets', this.ticket.id, 'edit']);
    }
  }

  addComment(): void {
    if (!this.ticket || !this.newComment.trim()) return;
    
    this.addingComment = true;
    this.ticketService.addAdminComment(this.ticket.id, this.newComment.trim(), 'Admin', true).subscribe({
      next: () => {
        // Don't add optimistically - let SignalR broadcast handle it
        this.newComment = '';
        this.addingComment = false;
        this.shouldScrollToBottom = true;
        
        // Reload history
        const id = this.ticket!.id;
        this.ticketService.getTicketById(id).subscribe({
          next: (response: any) => {
            const data = response?.data?.data || response?.data || response;
            this.history = data.history?.$values || data.history || [];
          }
        });
      },
      error: () => {
        this.addingComment = false;
      }
    });
  }

  private async initSignalR(ticketId: string): Promise<void> {
    if (!this.isBrowser) {
      console.log('SignalR: Not in browser environment');
      return;
    }

    // Check both localStorage and sessionStorage (token can be in either depending on "Remember Me" setting)
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    
    if (!token) {
      console.warn('SignalR: No token found. User may not be authenticated.');
      this.error = 'Oturum süresi dolmuş, lütfen sayfayı yenileyin';
      return;
    }

    try {
      console.log('SignalR: Attempting to start connection with token');
      await this.signalRService.startConnection(token);
      
      console.log('SignalR: Joining ticket group', ticketId);
      await this.signalRService.joinTicketGroup(ticketId);
      this.isSignalRConnected = true;
      console.log('SignalR: Successfully connected and joined group');

      this.commentSubscription = this.signalRService.commentReceived$.subscribe(
        (comment: CommentMessage) => {
          console.log('Ticket-View: Received comment from SignalR:', comment);
          if (comment.ticketId === ticketId) {
            const exists = this.comments.some(c => c.id === comment.id);
            if (!exists) {
              console.log('Ticket-View: Adding new comment to list');
              // Immutability pattern - create new array reference to trigger change detection
              this.comments = [...this.comments, comment];
              this.shouldScrollToBottom = true;
              // Only reload history, don't reload ticket which resets scroll
              this.ticketService.getTicketById(ticketId).subscribe({
                next: (response: any) => {
                  const data = response?.data?.data || response?.data || response;
                  this.history = data.history?.$values || data.history || [];
                }
              });
            } else {
              console.log('Ticket-View: Comment already exists, skipping');
            }
          }
        }
      );
    } catch (error) {
      console.error('SignalR: Connection failed:', error);
      this.isSignalRConnected = false;
      console.warn('SignalR: Real-time updates disabled. Comments will appear after refresh.');
    }
  }
}
