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
        this.comments = data.comments?.$values || data.comments || [];
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
        this.newComment = '';
        this.addingComment = false;
        // Only reload history, don't reload ticket which resets scroll
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
              this.comments.push(comment);
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
