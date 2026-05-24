import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, CreateGarageRequest, Garage, UpdateGarageRequest
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class GaragesApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/garages`;

  list(): Observable<Garage[]> {
    return this.http.get<ApiResponse<Garage[]>>(this.base).pipe(map(r => r.data));
  }
  get(id: number): Observable<Garage> {
    return this.http.get<ApiResponse<Garage>>(`${this.base}/${id}`).pipe(map(r => r.data));
  }
  create(body: CreateGarageRequest): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.base, body).pipe(map(r => r.data));
  }
  update(id: number, body: UpdateGarageRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
