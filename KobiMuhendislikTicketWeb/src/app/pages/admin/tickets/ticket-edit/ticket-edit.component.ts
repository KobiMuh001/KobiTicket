import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../../core/services/ticket.service';
import { StaffService, Staff } from '../../../../core/services/staff.service';
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
  productId?: number;
  assetName?: string;
  assetUnderWarranty?: boolean;
  comments: TicketComment[];
  history: TicketHistoryItem[];
}

@Component({
  selector: 'app-ticket-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-edit.component.html',
  styleUrls: ['./ticket-edit.component.scss']
})
export class TicketEditComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('commentsContainer') private commentsContainer!: ElementRef;
  private shouldScrollToBottom = false;
  private lastCommentsLength = 0;

  ticket: TicketDetail | null = null;
  comments: TicketComment[] = [];
  history: TicketHistoryItem[] = [];
  loading = true;
  saving = false;
  error = '';

  newComment = '';
  addingComment = false;
  
  selectedStaffId = '';
  staffList: Staff[] = [];
  assigning = false;
  loadingStaff = false;

  activeTab: 'details' | 'comments' | 'history' = 'details';
  private commentSubscription?: Subscription;
  isSignalRConnected = false;
  private isBrowser: boolean;
  baseUrl = environment.baseUrl;
  showImagePreview = false;
  selectedImagePath: string | null = null;
  private refreshInterval: any;

  statusOptions = [
    { value: 1, label: 'Açık', class: 'open' },
    { value: 2, label: 'İşlemde', class: 'processing' },
    { value: 3, label: 'Müşteri Bekleniyor', class: 'waiting' },
    { value: 4, label: 'Çözüldü', class: 'resolved' },
    { value: 5, label: 'Kapatıldı', class: 'closed' }
  ];

  statusValueMap: { [key: number]: number } = {
    1: 1,
    2: 2,
    3: 3,
    4: 4,
    5: 5
  };

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

  priorityOptions = [
    { value: 1, label: 'Düşük', class: 'low' },
    { value: 2, label: 'Orta', class: 'medium' },
    { value: 3, label: 'Yüksek', class: 'high' },
    { value: 4, label: 'Kritik', class: 'critical' }
  ];

  priorityValueMap: { [key: number]: number } = {
    1: 1,
    2: 2,
    3: 3,
    4: 4
  };

  // String -> Number dönüşüm haritaları
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
    private staffService: StaffService,
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

      this.refreshInterval = setInterval(() => {
        this.refreshCommentsAndHistory(id);
      }, 2000);
    }
    this.loadStaff();
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }

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
    if (this.commentsContainer) {
      try {
        setTimeout(() => {
          this.commentsContainer.nativeElement.scrollTop = this.commentsContainer.nativeElement.scrollHeight;
        }, 0);
      } catch (err) {}
    }
  }

  public scrollToCommentsBottom(): void {
    setTimeout(() => this.scrollToBottom(), 0);
  }

  loadStaff(): void {
    this.loadingStaff = true;
    this.staffService.getAllStaff(true).subscribe({
      next: (response: any) => {
        const data = response?.data?.$values || response?.data || response;
        this.staffList = Array.isArray(data) ? data : [];
        this.loadingStaff = false;
      },
      error: () => {
        this.loadingStaff = false;
      }
    });
  }

  loadTicket(id: number | string): void {
    this.loading = true;
    this.ticketService.getTicketById(id).subscribe({
      next: (response: any) => {
        const data = response?.data?.data || response?.data || response;
        
        // Status ve Priority'yi number'a çevir (backend string döndürüyor)
        if (typeof data.status === 'string') {
          data.status = this.statusStringToNumber[data.status] || 1;
        }
        if (typeof data.priority === 'string') {
          data.priority = this.priorityStringToNumber[data.priority] || 1;
        }
        
        this.ticket = data;
        
        // Process comments to mark staff messages
        let newComments = data.comments?.$values || data.comments || [];
        newComments = newComments.map((c: TicketComment) => {
          // If isAdminReply is true but authorName is not "Admin", it's a staff message
          if (c.isAdminReply && c.authorName !== 'Admin') {
            return { ...c, isStaff: true };
          }
          return c;
        });
        
        this.history = data.history?.$values || data.history || [];
        this.loading = false;
        // Sadece yorumlar değiştiyse scroll
        if (this.comments.length !== newComments.length) {
          this.shouldScrollToBottom = true;
        }
        this.comments = newComments;
      },
      error: () => {
        this.error = 'Talep yuklenirken bir hata olustu';
        this.loading = false;
      }
    });
  }

  private refreshCommentsAndHistory(ticketId: string): void {
    this.ticketService.getTicketById(ticketId).subscribe({
      next: (response: any) => {
        const data = response?.data?.data || response?.data || response;

        let freshComments = data.comments?.$values || data.comments || [];
        freshComments = freshComments.map((c: TicketComment) => {
          if (c.isAdminReply && c.authorName !== 'Admin') {
            return { ...c, isStaff: true };
          }
          return c;
        });

        if (freshComments.length !== this.comments.length) {
          this.comments = freshComments;
          this.shouldScrollToBottom = true;
        }

        this.history = data.history?.$values || data.history || [];
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

  updateStatus(statusValue: number): void {
    if (!this.ticket) return;

    this.saving = true;
    this.ticketService.updateTicketStatus(this.ticket.id, statusValue).subscribe({
      next: () => {
        if (this.ticket) {
          this.ticket.status = statusValue;
        }
        this.saving = false;
        this.loadTicket(this.ticket!.id);
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  updatePriority(priorityValue: number): void {
    if (!this.ticket) return;

    this.saving = true;
    this.ticketService.updateTicketPriority(this.ticket.id, priorityValue).subscribe({
      next: () => {
        if (this.ticket) {
          this.ticket.priority = priorityValue;
        }
        this.saving = false;
        this.loadTicket(this.ticket!.id);
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  assignTicket(): void {
    if (!this.ticket || !this.selectedStaffId) return;
    
    const selectedId = typeof this.selectedStaffId === 'number' ? this.selectedStaffId : Number(this.selectedStaffId);
    const selectedStaff = this.staffList.find(s => Number(s.id) === selectedId);
    if (!selectedStaff) return;

    this.assigning = true;
    this.ticketService.assignTicket(this.ticket.id, selectedStaff.fullName).subscribe({
      next: () => {
        if (this.ticket) {
          this.ticket.assignedPerson = selectedStaff.fullName;
        }
        this.assigning = false;
        this.loadTicket(this.ticket!.id);
      },
      error: () => {
        this.assigning = false;
      }
    });
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
        
        // Only reload history to avoid scroll reset
        if (this.ticket) {
          this.ticketService.getTicketById(this.ticket.id).subscribe({
            next: (response: any) => {
              const data = response?.data?.data || response?.data || response;
              this.history = data.history?.$values || data.history || [];
            }
          });
        }
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

    
    let token = localStorage.getItem('token') || sessionStorage.getItem('token');
    
    if (!token) {
      console.warn('SignalR: No token found. User may not be authenticated.');
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
          console.log('Ticket-Edit: Received comment from SignalR:', comment);
          if (String(comment.ticketId) === String(ticketId)) {
            const exists = this.comments.some(c => String(c.id) === String(comment.id));
            if (!exists) {
              console.log('Ticket-Edit: Adding new comment to list');
              // Immutability pattern - create new array reference to trigger change detection
              this.comments = [...this.comments, comment];
              this.shouldScrollToBottom = true;
              // Only reload history when new comment from other user arrives, don't reload ticket which resets scroll
              this.ticketService.getTicketById(ticketId).subscribe({
                next: (response: any) => {
                  const data = response?.data?.data || response?.data || response;
                  this.history = data.history?.$values || data.history || [];
                }
              });
            } else {
              console.log('Ticket-Edit: Comment already exists, skipping');
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
}
