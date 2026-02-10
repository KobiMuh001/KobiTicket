import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser, Location } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StaffService } from '../../../core/services/staff.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('commentsContainer') private commentsContainer!: ElementRef;
  
  ticketId: string = '';
  ticket: any = null;
  comments: any[] = [];
  history: any[] = [];
  isLoading = true;
  error: string | null = null;
  successMessage: string | null = null;
  
  // Staff info
  staffProfile: any = null;
  isMyTicket = false;
  
  // Form fields
  newComment: string = '';
  solutionNote: string = '';
  showResolveModal = false;
  
  activeTab: 'comments' | 'history' = 'comments';

  // SignalR
  private commentSubscription?: Subscription;
  isSignalRConnected = false;
  private shouldScrollToBottom = false;
  private isBrowser: boolean;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private staffService: StaffService,
    private signalRService: SignalRService,
    public location: Location,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';
    this.loadStaffProfile();
    this.loadTicket();
    this.loadComments();
    this.loadHistory();
    if (this.isBrowser) {
      this.initSignalR();
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
      console.log('SignalR: Staff - Attempting to start connection');
      await this.signalRService.startConnection(token);
      
      console.log('SignalR: Staff - Joining ticket group', this.ticketId);
      await this.signalRService.joinTicketGroup(this.ticketId);
      this.isSignalRConnected = true;
      console.log('SignalR: Staff - Successfully connected and joined group');

      this.commentSubscription = this.signalRService.commentReceived$.subscribe(
        (comment: CommentMessage) => {
          console.log('Staff-Detail: Received comment from SignalR:', comment);
          // Aynı ticket'a ait yorum mu kontrol et
          if (comment.ticketId === this.ticketId) {
            // Zaten listede var mı kontrol et
            const exists = this.comments.some(c => c.id === comment.id);
            if (!exists) {
              console.log('Staff-Detail: Adding new comment to list');
              this.comments.push(comment);
              this.shouldScrollToBottom = true;
              // Only reload history when new comment from other user arrives, don't reload ticket which resets scroll
              this.loadHistory();
            } else {
              console.log('Staff-Detail: Comment already exists, skipping');
            }
          }
        }
      );
    } catch (error) {
      console.error('SignalR: Connection failed:', error);
      this.isSignalRConnected = false;
      console.warn('SignalR: Real-time updates disabled for staff');
    }
  }

  loadStaffProfile(): void {
    this.staffService.getMyProfile().subscribe({
      next: (res) => {
        if (res.success) {
          this.staffProfile = res.data;
          this.checkOwnership();
        }
      }
    });
  }

  checkOwnership(): void {
    if (this.ticket && this.staffProfile) {
      this.isMyTicket = this.ticket.assignedPerson === this.staffProfile.fullName;
    }
  }

  loadTicket(): void {
    this.staffService.getTicketDetail(this.ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.ticket = res.data;
          this.checkOwnership();
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Ticket yüklenirken hata oluştu';
        this.isLoading = false;
      }
    });
  }

  claimTicket(): void {
    this.staffService.claimTicket(this.ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Ticket üstlenildi!';
          this.loadTicket();
          this.loadStaffProfile();
          setTimeout(() => this.successMessage = null, 3000);
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Ticket üstlenilirken hata oluştu';
      }
    });
  }

  loadComments(): void {
    this.staffService.getTicketComments(this.ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.comments = res.data;
          this.shouldScrollToBottom = true;
        }
      }
    });
  }

  loadHistory(): void {
    this.staffService.getTicketHistory(this.ticketId).subscribe({
      next: (res) => {
        if (res.success) {
          this.history = res.data;
        }
      }
    });
  }

  addComment(): void {
    if (!this.newComment.trim()) return;
    
    this.staffService.addComment(this.ticketId, this.newComment).subscribe({
      next: (res) => {
        if (res.success) {
          this.newComment = '';
          // Only reload history to avoid scroll reset
          this.loadHistory();
          this.successMessage = 'Yorum eklendi';
          setTimeout(() => this.successMessage = null, 3000);
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Yorum eklenirken hata oluştu';
      }
    });
  }

  updateStatus(newStatus: number): void {
    this.staffService.updateTicketStatus(this.ticketId, newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadTicket();
          this.loadHistory();
          this.successMessage = 'Durum güncellendi';
          setTimeout(() => this.successMessage = null, 3000);
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Durum güncellenirken hata oluştu';
      }
    });
  }

  resolveTicket(): void {
    if (!this.solutionNote.trim()) {
      this.error = 'Çözüm notu gerekli';
      return;
    }
    
    this.staffService.resolveTicket(this.ticketId, this.solutionNote).subscribe({
      next: (res) => {
        if (res.success) {
          this.showResolveModal = false;
          this.solutionNote = '';
          this.loadTicket();
          this.loadHistory();
          this.successMessage = 'Ticket başarıyla çözüldü!';
          setTimeout(() => this.successMessage = null, 3000);
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Ticket çözülürken hata oluştu';
      }
    });
  }

  releaseTicket(): void {
    if (confirm('Bu ticketı bırakmak istediğinize emin misiniz?')) {
      this.staffService.releaseTicket(this.ticketId).subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigate(['/staff/my-tickets']);
          }
        },
        error: (err) => {
          this.error = err.error?.message || 'Ticket bırakılırken hata oluştu';
        }
      });
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 1: return 'Açık';
      case 2: return 'İşlemde';
      case 3: return 'Müşteri Bekliyor';
      case 4: return 'Çözüldü';
      case 5: return 'Kapalı';
      default: return 'Bilinmiyor';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-open';
      case 2: return 'status-processing';
      case 3: return 'status-waiting';
      case 4: return 'status-resolved';
      case 5: return 'status-closed';
      default: return '';
    }
  }

  getPriorityText(priority: number): string {
    switch (priority) {
      case 1: return 'Düşük';
      case 2: return 'Normal';
      case 3: return 'Yüksek';
      case 4: return 'Kritik';
      default: return 'Normal';
    }
  }

  getPriorityClass(priority: number): string {
    switch (priority) {
      case 1: return 'priority-low';
      case 2: return 'priority-normal';
      case 3: return 'priority-high';
      case 4: return 'priority-critical';
      default: return 'priority-normal';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
