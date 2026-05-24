import { Injectable } from '@angular/core';

const ACCESS_KEY = 'gs_access_token';
const REFRESH_KEY = 'gs_refresh_token';
const ROLE_KEY = 'gs_role';
const USER_KEY = 'gs_user_name';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  get accessToken(): string | null {
    return localStorage.getItem(ACCESS_KEY);
  }
  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }
  get role(): string | null {
    return localStorage.getItem(ROLE_KEY);
  }
  get userName(): string | null {
    return localStorage.getItem(USER_KEY);
  }

  save(access: string | null, refresh: string | null, role: string | null, userName: string | null): void {
    if (access) localStorage.setItem(ACCESS_KEY, access);
    if (refresh) localStorage.setItem(REFRESH_KEY, refresh);
    if (role) localStorage.setItem(ROLE_KEY, role);
    if (userName) localStorage.setItem(USER_KEY, userName);
  }

  clear(): void {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
