import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../../core/services/ticket.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { Subscription } from 'rxjs';

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
  isLoading = true;
  isSubmittingComment = false;
  newComment = '';
  errorMessage = '';
  
  // SignalR
  private ticketId: string = '';
  private commentSubscription?: Subscription;
  private isSignalRConnected = false;
  private shouldScrollToBottom = false;
  private isBrowser: boolean;

  constructor(
    private ticketService: TicketService,
    private signalRService: SignalRService,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.ticketId = id;
      this.loadTicket(id);
      this.loadComments(id);
      if (this.isBrowser) {
        this.initSignalR();
      }
    }
  }

  ngOnDestroy(): void {
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
            assetName: found.asset?.productName || found.asset?.name || found.assetName
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

  addComment(): void {
    if (!this.newComment.trim() || !this.ticket) return;

    this.isSubmittingComment = true;
    
    this.ticketService.addComment(this.ticket.id, this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.isSubmittingComment = false;
      },
      error: () => {
        this.errorMessage = 'Yorum eklenirken bir hata oluştu.';
        this.isSubmittingComment = false;
      }
    });
  }

  getStatusText(status: string | number): string {
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
}
