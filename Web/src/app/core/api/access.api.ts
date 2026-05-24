import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, AssignFamilyAccessRequest, CreateGuestAccessRequest,
  CreateGuestAccessResponse, GarageMember
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AccessApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/access`;

  members(garageId: number): Observable<GarageMember[]> {
    return this.http.get<ApiResponse<GarageMember[]>>(`${this.base}/garages/${garageId}/members`).pipe(map(r => r.data));
  }
  assignFamily(body: AssignFamilyAccessRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/family`, body);
  }
  createGuest(body: CreateGuestAccessRequest): Observable<CreateGuestAccessResponse> {
    return this.http.post<ApiResponse<CreateGuestAccessResponse>>(`${this.base}/guest`, body).pipe(map(r => r.data));
  }
  revoke(accessId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${accessId}`);
  }
}
