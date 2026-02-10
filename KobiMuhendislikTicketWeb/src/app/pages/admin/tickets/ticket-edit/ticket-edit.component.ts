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
  createdDate: string;
}

interface TicketHistoryItem {
  description: string;
  actionBy: string;
  createdDate: string;
}

interface TicketDetail {
  id: string;
  title: string;
  description: string;
  status: string | number;
  priority: string | number;
  assignedPerson?: string;
  imagePath?: string;
  createdDate: string;
  updatedDate?: string;
  tenantId: string;
  tenantName: string;
  tenantEmail?: string;
  tenantPhone?: string;
  assetId?: string;
  assetName?: string;
  assetSerialNumber?: string;
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
    }
    this.loadStaff();
  }

  ngOnDestroy(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.signalRService.leaveTicketGroup(id);
    }
    this.commentSubscription?.unsubscribe();
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

  loadTicket(id: string): void {
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
        const newComments = data.comments?.$values || data.comments || [];
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
    
    const selectedStaff = this.staffList.find(s => s.id === this.selectedStaffId);
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
        this.newComment = '';
        this.addingComment = false;
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
          if (comment.ticketId === ticketId) {
            const exists = this.comments.some(c => c.id === comment.id);
            if (!exists) {
              console.log('Ticket-Edit: Adding new comment to list');
              this.comments.push(comment);
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
    return new Date(date).toLocaleString('tr-TR');
  }

  goBack(): void {
    this.router.navigate(['/admin/tickets']);
  }
}
