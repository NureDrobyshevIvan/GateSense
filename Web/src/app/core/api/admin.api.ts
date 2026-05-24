import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminGarage, AdminUpdateUserRequest, AdminUser, ApiResponse, AssignRoleRequest,
  UpdateGarageRequest
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AdminApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/admin`;

  // Users
  listUsers(): Observable<AdminUser[]> {
    return this.http.get<ApiResponse<AdminUser[]>>(`${this.base}/users`).pipe(map(r => r.data));
  }
  getUser(id: number): Observable<AdminUser> {
    return this.http.get<ApiResponse<AdminUser>>(`${this.base}/users/${id}`).pipe(map(r => r.data));
  }
  updateUser(id: number, body: AdminUpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/users/${id}`, body);
  }
  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/users/${id}`);
  }
  assignRole(userId: number, role: string): Observable<void> {
    return this.http.post<void>(`${this.base}/users/${userId}/roles`, { role } as AssignRoleRequest);
  }
  removeRole(userId: number, role: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/users/${userId}/roles/${role}`);
  }

  // Garages
  listGarages(): Observable<AdminGarage[]> {
    return this.http.get<ApiResponse<AdminGarage[]>>(`${this.base}/garages`).pipe(map(r => r.data));
  }
  updateGarage(id: number, body: UpdateGarageRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/garages/${id}`, body);
  }
  deleteGarage(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/garages/${id}`);
  }
}
