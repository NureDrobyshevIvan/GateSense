import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

import { GaragesApi } from '../../../core/api/garages.api';
import { Garage } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-garages-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2 class="h4 mb-0">{{ 'garages.title' | translate }}</h2>
      <button class="btn btn-primary" (click)="openCreate(createTpl)">
        + {{ 'garages.add' | translate }}
      </button>
    </div>

    @if (loading()) {
      <div class="text-muted">{{ 'common.loading' | translate }}</div>
    } @else if (items().length === 0) {
      <div class="alert alert-light">{{ 'garages.empty' | translate }}</div>
    } @else {
      <div class="row g-3">
        @for (g of items(); track g.id) {
          <div class="col-md-4">
            <div class="card h-100 shadow-sm">
              <div class="card-body">
                <h5 class="card-title">{{ g.name }}</h5>
                @if (g.address) {
                  <p class="card-text text-muted">{{ g.address }}</p>
                }
              </div>
              <div class="card-footer bg-white d-flex gap-2">
                <a class="btn btn-sm btn-primary" [routerLink]="['/app/garages', g.id]">
                  {{ 'garage.openGate' | translate }}/{{ 'garage.closeGate' | translate }}
                </a>
                <button class="btn btn-sm btn-outline-danger ms-auto" (click)="del(g)">
                  {{ 'garages.delete' | translate }}
                </button>
              </div>
            </div>
          </div>
        }
      </div>
    }

    <ng-template #createTpl let-modal>
      <div class="modal-header">
        <h5 class="modal-title">{{ 'garages.add' | translate }}</h5>
      </div>
      <div class="modal-body">
        <div class="mb-2">
          <label class="form-label">{{ 'garages.name' | translate }}</label>
          <input class="form-control" [(ngModel)]="newName">
        </div>
        <div class="mb-2">
          <label class="form-label">{{ 'garages.address' | translate }}</label>
          <input class="form-control" [(ngModel)]="newAddress">
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-secondary" (click)="modal.dismiss()">{{ 'garages.cancel' | translate }}</button>
        <button class="btn btn-primary" (click)="create(modal)">{{ 'garages.create' | translate }}</button>
      </div>
    </ng-template>
  `
})
export class GaragesListComponent implements OnInit {
  private api = inject(GaragesApi);
  private modal = inject(NgbModal);
  private lang = inject(LanguageService);

  items = signal<Garage[]>([]);
  loading = signal(false);
  newName = '';
  newAddress = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (data) => {
        const collator = this.lang.collator();
        this.items.set([...data].sort((a, b) => collator.compare(a.name, b.name)));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(tpl: any): void {
    this.newName = '';
    this.newAddress = '';
    this.modal.open(tpl);
  }

  create(modalRef: any): void {
    if (!this.newName.trim()) return;
    this.api.create({ name: this.newName.trim(), address: this.newAddress.trim() || undefined }).subscribe(() => {
      modalRef.close();
      this.load();
    });
  }

  del(g: Garage): void {
    if (!confirm('Видалити?')) return;
    this.api.delete(g.id).subscribe(() => this.load());
  }
}
