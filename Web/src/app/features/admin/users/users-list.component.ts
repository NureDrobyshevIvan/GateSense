import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { AdminApi } from '../../../core/api/admin.api';
import { AdminUser } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <h2 class="h4 mb-3">{{ 'admin.users.title' | translate }}</h2>

    <div class="mb-3">
      <input class="form-control" style="max-width:300px"
             placeholder="Search..." [(ngModel)]="filter" (input)="apply()">
    </div>

    @if (filtered().length === 0) {
      <div class="text-muted">{{ 'common.loading' | translate }}</div>
    }

    <table class="table table-hover">
      <thead>
        <tr>
          <th (click)="sortBy('userName')" style="cursor:pointer">{{ 'admin.users.userName' | translate }}</th>
          <th (click)="sortBy('email')" style="cursor:pointer">{{ 'admin.users.email' | translate }}</th>
          <th>{{ 'admin.users.firstName' | translate }}</th>
          <th>{{ 'admin.users.lastName' | translate }}</th>
          <th>{{ 'admin.users.roles' | translate }}</th>
          <th class="text-end">{{ 'admin.users.actions' | translate }}</th>
        </tr>
      </thead>
      <tbody>
        @for (u of filtered(); track u.id) {
          <tr>
            <td>{{ u.userName }}</td>
            <td>{{ u.email }}</td>
            <td>{{ u.firstName }}</td>
            <td>{{ u.lastName }}</td>
            <td>
              @for (r of u.roles; track r) {
                <span class="badge" [class.bg-warning]="r === 'admin'" [class.bg-secondary]="r !== 'admin'" [class.text-dark]="r === 'admin'">
                  {{ r }}
                </span>
              }
            </td>
            <td class="text-end">
              @if (!u.roles.includes('admin')) {
                <button class="btn btn-sm btn-warning me-1" (click)="assign(u, 'admin')">
                  {{ 'admin.users.makeAdmin' | translate }}
                </button>
              } @else {
                <button class="btn btn-sm btn-outline-warning me-1" (click)="remove(u, 'admin')">
                  {{ 'admin.users.removeAdmin' | translate }}
                </button>
              }
              <button class="btn btn-sm btn-outline-danger" (click)="del(u)">
                {{ 'admin.users.delete' | translate }}
              </button>
            </td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class UsersListComponent implements OnInit {
  private api = inject(AdminApi);
  private lang = inject(LanguageService);

  all = signal<AdminUser[]>([]);
  filtered = signal<AdminUser[]>([]);
  filter = '';
  sortKey: keyof AdminUser = 'userName';
  sortDir: 1 | -1 = 1;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.listUsers().subscribe(list => {
      this.all.set(list);
      this.apply();
    });
  }

  apply(): void {
    const f = this.filter.trim().toLowerCase();
    const collator = this.lang.collator();
    let list = [...this.all()];
    if (f) {
      list = list.filter(u =>
        (u.userName + ' ' + u.email + ' ' + u.firstName + ' ' + u.lastName).toLowerCase().includes(f)
      );
    }
    list.sort((a, b) => collator.compare(String(a[this.sortKey] ?? ''), String(b[this.sortKey] ?? '')) * this.sortDir);
    this.filtered.set(list);
  }

  sortBy(key: keyof AdminUser): void {
    if (this.sortKey === key) this.sortDir = (this.sortDir === 1 ? -1 : 1);
    else { this.sortKey = key; this.sortDir = 1; }
    this.apply();
  }

  assign(u: AdminUser, role: string): void {
    this.api.assignRole(u.id, role).subscribe(() => this.load());
  }
  remove(u: AdminUser, role: string): void {
    this.api.removeRole(u.id, role).subscribe(() => this.load());
  }
  del(u: AdminUser): void {
    if (!confirm('Delete user?')) return;
    this.api.deleteUser(u.id).subscribe(() => this.load());
  }
}
