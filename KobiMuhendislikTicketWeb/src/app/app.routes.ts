import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard, customerGuard, staffGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
    canActivate: [guestGuard]
  },
  {
    path: 'staff-login',
    loadComponent: () => import('./pages/staff-login/staff-login.component').then(m => m.StaffLoginComponent),
    canActivate: [guestGuard]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./pages/unauthorized.component').then(m => m.UnauthorizedComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./layouts/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./pages/admin/tickets/tickets.component').then(m => m.TicketsComponent)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('./pages/admin/tickets/ticket-view/ticket-view.component').then(m => m.TicketViewComponent)
      },
      {
        path: 'tickets/:id/edit',
        loadComponent: () => import('./pages/admin/tickets/ticket-edit/ticket-edit.component').then(m => m.TicketEditComponent)
      },
      {
        path: 'tenants',
        loadComponent: () => import('./pages/admin/tenants/tenants.component').then(m => m.TenantsComponent)
      },
      {
        path: 'tenants/:id/products',
        loadComponent: () => import('./pages/admin/tenants/tenant-products/tenant-products.component').then(m => m.TenantProductsComponent)
      },
      {
        path: 'tenants/:id',
        loadComponent: () => import('./pages/admin/tenants/tenant-edit/tenant-edit.component').then(m => m.TenantEditComponent)
      },
      {
        path: 'assets',
        loadComponent: () => import('./pages/admin/assets/assets.component').then(m => m.AssetsComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./pages/admin/products/products.component').then(m => m.ProductsComponent)
      },
      {
        path: 'products/:id',
        loadComponent: () => import('./pages/admin/products/product-tenants.component').then(m => m.ProductTenantsComponent)
      },
      {
        path: 'assets/:id',
        loadComponent: () => import('./pages/admin/assets/asset-edit.component').then(m => m.AssetEditComponent)
      },
      {
        path: 'staff',
        loadComponent: () => import('./pages/admin/staff/staff.component').then(m => m.StaffComponent)
      },
      {
        path: 'staff/:id',
        loadComponent: () => import('./pages/admin/staff/staff-edit.component').then(m => m.StaffEditComponent)
      }
    ]
  },
  {
    path: 'customer',
    loadComponent: () => import('./layouts/customer-layout/customer-layout.component').then(m => m.CustomerLayoutComponent),
    canActivate: [customerGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/customer/customer-dashboard/customer-dashboard.component').then(m => m.CustomerDashboardComponent)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./pages/customer/customer-tickets/customer-tickets.component').then(m => m.CustomerTicketsComponent)
      },
      {
        path: 'tickets/new',
        loadComponent: () => import('./pages/customer/customer-ticket-create/customer-ticket-create.component').then(m => m.CustomerTicketCreateComponent)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('./pages/customer/customer-ticket-detail/customer-ticket-detail.component').then(m => m.CustomerTicketDetailComponent)
      },
      {
        path: 'assets',
        loadComponent: () => import('./pages/customer/customer-assets/customer-assets.component').then(m => m.CustomerAssetsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/customer/customer-profile/customer-profile.component').then(m => m.CustomerProfileComponent)
      }
    ]
  },
  {
    path: 'staff',
    loadComponent: () => import('./layouts/staff-layout/staff-layout.component').then(m => m.StaffLayoutComponent),
    canActivate: [staffGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/staff/dashboard/staff-dashboard.component').then(m => m.StaffDashboardComponent)
      },
      {
        path: 'my-tickets',
        loadComponent: () => import('./pages/staff/my-tickets/my-tickets.component').then(m => m.MyTicketsComponent)
      },
      {
        path: 'open-tickets',
        loadComponent: () => import('./pages/staff/open-tickets/open-tickets.component').then(m => m.OpenTicketsComponent)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('./pages/staff/ticket-detail/ticket-detail.component').then(m => m.TicketDetailComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/staff/profile/profile.component').then(m => m.StaffProfileComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];

