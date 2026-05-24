import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, GateCommandRequest, GateStateResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class GateApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/garages`;

  open(garageId: number, body: GateCommandRequest = {}): Observable<void> {
    return this.http.post<void>(`${this.base}/${garageId}/gate/open`, body);
  }
  close(garageId: number, body: GateCommandRequest = {}): Observable<void> {
    return this.http.post<void>(`${this.base}/${garageId}/gate/close`, body);
  }
  state(garageId: number): Observable<GateStateResponse> {
    return this.http.get<ApiResponse<GateStateResponse>>(`${this.base}/${garageId}/gate/state`).pipe(map(r => r.data));
  }
}
