import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AuthService } from '../../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LanguageSwitcherComponent],
  template: `
    <div class="container py-5" style="max-width: 520px;">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h3 mb-0">{{ 'app.title' | translate }}</h1>
        <app-language-switcher></app-language-switcher>
      </div>
      <div class="card shadow-sm">
        <div class="card-body">
          <h2 class="h5 mb-3">{{ 'auth.register' | translate }}</h2>

          <form (ngSubmit)="submit()">
            <div class="row g-2 mb-2">
              <div class="col">
                <label class="form-label">{{ 'auth.firstName' | translate }}</label>
                <input class="form-control" name="firstName" [(ngModel)]="firstName">
              </div>
              <div class="col">
                <label class="form-label">{{ 'auth.lastName' | translate }}</label>
                <input class="form-control" name="lastName" [(ngModel)]="lastName">
              </div>
            </div>
            <div class="mb-2">
              <label class="form-label">{{ 'auth.email' | translate }}</label>
              <input class="form-control" type="email" name="email" [(ngModel)]="email">
            </div>
            <div class="mb-2">
              <label class="form-label">{{ 'auth.userName' | translate }}</label>
              <input class="form-control" name="userName" [(ngModel)]="userName">
            </div>
            <div class="mb-3">
              <label class="form-label">{{ 'auth.password' | translate }}</label>
              <input class="form-control" type="password" name="password" [(ngModel)]="password">
            </div>

            @if (error()) {
              <div class="alert alert-danger py-2">{{ error() }}</div>
            }

            <button class="btn btn-primary w-100" type="submit" [disabled]="loading()">
              @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span> }
              {{ 'auth.registerCta' | translate }}
            </button>

            <div class="text-center mt-3">
              <a routerLink="/login">{{ 'auth.haveAccount' | translate }}</a>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  firstName = '';
  lastName = '';
  email = '';
  userName = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (![this.firstName, this.lastName, this.email, this.userName, this.password].every(v => v.trim())) {
      this.error.set('Заповніть усі поля');
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth.register({
      firstName: this.firstName.trim(),
      lastName: this.lastName.trim(),
      email: this.email.trim(),
      userName: this.userName.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.title ?? err?.error?.detail ?? err?.message ?? 'Register failed');
      }
    });
  }
}
