import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AuthService } from '../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, TranslateModule, LanguageSwitcherComponent],
  template: `
    <nav class="navbar navbar-expand navbar-dark bg-warning px-3">
      <a class="navbar-brand text-dark" routerLink="/admin/users">{{ 'app.title' | translate }} · {{ 'nav.admin' | translate }}</a>
      <ul class="navbar-nav me-auto">
        <li class="nav-item">
          <a class="nav-link text-dark" routerLink="/admin/users" routerLinkActive="fw-bold">{{ 'nav.users' | translate }}</a>
        </li>
        <li class="nav-item">
          <a class="nav-link text-dark" routerLink="/admin/garages" routerLinkActive="fw-bold">{{ 'nav.allGarages' | translate }}</a>
        </li>
        <li class="nav-item">
          <a class="nav-link text-dark" routerLink="/admin/backup" routerLinkActive="fw-bold">{{ 'nav.backup' | translate }}</a>
        </li>
      </ul>
      <div class="d-flex align-items-center gap-3">
        <app-language-switcher></app-language-switcher>
        <a class="btn btn-sm btn-dark" routerLink="/app/garages">User view</a>
        <span class="text-dark small">{{ auth.userName() }}</span>
        <button class="btn btn-sm btn-outline-dark" (click)="logout()">{{ 'app.logout' | translate }}</button>
      </div>
    </nav>

    <main class="container-fluid py-4">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AdminLayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
