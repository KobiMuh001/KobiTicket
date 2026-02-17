import { Injectable, Inject, PLATFORM_ID, NgZone } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CommentMessage {
  id: string;
  ticketId: string;
  message: string;
  authorName: string;
  isAdminReply: boolean;
  createdDate: string;
}

export interface TicketUpdateMessage {
  id: string;
  title: string;
  status: number;
  priority: number;
  companyName: string;
  createdDate: string;
  assignedToName?: string;
}

export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  processingTickets: number;
  resolvedTickets: number;
  totalTenants: number;
  totalAssets: number;
  criticalTicketCount: number;
  topFailingAssets: Array<{productName: string, ticketCount: number}>;
}

export interface StaffNotification {
  id: string;
  staffId: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  ticketId?: string;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private dashboardHubConnection: signalR.HubConnection | null = null;
  private notificationHubConnection: signalR.HubConnection | null = null;
  private joinedTicketGroups = new Set<string>();
  private commentReceived = new Subject<CommentMessage>();
  private ticketUpdated = new Subject<TicketUpdateMessage>();
  private dashboardStatsUpdated = new Subject<DashboardStats>();
  private staffNotificationReceived = new Subject<StaffNotification>();
  private connectionState = new Subject<string>();

  public commentReceived$: Observable<CommentMessage> = this.commentReceived.asObservable();
  public ticketUpdated$: Observable<TicketUpdateMessage> = this.ticketUpdated.asObservable();
  public dashboardStatsUpdated$: Observable<DashboardStats> = this.dashboardStatsUpdated.asObservable();
  public staffNotificationReceived$: Observable<StaffNotification> = this.staffNotificationReceived.asObservable();
  public connectionState$: Observable<string> = this.connectionState.asObservable();

