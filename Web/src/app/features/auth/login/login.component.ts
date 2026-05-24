import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AuthService } from '../../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LanguageSwitcherComponent],
  template: `
    <div class="container py-5" style="max-width: 480px;">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h3 mb-0">{{ 'app.title' | translate }}</h1>
        <app-language-switcher></app-language-switcher>
      </div>
      <div class="card shadow-sm">
        <div class="card-body">
          <h2 class="h5 mb-3">{{ 'auth.login' | translate }}</h2>

          <form (ngSubmit)="submit()" #f="ngForm">
            <div class="mb-3">
              <label class="form-label">{{ 'auth.login_field' | translate }}</label>
              <input class="form-control" name="login" [(ngModel)]="login" required>
            </div>
            <div class="mb-3">
              <label class="form-label">{{ 'auth.password' | translate }}</label>
              <input class="form-control" type="password" name="password" [(ngModel)]="password" required>
            </div>

            @if (error()) {
              <div class="alert alert-danger py-2">{{ error() }}</div>
            }

            <button class="btn btn-primary w-100" type="submit" [disabled]="loading()">
              @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span> }
              {{ 'auth.loginCta' | translate }}
            </button>

            <div class="text-center mt-3">
              <a routerLink="/register">{{ 'auth.noAccount' | translate }}</a>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  login = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (!this.login.trim() || !this.password) {
      this.error.set('Заповніть поля');
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth.login({ login: this.login.trim(), password: this.password }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate([this.auth.isAdmin() ? '/admin/users' : '/app/garages']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.title ?? err?.error?.detail ?? err?.message ?? 'Login failed');
      }
    });
  }
}
