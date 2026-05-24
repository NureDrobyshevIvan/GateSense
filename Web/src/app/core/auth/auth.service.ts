import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginRequest, LoginResponse, RegisterRequest } from '../models/api.models';
import { TokenStorage } from './token.storage';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private store = inject(TokenStorage);

  readonly role = signal<string | null>(this.store.role);
  readonly userName = signal<string | null>(this.store.userName);
  readonly isAuthenticated = computed(() => !!this.role());
  readonly isAdmin = computed(() => (this.role() ?? '').toLowerCase() === 'admin');

  login(req: LoginRequest): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${environment.apiBaseUrl}/Auth/login`, req).pipe(
      map(r => r.data),
      tap(r => {
        this.store.save(r.accessToken ?? null, r.refreshToken ?? null, r.role, r.userName ?? null);
        this.role.set(r.role);
        this.userName.set(r.userName ?? null);
      })
    );
  }

  register(req: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/Auth/register`, req);
  }

  logout(): void {
    this.http.get(`${environment.apiBaseUrl}/Auth/logout`).subscribe({
      next: () => {},
      error: () => {}
    });
    this.store.clear();
    this.role.set(null);
    this.userName.set(null);
  }
}
