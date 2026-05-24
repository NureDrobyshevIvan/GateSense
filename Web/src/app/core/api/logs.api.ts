import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, GateEvent, Paginated } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class LogsApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/garages`;

  gateLogs(garageId: number, page = 1, pageSize = 50): Observable<Paginated<GateEvent>> {
    const params = new HttpParams().set('PageNumber', page).set('PageSize', pageSize);
    return this.http.get<ApiResponse<Paginated<GateEvent>>>(`${this.base}/${garageId}/logs/gate`, { params })
      .pipe(map(r => r.data));
  }
}
