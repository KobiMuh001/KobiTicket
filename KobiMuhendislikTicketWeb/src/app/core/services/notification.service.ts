import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, interval } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  ticketId?: string;
  createdDate: string;
}

export interface NotificationResponse {
  success: boolean;
  data: Notification[];
  unreadCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = environment.apiUrl;
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  public notificationsList$ = this.notificationsSubject.asObservable();
  
  private pollingInterval: any = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // Staff bildirimleri başlat
  initializeStaffNotifications(): void {
    // Polling zaten başlatılmışsa başlatma
    if (this.pollingInterval) {
      console.log('Staff notifications polling already started');
      return;
    }

    console.log('Initializing staff notifications...');
    // İlk yükle
    this.getStaffNotifications(20).subscribe({
      next: () => console.log('Staff notifications loaded'),
      error: (err) => console.error('Staff notifications load error:', err)
    });
    
    // Her 30 saniyede bir güncelle
    this.pollingInterval = setInterval(() => {
      console.log('Staff notifications polling...');
      this.getStaffNotifications(20).subscribe({
        error: (err) => console.error('Staff notifications polling error:', err)
      });
    }, 30000);
  }

  // Admin bildirimleri başlat
  initializeAdminNotifications(): void {
    // Polling zaten başlatılmışsa başlatma
    if (this.pollingInterval) {
      console.log('Admin notifications polling already started');
      return;
    }

    console.log('Initializing admin notifications...');
    // İlk yükle
    this.getNotifications(20).subscribe({
      next: () => console.log('Admin notifications loaded'),
      error: (err) => console.error('Admin notifications load error:', err)
    });
    
    // Her 30 saniyede bir güncelle
    this.pollingInterval = setInterval(() => {
      console.log('Admin notifications polling...');
      this.getNotifications(20).subscribe({
        error: (err) => console.error('Admin notifications polling error:', err)
      });
    }, 30000);
  }

  // Customer bildirimleri başlat
  initializeCustomerNotifications(): void {
    // Polling zaten başlatılmışsa başlatma
    if (this.pollingInterval) {
      console.log('Customer notifications polling already started');
      return;
    }

    console.log('Initializing customer notifications...');
    // İlk yükle
    this.getCustomerNotifications(20).subscribe({
      next: () => console.log('Customer notifications loaded'),
      error: (err) => console.error('Customer notifications load error:', err)
    });

    // Her 30 saniyede bir güncelle
    this.pollingInterval = setInterval(() => {
      console.log('Customer notifications polling...');
      this.getCustomerNotifications(20).subscribe({
        error: (err) => console.error('Customer notifications polling error:', err)
      });
    }, 30000);
  }

