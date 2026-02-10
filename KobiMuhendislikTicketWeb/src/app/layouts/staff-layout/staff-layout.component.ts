import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-staff-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './staff-layout.component.html',
  styleUrls: ['./staff-layout.component.scss']
})
export class StaffLayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  currentUser: User | null = null;
  unreadCount$: Observable<number>;
  
  menuItems = [
    {
      title: 'Dashboard',
      icon: 'dashboard',
      route: '/staff/dashboard'
    },
    {
      title: 'Ticketlarım',
      icon: 'my-tickets',
      route: '/staff/my-tickets'
    },
    {
      title: 'Açık Ticketlar',
      icon: 'open-tickets',
      route: '/staff/open-tickets'
    },
    {
      title: 'Profilim',
      icon: 'profile',
      route: '/staff/profile'
    }
  ];

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  logout(): void {
    this.authService.logout();
  }
}
