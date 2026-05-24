import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { BackupApi } from '../../../core/api/backup.api';

@Component({
  selector: 'app-backup',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <h2 class="h4 mb-3">{{ 'admin.backup.title' | translate }}</h2>

    <div class="row g-4">
      <div class="col-md-6">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5 class="card-title">{{ 'admin.backup.export' | translate }}</h5>
            <p class="text-muted">{{ 'admin.backup.exportHint' | translate }}</p>
            <button class="btn btn-primary" (click)="doExport()" [disabled]="exporting()">
              @if (exporting()) { <span class="spinner-border spinner-border-sm me-2"></span> }
              {{ 'admin.backup.export' | translate }}
            </button>
          </div>
        </div>
      </div>

      <div class="col-md-6">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5 class="card-title">{{ 'admin.backup.import' | translate }}</h5>
            <p class="text-muted">{{ 'admin.backup.importHint' | translate }}</p>
            <input #fileInput type="file" accept="application/json" (change)="onFile($event)" class="form-control mb-2">
            <button class="btn btn-primary" [disabled]="!selectedFile() || importing()" (click)="doImport()">
              @if (importing()) { <span class="spinner-border spinner-border-sm me-2"></span> }
              {{ 'admin.backup.import' | translate }}
            </button>

            @if (resultMessage()) {
              <div class="alert mt-3" [class.alert-success]="resultOk()" [class.alert-danger]="!resultOk()">
                {{ resultMessage() }}
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class BackupComponent {
  private api = inject(BackupApi);
  private translate = inject(TranslateService);

  exporting = signal(false);
  importing = signal(false);
  selectedFile = signal<File | null>(null);
  resultMessage = signal<string | null>(null);
  resultOk = signal(true);

  doExport(): void {
    this.exporting.set(true);
    this.api.exportBackup().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        const ts = new Date().toISOString().replace(/[:.]/g, '-');
        a.href = url;
        a.download = `gatesense-backup-${ts}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: () => { this.exporting.set(false); }
    });
  }

  onFile(e: Event): void {
    const input = e.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
    this.resultMessage.set(null);
  }

  doImport(): void {
    const file = this.selectedFile();
    if (!file) return;
    this.importing.set(true);
    this.api.importBackup(file).subscribe({
      next: (res) => {
        this.importing.set(false);
        this.resultOk.set(true);
        const total = res.data.usersImported + res.data.garagesImported + res.data.devicesImported +
                      res.data.accessKeysImported + res.data.garageAccessImported +
                      res.data.gateEventsImported + res.data.sensorReadingsImported;
        this.translate.get('admin.backup.importSuccess', { count: total }).subscribe(msg => this.resultMessage.set(msg));
      },
      error: (err) => {
        this.importing.set(false);
        this.resultOk.set(false);
        this.translate.get('admin.backup.importError').subscribe(msg =>
          this.resultMessage.set(`${msg}: ${err?.error?.error ?? err?.message}`)
        );
      }
    });
  }
}
