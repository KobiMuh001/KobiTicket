import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser, Location } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StaffService } from '../../../core/services/staff.service';
import { SystemParameterService } from '../../../core/services/system-parameter.service';
import { SignalRService, CommentMessage } from '../../../core/services/signalr.service';
import { Subscription } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('commentsContainer') private commentsContainer!: ElementRef;
  
  baseUrl = environment.baseUrl;
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
  showImagePreview = false;
  selectedImagePath: string | null = null;
  // Status change confirmation
  showConfirmStatusModal = false;
  pendingStatus: number | null = null;
  pendingStatusLabel: string | null = null;
  // Release confirmation
  showReleaseModal = false;
  releasing = false;
  
  activeTab: 'comments' | 'history' = 'comments';

  // SignalR
  private commentSubscription?: Subscription;
  isSignalRConnected = false;
  public shouldScrollToBottom = false;
  private isBrowser: boolean;
  private refreshInterval: any;
  statusOptions: Array<any> = [];
  priorityOptions: Array<any> = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private staffService: StaffService,
    private paramSvc: SystemParameterService,
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
    this.loadLookups();
    if (this.isBrowser) {
      this.initSignalR();
    }
    
    // Commentsları 2 saniyede bir otomatik yenile (polling)
    this.refreshInterval = setInterval(() => {
      console.log('Staff Ticket-Detail: Periodic comments refresh triggered');
      this.loadComments();
    }, 2000);
  }

  loadLookups(): void {
    this.paramSvc.getByGroup('TicketStatus').subscribe({
      next: (res: any) => {
        const sData = res?.data?.data || res?.data || res || [];
        this.statusOptions = (Array.isArray(sData) ? sData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.statusOptions = []; }
    });

    this.paramSvc.getByGroup('TicketPriority').subscribe({
      next: (res: any) => {
        const pData = res?.data?.data || res?.data || res || [];
        this.priorityOptions = (Array.isArray(pData) ? pData : []).map((p: any, i: number) => ({ id: p.id, key: p.key, label: p.value, sortOrder: p.sortOrder ?? i + 1, color: p.value2 ?? p.color ?? null }));
      },
      error: () => { this.priorityOptions = []; }
    });
  }

  getStatusColor(status: number): string | null {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    return found?.color ?? null;
  }

  getPriorityColor(priority: number): string | null {
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    return found?.color ?? null;
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
    setTimeout(() => {
      try {
        if (this.commentsContainer) {
          const element = this.commentsContainer.nativeElement;
          element.scrollTop = element.scrollHeight;
        }
      } catch (err) {}
    }, 100);
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

  openImagePreview(imagePath: string): void {
    this.selectedImagePath = imagePath;
    this.showImagePreview = true;
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
      const assigned = (this.ticket.assignedPerson || '').toString().trim().toLocaleLowerCase('tr-TR');
      const me = (this.staffProfile.fullName || '').toString().trim().toLocaleLowerCase('tr-TR');
      this.isMyTicket = assigned.length > 0 && assigned === me;
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
          // Process comments to mark staff messages
          let comments = res.data || [];
          comments = comments.map((c: any) => {
            // If isAdminReply is true but authorName is not "Admin", it's a staff message
            if (c.isAdminReply && c.authorName !== 'Admin') {
              return { ...c, isStaff: true };
            }
            return c;
          });
          this.comments = comments;
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
    if (!this.isMyTicket) {
      this.error = 'Ticketı üstlenmeden mesaj gönderemezsiniz';
      return;
    }
    if (!this.newComment.trim()) return;
    
    this.staffService.addComment(this.ticketId, this.newComment).subscribe({
      next: (res) => {
        if (res.success) {
          this.newComment = '';
          // Yorumları hemen yükle
          this.loadComments();
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

  // Safe handler for status dropdown changes
  onStatusSelect(event: Event): void {
    const target = event.target as HTMLSelectElement | null;
    const val = target?.value;
    if (val === undefined || val === null) return;
    // Find the selected option object from loaded lookups
    const selected = this.statusOptions.find((s: any) =>
      String(s.sortOrder ?? s.id) === String(val) || String(s.id) === String(val) || String(s.key) === String(val)
    );
    if (!selected) {
      this.error = 'Geçersiz durum değeri';
      // revert UI
      if (target) target.value = (this.ticket?.status ?? '') + '';
      console.debug('Status select - selected not found. statusOptions=', this.statusOptions, 'value=', val);
      return;
    }

    // Determine numeric status to send to API. Prefer numeric `key`, then `sortOrder`, then `id`.
    let sendValue: number | null = null;
    if (selected.key !== undefined && selected.key !== null && !isNaN(Number(selected.key))) {
      sendValue = Number(selected.key);
    } else if (selected.sortOrder !== undefined && selected.sortOrder !== null && !isNaN(Number(selected.sortOrder))) {
      sendValue = Number(selected.sortOrder);
    } else if (selected.id !== undefined && selected.id !== null && !isNaN(Number(selected.id))) {
      sendValue = Number(selected.id);
    }

    if (sendValue === null) {
      this.error = 'Geçersiz durum değeri';
      if (target) target.value = (this.ticket?.status ?? '') + '';
      console.debug('Status select - resolved selected but no numeric sendValue. selected=', selected);
      return;
    }

    // Prepare confirmation modal with resolved numeric value
    this.pendingStatus = sendValue;
    this.pendingStatusLabel = selected.label || String(sendValue);
    this.showConfirmStatusModal = true;
    // Revert select UI to actual ticket.status so the displayed status stays accurate until confirmed
    try { if (target) target.value = (this.ticket?.status ?? '') + ''; } catch (e) {}
  }

  // Log before sending to server and after response to help debug 400
  changeTicketStatus(ticketId: number, newStatus: number): void {
    console.debug('Attempting status change', { ticketId, newStatus });
    this.staffService.updateTicketStatus(String(ticketId), newStatus).subscribe({
      next: (res: any) => {
        console.debug('Status change response', res);
        if (res && res.success) {
          const t = this.ticket; // if on detail view
          if (t && t.id === ticketId) {
            t.status = Number(newStatus);
            this.loadTicket();
          }
          this.successMessage = 'Ticket durumu başarıyla güncellendi';
          setTimeout(() => this.successMessage = null, 3000);
        } else if (res && res.message) {
          this.error = res.message;
        }
      },
      error: (err) => {
        console.debug('Status change error', err);
        this.error = err.error?.message || 'Ticket durumu güncellenirken hata oluştu';
      }
    });
  }

  confirmStatusChange(): void {
    if (this.pendingStatus === null) return;
    const ns = this.pendingStatus;
    this.showConfirmStatusModal = false;
    this.pendingStatus = null;
    this.pendingStatusLabel = null;
    this.updateStatus(ns);
  }

  cancelStatusChange(): void {
    this.showConfirmStatusModal = false;
    this.pendingStatus = null;
    this.pendingStatusLabel = null;
    // ticket remains unchanged; view will update to current ticket.status
  }

  releaseTicket(): void {
    // show centered confirmation modal instead of native confirm
    this.showReleaseModal = true;
  }

  confirmRelease(): void {
    this.releasing = true;
    this.staffService.releaseTicket(this.ticketId).subscribe({
      next: (res) => {
        this.releasing = false;
        this.showReleaseModal = false;
        if (res.success) {
          this.router.navigate(['/staff/my-tickets']);
        } else {
          this.error = res.message || 'Ticket bırakılırken hata oluştu';
        }
      },
      error: (err) => {
        this.releasing = false;
        this.showReleaseModal = false;
        this.error = err.error?.message || 'Ticket bırakılırken hata oluştu';
      }
    });
  }

  cancelRelease(): void {
    this.showReleaseModal = false;
  }

  getStatusText(status: number): string {
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) return found.label || 'Bilinmiyor';

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
    const n = Number(status);
    const found = this.statusOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(status) || String(o.key) === String(status) || o.label === status);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'status-open';
        case 2: return 'status-processing';
        case 3: return 'status-waiting';
        case 4: return 'status-resolved';
        case 5: return 'status-closed';
        default: return '';
      }
    }
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
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) return found.label || 'Normal';

    switch (priority) {
      case 1: return 'Düşük';
      case 2: return 'Normal';
      case 3: return 'Yüksek';
      case 4: return 'Kritik';
      default: return 'Normal';
    }
  }

  getPriorityClass(priority: number): string {
    const n = Number(priority);
    const found = this.priorityOptions.find((o: any) => Number(o.sortOrder ?? o.id) === n || String(o.id) === String(priority) || String(o.key) === String(priority) || o.label === priority);
    if (found) {
      const num = Number(found.sortOrder ?? found.id);
      switch (num) {
        case 1: return 'priority-low';
        case 2: return 'priority-normal';
        case 3: return 'priority-high';
        case 4: return 'priority-critical';
        default: return 'priority-normal';
      }
    }
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

  formatTime(dateString: string): string {
    return new Date(dateString).toLocaleTimeString('tr-TR', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isMyComment(comment: any): boolean {
    if (!this.staffProfile || !comment) return false;
    return comment.authorName === this.staffProfile.fullName;
  }
}
