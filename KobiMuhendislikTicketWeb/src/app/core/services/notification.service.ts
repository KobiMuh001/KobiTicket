import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, interval } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

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

  constructor(private http: HttpClient) {}

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
    return this.http.patch(`${this.apiUrl}/admin/notifications/${notificationId}/read`, {}).pipe(
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
    return this.http.patch(`${this.apiUrl}/admin/notifications/read-all`, {}).pipe(
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
    return this.http.delete(`${this.apiUrl}/admin/notifications/${notificationId}`).pipe(
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
    this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
  }
}