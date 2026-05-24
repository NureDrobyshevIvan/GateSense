import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AuthService } from '../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, TranslateModule, LanguageSwitcherComponent],
  template: `
    <nav class="navbar navbar-expand navbar-dark bg-dark px-3">
      <a class="navbar-brand" routerLink="/app/garages">{{ 'app.title' | translate }}</a>
      <ul class="navbar-nav me-auto">
        <li class="nav-item">
          <a class="nav-link" routerLink="/app/garages" routerLinkActive="active">
            {{ 'nav.myGarages' | translate }}
          </a>
        </li>
        @if (auth.isAdmin()) {
          <li class="nav-item">
            <a class="nav-link text-warning" routerLink="/admin/users" routerLinkActive="active">
              {{ 'nav.admin' | translate }}
            </a>
          </li>
        }
      </ul>
      <div class="d-flex align-items-center gap-3">
        <app-language-switcher></app-language-switcher>
        <span class="text-light small">{{ auth.userName() }}</span>
        <button class="btn btn-sm btn-outline-light" (click)="logout()">{{ 'app.logout' | translate }}</button>
      </div>
    </nav>

    <main class="container py-4">
      <router-outlet></router-outlet>
    </main>
  `
})
export class UserLayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
