import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { LogsApi } from '../../../core/api/logs.api';
import { GateActionLabels, GateEvent, GateResultLabels, TriggerSourceLabels } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-event-log',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  template: `
    <a [routerLink]="['/app/garages', garageId()]" class="text-decoration-none">&larr; Garage</a>
    <h2 class="h4 mt-2">{{ 'events.title' | translate }}</h2>

    @if (items().length === 0) {
      <div class="alert alert-light">{{ 'events.empty' | translate }}</div>
    } @else {
      <table class="table table-sm table-hover">
        <thead>
          <tr>
            <th>{{ 'events.time' | translate }}</th>
            <th>{{ 'events.action' | translate }}</th>
            <th>{{ 'events.trigger' | translate }}</th>
            <th>{{ 'events.result' | translate }}</th>
          </tr>
        </thead>
        <tbody>
          @for (e of items(); track e.id) {
            <tr>
              <td>{{ e.createdOn | date:'medium':undefined:lang.current() }}</td>
              <td><span class="badge bg-secondary">{{ actionLabel(e.action) }}</span></td>
              <td>{{ triggerLabel(e.triggerSource) }}</td>
              <td>
                @if (e.result === 0) {
                  <span class="text-success">{{ resultLabel(e.result) }}</span>
                } @else {
                  <span class="text-danger">{{ resultLabel(e.result) }} ({{ e.failureReason }})</span>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class EventLogComponent implements OnInit {
  garageId = input.required<number>();
  private api = inject(LogsApi);
  lang = inject(LanguageService);

  items = signal<GateEvent[]>([]);

  ngOnInit(): void {
    this.api.gateLogs(this.garageId()).subscribe(p => this.items.set(p.items));
  }

  actionLabel(a?: number | null): string { return a != null ? (GateActionLabels[a] ?? '?') : '?'; }
  triggerLabel(t?: number | null): string { return t != null ? (TriggerSourceLabels[t] ?? '?') : '?'; }
  resultLabel(r?: number | null): string { return r != null ? (GateResultLabels[r] ?? '?') : '?'; }
}
