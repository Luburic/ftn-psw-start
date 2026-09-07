import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const TOKEN_KEY = 'explorer.token';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export interface User {
  id: string;
  email: string;
  roles: string[];
}

interface TokenResponse {
  accessToken: string;
}

function decodeUser(token: string | null): User | null {
  if (token === null) {
    return null;
  }
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    const claim = payload[ROLE_CLAIM];
    let roles: string[] = [];
    if (typeof claim === 'string') {
      roles = [claim];
    } else if (Array.isArray(claim)) {
      roles = claim;
    }
    return { id: payload.sub, email: payload.email, roles };
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  readonly accessToken = computed(() => this.token());
  readonly user = computed(() => decodeUser(this.token()));
  readonly isLoggedIn = computed(() => this.user() !== null);

  async login(email: string, password: string): Promise<void> {
    await this.authenticate('/api/identity/login', email, password);
  }

  async register(email: string, password: string): Promise<void> {
    await this.authenticate('/api/identity/register', email, password);
  }

  logout(): void {
    this.token.set(null);
    localStorage.removeItem(TOKEN_KEY);
  }

  private async authenticate(url: string, email: string, password: string): Promise<void> {
    const response = await firstValueFrom(this.http.post<TokenResponse>(url, { email, password }));
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    this.token.set(response.accessToken);
  }
}
