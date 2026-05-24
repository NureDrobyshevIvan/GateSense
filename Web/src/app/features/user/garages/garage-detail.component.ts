import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { GaragesApi } from '../../../core/api/garages.api';
import { GateApi } from '../../../core/api/gate.api';
import { SensorsApi } from '../../../core/api/sensors.api';
import { Garage, GateStateResponse, SensorReading, SensorTypeLabels } from '../../../core/models/api.models';
import { LanguageService } from '../../../core/i18n/language.service';

@Component({
  selector: 'app-garage-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  template: `
    <a routerLink="/app/garages" class="text-decoration-none">&larr; {{ 'garages.title' | translate }}</a>
    <h2 class="h4 mt-2">{{ garage()?.name }}</h2>

    @if (alerts().length) {
      <div class="alert alert-danger">
        <strong>⚠ {{ 'garage.alert' | translate }}:</strong>
        @for (a of alerts(); track a.id) {
          <span class="ms-2">{{ labelFor(a.sensorType) }} = {{ a.value }} {{ a.unit }}</span>
        }
      </div>
    }

    <div class="row g-4">
      <div class="col-md-5">
        <div class="card text-center shadow-sm">
          <div class="card-body py-5">
            <div class="display-1 mb-3">{{ isOpen() ? '🔓' : '🔒' }}</div>
            <h3 class="h2 mb-3">{{ stateLabel() | translate }}</h3>
            <button class="btn btn-lg" [class.btn-warning]="isOpen()" [class.btn-success]="!isOpen()"
                    (click)="toggle()" [disabled]="acting()">
              @if (acting()) { <span class="spinner-border spinner-border-sm me-2"></span> }
              {{ (isOpen() ? 'garage.closeGate' : 'garage.openGate') | translate }}
            </button>
            @if (state()?.lastActionTime) {
              <div class="text-muted small mt-3">
                {{ 'garage.lastAction' | translate }}: {{ state()!.lastActionTime | date:'medium':undefined:lang.current() }}
              </div>
            }
          </div>
        </div>
      </div>

      <div class="col-md-7">
        <div class="card shadow-sm">
          <div class="card-header">{{ 'garage.sensors' | translate }}</div>
          <div class="card-body">
            @if (latest().length === 0) {
              <div class="text-muted">{{ 'garage.noSensorData' | translate }}</div>
            } @else {
              <table class="table table-sm mb-0">
                <tbody>
                  @for (r of latest(); track r.id) {
                    <tr>
                      <td>{{ labelFor(r.sensorType) }}</td>
                      <td class="text-end fw-semibold">{{ r.value }} {{ r.unit }}</td>
                      <td class="text-end text-muted small">{{ r.recordedOn | date:'short':undefined:lang.current() }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        </div>

        <div class="d-flex gap-2 mt-3">
          <a class="btn btn-outline-primary" [routerLink]="['/app/garages', garageId(), 'events']">
            {{ 'garage.events' | translate }}
          </a>
          <a class="btn btn-outline-primary" [routerLink]="['/app/garages', garageId(), 'access']">
            {{ 'garage.access' | translate }}
          </a>
        </div>
      </div>
    </div>
  `
})
export class GarageDetailComponent implements OnInit {
  garageId = input.required<number>();

  private garagesApi = inject(GaragesApi);
  private gateApi = inject(GateApi);
  private sensorsApi = inject(SensorsApi);
  lang = inject(LanguageService);

  garage = signal<Garage | null>(null);
  state = signal<GateStateResponse | null>(null);
  latest = signal<SensorReading[]>([]);
  alerts = signal<SensorReading[]>([]);
  acting = signal(false);

  ngOnInit(): void { this.refresh(); }

  isOpen(): boolean { return this.state()?.state?.toLowerCase() === 'open'; }
  stateLabel(): string {
    const s = this.state()?.state?.toLowerCase();
    if (s === 'open') return 'garage.open';
    if (s === 'closed') return 'garage.closed';
    return 'garage.unknown';
  }
  labelFor(t?: number | null): string {
    return t != null ? (SensorTypeLabels[t] ?? '?') : '?';
  }

  refresh(): void {
    const id = this.garageId();
    this.garagesApi.get(id).subscribe(g => this.garage.set(g));
    this.gateApi.state(id).subscribe(s => this.state.set(s));
    this.sensorsApi.latest(id).subscribe(r => this.latest.set(r));
    this.sensorsApi.alerts(id).subscribe(a => this.alerts.set(a));
  }

  toggle(): void {
    const id = this.garageId();
    this.acting.set(true);
    const op = this.isOpen() ? this.gateApi.close(id) : this.gateApi.open(id);
    op.subscribe({
      next: () => this.gateApi.state(id).subscribe(s => { this.state.set(s); this.acting.set(false); }),
      error: () => this.acting.set(false)
    });
  }
}
