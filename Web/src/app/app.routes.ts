import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./features/user/user-layout.component').then(m => m.UserLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'garages' },
      {
        path: 'garages',
        loadComponent: () => import('./features/user/garages/garages-list.component').then(m => m.GaragesListComponent)
      },
      {
        path: 'garages/:garageId',
        loadComponent: () => import('./features/user/garages/garage-detail.component').then(m => m.GarageDetailComponent)
      },
      {
        path: 'garages/:garageId/events',
        loadComponent: () => import('./features/user/events/event-log.component').then(m => m.EventLogComponent)
      },
      {
        path: 'garages/:garageId/access',
        loadComponent: () => import('./features/user/access/access-management.component').then(m => m.AccessManagementComponent)
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/admin-layout.component').then(m => m.AdminLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'users' },
      {
        path: 'users',
        loadComponent: () => import('./features/admin/users/users-list.component').then(m => m.UsersListComponent)
      },
      {
        path: 'garages',
        loadComponent: () => import('./features/admin/garages/admin-garages.component').then(m => m.AdminGaragesComponent)
      },
      {
        path: 'backup',
        loadComponent: () => import('./features/admin/backup/backup.component').then(m => m.BackupComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
