import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-layout.component.html',
  styleUrls: ['./customer-layout.component.scss']
})
export class CustomerLayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  currentUser: User | null = null;
  
  menuItems = [
    {
      title: 'Ana Sayfa',
      icon: 'home',
      route: '/customer/dashboard'
    },
    {
      title: 'Ticketlarım',
      icon: 'ticket',
      route: '/customer/tickets'
    },
    {
      title: 'Varlıklarım',
      icon: 'assets',
      route: '/customer/assets'
    },
    {
      title: 'Profilim',
      icon: 'profile',
      route: '/customer/profile'
    }
  ];

  constructor(private authService: AuthService) {}

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
