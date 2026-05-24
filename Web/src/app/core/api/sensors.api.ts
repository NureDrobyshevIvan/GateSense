import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Paginated, SensorReading } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class SensorsApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/garages`;

  latest(garageId: number): Observable<SensorReading[]> {
    return this.http.get<ApiResponse<SensorReading[]>>(`${this.base}/${garageId}/sensors/latest`).pipe(map(r => r.data));
  }
  alerts(garageId: number): Observable<SensorReading[]> {
    return this.http.get<ApiResponse<SensorReading[]>>(`${this.base}/${garageId}/sensors/alerts`).pipe(map(r => r.data));
  }
  history(garageId: number, page = 1, pageSize = 50): Observable<Paginated<SensorReading>> {
    const params = new HttpParams().set('PageNumber', page).set('PageSize', pageSize);
    return this.http.get<ApiResponse<Paginated<SensorReading>>>(`${this.base}/${garageId}/sensors/history`, { params })
      .pipe(map(r => r.data));
  }
}