  private hubUrl = environment.apiUrl.replace('/api', '/hubs/comments');
  private dashboardHubUrl = environment.apiUrl.replace('/api', '/hubs/dashboard-stats');
  private notificationHubUrl = environment.apiUrl.replace('/api', '/hubs/notifications');
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object, private ngZone: NgZone) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  public startConnection(token: string): Promise<void> {
    if (!this.isBrowser) {
      console.log('SignalR: Not in browser environment, skipping connection');
      return Promise.resolve();
    }

    if (!token) {
      console.error('SignalR: Token is required but was not provided');
      return Promise.reject('No authentication token');
    }

    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR: Hub connection already connected');
      return Promise.resolve();
    }

    if (this.hubConnection?.state === signalR.HubConnectionState.Connecting || this.hubConnection?.state === signalR.HubConnectionState.Reconnecting) {
      console.log('SignalR: Hub connection is connecting/reconnecting');
      return this.waitForConnectedState();
    }

    if (!this.hubConnection) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.registerHandlers();
    }

    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR: Connected successfully to ' + this.hubUrl);
        this.connectionState.next('Connected');
      })
      .catch(err => {
        console.error('SignalR: Connection failed to ' + this.hubUrl, err);
        console.error('SignalR: Error details:', {
          message: err?.message,
          statusCode: err?.statusCode,
          source: err?.source
        });
        this.connectionState.next('Error');
        this.hubConnection = null;
        throw err;
      });
  }

  private waitForConnectedState(timeoutMs: number = 10000): Promise<void> {
    return new Promise((resolve, reject) => {
      const startedAt = Date.now();
      const timer = setInterval(() => {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
          clearInterval(timer);
          resolve();
          return;
        }

        if (Date.now() - startedAt > timeoutMs) {
          clearInterval(timer);
          reject(new Error('SignalR connection timeout while waiting for connected state'));
        }
      }, 150);
    });
  }

  public startDashboardConnection(token: string): Promise<void> {
    if (!this.isBrowser) {
      console.log('SignalR: Dashboard - Not in browser environment');
      return Promise.resolve();
    }

    if (!token) {
      console.error('SignalR: Dashboard - Token is required');
      return Promise.reject('No authentication token');
    }

    if (this.dashboardHubConnection) {
      console.log('SignalR: Dashboard hub connection already exists');
      return Promise.resolve();
    }

    this.dashboardHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.dashboardHubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerDashboardHandlers();

    return this.dashboardHubConnection.start()
      .then(() => {
        console.log('SignalR: Dashboard connected successfully to ' + this.dashboardHubUrl);
      })
      .catch(err => {
        console.error('SignalR: Dashboard connection failed', err);
        throw err;
      });
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    // Önceki handler'ları temizle (duplicate önlemek için)
    this.hubConnection.off('ReceiveComment');
    this.hubConnection.off('TicketUpdated');

    this.hubConnection.on('ReceiveComment', (comment: CommentMessage) => {
      console.log('New comment received:', comment);
      this.ngZone.run(() => this.commentReceived.next(comment));
    });

    this.hubConnection.on('TicketUpdated', (ticket: TicketUpdateMessage) => {
      console.log('Ticket updated:', ticket);
      this.ngZone.run(() => this.ticketUpdated.next(ticket));
    });

    this.hubConnection.onreconnecting(() => {
      console.log('SignalR Reconnecting...');
      this.connectionState.next('Reconnecting');
    });

    this.hubConnection.onreconnected(async () => {
      console.log('SignalR Reconnected');
      this.connectionState.next('Connected');

      for (const ticketId of this.joinedTicketGroups) {
        try {
          await this.hubConnection?.invoke('JoinTicketGroup', ticketId);
          console.log('SignalR: Re-joined ticket group:', ticketId);
        } catch (error) {
          console.error('SignalR: Failed to re-join group', ticketId, error);
        }
      }
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR Disconnected');
      this.connectionState.next('Disconnected');
    });
  }

  private registerDashboardHandlers(): void {
    if (!this.dashboardHubConnection) return;

    // Önceki handler'ları temizle (duplicate önlemek için)
    this.dashboardHubConnection.off('DashboardStatsUpdated');

    this.dashboardHubConnection.on('DashboardStatsUpdated', (stats: DashboardStats) => {
      console.log('Dashboard stats updated:', stats);
      this.ngZone.run(() => this.dashboardStatsUpdated.next(stats));
    });

    this.dashboardHubConnection.onreconnecting(() => {
      console.log('SignalR Dashboard Reconnecting...');
    });

    this.dashboardHubConnection.onreconnected(() => {
      console.log('SignalR Dashboard Reconnected');
    });

    this.dashboardHubConnection.onclose(() => {
      console.log('SignalR Dashboard Disconnected');
    });
  }

  public startNotificationConnection(token: string): Promise<void> {
    if (!this.isBrowser) {
      console.log('SignalR: Notification - Not in browser environment');
      return Promise.resolve();
    }

    if (!token) {
      console.error('SignalR: Notification - Token is required');
      return Promise.reject('No authentication token');
    }

    if (this.notificationHubConnection) {
      console.log('SignalR: Notification hub connection already exists');
      return Promise.resolve();
    }

    this.notificationHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.notificationHubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerNotificationHandlers();

    return this.notificationHubConnection.start()
      .then(() => {
        console.log('SignalR: Notification connected successfully to ' + this.notificationHubUrl);
      })
      .catch(err => {
        console.error('SignalR: Notification connection failed', err);
        throw err;
      });
  }

  private registerNotificationHandlers(): void {
    if (!this.notificationHubConnection) return;

    // Önceki handler'ları temizle (duplicate önlemek için)
    this.notificationHubConnection.off('StaffNotificationReceived');

    this.notificationHubConnection.on('StaffNotificationReceived', (notification: StaffNotification) => {
      console.log('Staff notification received:', notification);
      this.ngZone.run(() => this.staffNotificationReceived.next(notification));
    });

    this.notificationHubConnection.onreconnecting(() => {
      console.log('SignalR Notification Reconnecting...');
    });

    this.notificationHubConnection.onreconnected(() => {
      console.log('SignalR Notification Reconnected');
    });

    this.notificationHubConnection.onclose(() => {
      console.log('SignalR Notification Disconnected');
    });
  }

  public async joinTicketGroup(ticketId: string): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      console.warn('SignalR: Cannot join ticket group, connection is not ready');
      return;
    }

    await this.hubConnection.invoke('JoinTicketGroup', ticketId);
    this.joinedTicketGroups.add(ticketId);
    console.log('Joined ticket group:', ticketId);
  }

  public async leaveTicketGroup(ticketId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveTicketGroup', ticketId);
      console.log('Left ticket group:', ticketId);
    }
    this.joinedTicketGroups.delete(ticketId);
  }

  public stopConnection(): Promise<void> {
    if (this.hubConnection) {
      return this.hubConnection.stop().then(() => {
        this.hubConnection = null;
        this.joinedTicketGroups.clear();
        console.log('SignalR Disconnected');
      });
    }
    return Promise.resolve();
  }

  public stopDashboardConnection(): Promise<void> {
    if (this.dashboardHubConnection) {
      return this.dashboardHubConnection.stop().then(() => {
        this.dashboardHubConnection = null;
        console.log('SignalR Dashboard Disconnected');
      });
    }
    return Promise.resolve();
  }

  public stopNotificationConnection(): Promise<void> {
    if (this.notificationHubConnection) {
      return this.notificationHubConnection.stop().then(() => {
        this.notificationHubConnection = null;
        console.log('SignalR Notification Disconnected');
      });
    }
    return Promise.resolve();
  }

  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  public isDashboardConnected(): boolean {
    return this.dashboardHubConnection?.state === signalR.HubConnectionState.Connected;
  }

  public isNotificationConnected(): boolean {
    return this.notificationHubConnection?.state === signalR.HubConnectionState.Connected;
  }

  public getNotificationHub(): signalR.HubConnection | null {
    return this.notificationHubConnection;
  }
}
