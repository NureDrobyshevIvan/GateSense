import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AccessApi } from '../../../core/api/access.api';
import { AccessLevelLabels, CreateGuestAccessResponse, GarageMember } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-access-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  template: `
    <a [routerLink]="['/app/garages', garageId()]" class="text-decoration-none">&larr; Garage</a>
    <h2 class="h4 mt-2">{{ 'access.title' | translate }}</h2>

    <div class="row g-3 mt-2">
      <div class="col-md-6">
        <div class="card shadow-sm">
          <div class="card-header">{{ 'access.addFamily' | translate }}</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">{{ 'access.familyEmail' | translate }}</label>
              <input class="form-control" [(ngModel)]="familyEmail" type="email">
            </div>
            <button class="btn btn-primary" (click)="addFamily()" [disabled]="!familyEmail">
              {{ 'access.addFamily' | translate }}
            </button>
          </div>
        </div>
      </div>

      <div class="col-md-6">
        <div class="card shadow-sm">
          <div class="card-header">{{ 'access.createGuest' | translate }}</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">{{ 'access.guestName' | translate }}</label>
              <input class="form-control" [(ngModel)]="guestName">
            </div>
            <div class="mb-2">
              <label class="form-label">{{ 'access.guestExpires' | translate }}</label>
              <input class="form-control" [(ngModel)]="guestExpires" type="datetime-local">
            </div>
            <button class="btn btn-primary" (click)="createGuest()" [disabled]="!guestName">
              {{ 'access.createGuest' | translate }}
            </button>
            @if (lastToken()) {
              <div class="alert alert-success mt-2 small">
                <strong>{{ 'access.createdToken' | translate }}:</strong>
                <div class="text-break font-monospace">{{ lastToken()!.token }}</div>
              </div>
            }
          </div>
        </div>
      </div>
    </div>

    <h3 class="h5 mt-4">{{ 'access.members' | translate }}</h3>
    @if (members().length === 0) {
      <div class="alert alert-light">{{ 'access.empty' | translate }}</div>
    } @else {
      <table class="table">
        <thead>
          <tr><th>User</th><th>{{ 'admin.users.roles' | translate }}</th><th>Expires</th><th></th></tr>
        </thead>
        <tbody>
          @for (m of members(); track m.id) {
            <tr>
              <td>{{ m.user?.userName ?? m.user?.email ?? ('#' + m.userId) }}</td>
              <td><span class="badge bg-info">{{ levelLabel(m.accessLevel) }}</span></td>
              <td>{{ m.expiresOn ? (m.expiresOn | date:'short':undefined:lang.current()) : '-' }}</td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-danger" (click)="revoke(m)">
                  {{ 'access.revoke' | translate }}
                </button>
              </td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class AccessManagementComponent implements OnInit {
  garageId = input.required<number>();
  private api = inject(AccessApi);
  lang = inject(LanguageService);

  members = signal<GarageMember[]>([]);
  familyEmail = '';
  guestName = '';
  guestExpires = '';
  lastToken = signal<CreateGuestAccessResponse | null>(null);

  ngOnInit(): void { this.load(); }

  load(): void { this.api.members(this.garageId()).subscribe(m => this.members.set(m)); }
  levelLabel(l?: number | null): string { return l != null ? (AccessLevelLabels[l] ?? '?') : '?'; }

  addFamily(): void {
    this.api.assignFamily({ garageId: this.garageId(), email: this.familyEmail.trim() }).subscribe(() => {
      this.familyEmail = '';
      this.load();
    });
  }

  createGuest(): void {
    this.api.createGuest({
      garageId: this.garageId(),
      recipientName: this.guestName.trim(),
      expiresOn: this.guestExpires ? new Date(this.guestExpires).toISOString() : undefined
    }).subscribe(res => {
      this.lastToken.set(res);
      this.guestName = '';
      this.guestExpires = '';
      this.load();
    });
  }

  revoke(m: GarageMember): void {
    this.api.revoke(m.id).subscribe(() => this.load());
  }
}
