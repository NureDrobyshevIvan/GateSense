import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { AdminApi } from '../../../core/api/admin.api';
import { AdminGarage } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-admin-garages',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <h2 class="h4 mb-3">{{ 'admin.garages.title' | translate }}</h2>

    <table class="table table-hover">
      <thead>
        <tr>
          <th>#</th>
          <th>{{ 'admin.garages.name' | translate }}</th>
          <th>{{ 'admin.garages.address' | translate }}</th>
          <th>{{ 'admin.garages.owner' | translate }}</th>
          <th class="text-end">{{ 'admin.garages.actions' | translate }}</th>
        </tr>
      </thead>
      <tbody>
        @for (g of items(); track g.id) {
          <tr>
            <td>{{ g.id }}</td>
            <td>{{ g.name }}</td>
            <td>{{ g.address }}</td>
            <td>{{ g.ownerUserName || g.ownerEmail || g.ownerId || '-' }}</td>
            <td class="text-end">
              <button class="btn btn-sm btn-outline-danger" (click)="del(g)">Delete</button>
            </td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class AdminGaragesComponent implements OnInit {
  private api = inject(AdminApi);
  private lang = inject(LanguageService);
  items = signal<AdminGarage[]>([]);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.listGarages().subscribe(list => {
      const collator = this.lang.collator();
      this.items.set([...list].sort((a, b) => collator.compare(a.name, b.name)));
    });
  }

  del(g: AdminGarage): void {
    if (!confirm('Delete garage?')) return;
    this.api.deleteGarage(g.id).subscribe(() => this.load());
  }
}
