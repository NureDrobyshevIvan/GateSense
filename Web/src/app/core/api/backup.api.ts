import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ImportBackupResponse, ApiResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BackupApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/admin/backup`;

  exportBackup(): Observable<Blob> {
    return this.http.get(`${this.base}/export`, { responseType: 'blob' });
  }

  importBackup(file: File): Observable<ApiResponse<ImportBackupResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<ImportBackupResponse>>(`${this.base}/import`, formData);
  }
}