  // Polling'i durdur
  stopPolling(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
      console.log('Notifications polling stopped');
    }
  }

  // Bildirimleri yükle
  getNotifications(take: number = 20): Observable<NotificationResponse> {
    return this.http.get<NotificationResponse>(`${this.apiUrl}/admin/notifications?take=${take}`).pipe(
      tap(response => {
        if (response.success) {
          this.notificationsSubject.next(response.data || []);
          this.unreadCountSubject.next(response.unreadCount || 0);
        }
      })
    );
  }

  // Staff bildirimlerini yükle
  getStaffNotifications(take: number = 20): Observable<NotificationResponse> {
    return this.http.get<NotificationResponse>(`${this.apiUrl}/staff/notifications?take=${take}`).pipe(
      tap(response => {
        if (response.success) {
          this.notificationsSubject.next(response.data || []);
          this.unreadCountSubject.next(response.unreadCount || 0);
        }
      })
    );
  }

  // Customer bildirimlerini yükle
  getCustomerNotifications(take: number = 20): Observable<NotificationResponse> {
    return this.http.get<NotificationResponse>(`${this.apiUrl}/tickets/notifications?take=${take}`).pipe(
      tap(response => {
        if (response.success) {
          this.notificationsSubject.next(response.data || []);
          this.unreadCountSubject.next(response.unreadCount || 0);
        }
      })
    );
  }

  // Okunmamış bildirim sayısını getir
  getUnreadCount(): Observable<{ success: boolean; count: number }> {
    return this.http.get<{ success: boolean; count: number }>(`${this.apiUrl}/admin/notifications/unread-count`).pipe(
      tap(response => {
        if (response.success) {
          this.unreadCountSubject.next(response.count);
        }
      })
    );
  }

  // Bildirimi okundu olarak işaretle
  markAsRead(notificationId: string): Observable<any> {
    const user = this.authService.getCurrentUser();
    const endpoint = user?.role === 'Staff'
      ? `${this.apiUrl}/staff/notifications/${notificationId}/read`
      : user?.role === 'Customer'
        ? `${this.apiUrl}/tickets/notifications/${notificationId}/read`
        : `${this.apiUrl}/admin/notifications/${notificationId}/read`;
    
    return this.http.patch(endpoint, {}).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value;
        const updated = notifications.map(n => 
          n.id === notificationId ? { ...n, isRead: true } : n
        );
        this.notificationsSubject.next(updated);
        this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
      })
    );
  }

  // Tüm bildirimleri okundu olarak işaretle
  markAllAsRead(): Observable<any> {
    const user = this.authService.getCurrentUser();
    const endpoint = user?.role === 'Staff'
      ? `${this.apiUrl}/staff/notifications/read-all`
      : user?.role === 'Customer'
        ? `${this.apiUrl}/tickets/notifications/read-all`
        : `${this.apiUrl}/admin/notifications/read-all`;
    return this.http.patch(endpoint, {}).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value;
        const updated = notifications.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(updated);
        this.unreadCountSubject.next(0);
      })
    );
  }

  // Bildirimi sil
  deleteNotification(notificationId: string): Observable<any> {
    const user = this.authService.getCurrentUser();
    const endpoint = user?.role === 'Staff'
      ? `${this.apiUrl}/staff/notifications/${notificationId}`
      : user?.role === 'Customer'
        ? `${this.apiUrl}/tickets/notifications/${notificationId}`
        : `${this.apiUrl}/admin/notifications/${notificationId}`;
    
    return this.http.delete(endpoint).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value;
        const deleted = notifications.find(n => n.id === notificationId);
        const updated = notifications.filter(n => n.id !== notificationId);
        this.notificationsSubject.next(updated);
        if (deleted && !deleted.isRead) {
          this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
        }
      })
    );
  }

  // Bildirim tipine göre ikon class'ı
  getNotificationIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'NewTicket': 'ticket',
      'TicketComment': 'comment',
      'TicketStatusChanged': 'status',
      'TicketAssigned': 'assign',
      'TicketPriorityChanged': 'priority',
      'TicketResolved': 'resolved',
      'General': 'info'
    };
    return iconMap[type] || 'info';
  }

  // Bildirim tipine göre renk class'ı
  getNotificationColor(type: string): string {
    const colorMap: { [key: string]: string } = {
      'NewTicket': 'primary',
      'TicketComment': 'info',
      'TicketStatusChanged': 'warning',
      'TicketAssigned': 'success',
      'TicketPriorityChanged': 'danger',
      'TicketResolved': 'success',
      'General': 'secondary'
    };
    return colorMap[type] || 'secondary';
  }

  // Direktli bildirim ekle (SignalR tarafından)
  addNotificationDirectly(notification: Notification): void {
    const current = this.notificationsSubject.value;
    const updated = [notification, ...current].slice(0, 20); // Son 20 bildirimi tut
    this.notificationsSubject.next(updated);
    if (!notification.isRead) {
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    }
  }

  // Yeni bildirimi ekle ve sayıyı güncelle (Staff için - backend'den gelen format)
  addStaffNotificationDirectly(notification: any): void {
    // Backend'den gelen notification formatını dönüştür
    const formattedNotification: Notification = {
      id: notification.id?.toString() || notification.Id?.toString() || `notif-${Date.now()}`,
      title: notification.title || notification.Title || '',
      message: notification.message || notification.Message || '',
      type: notification.type || notification.Type || '',
      isRead: notification.isRead ?? notification.IsRead ?? false,
      ticketId: notification.ticketId?.toString() || notification.TicketId?.toString(),
      createdDate: notification.createdDate || notification.CreatedDate || new Date().toISOString()
    };
    this.addNotificationDirectly(formattedNotification);
  }
}