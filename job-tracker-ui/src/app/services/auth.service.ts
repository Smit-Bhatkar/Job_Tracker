import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = '/api/auth';

  // Reactive signals for auth state
  private currentUser = signal<AuthResponse | null>(this.loadUserFromStorage());

  user = this.currentUser.asReadonly();
  isLoggedIn = computed(() => this.currentUser() !== null);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((user) => {
        sessionStorage.setItem('user', JSON.stringify(user));
        this.currentUser.set(user);
      }),
      catchError((err) => {
        return throwError(() => err);
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, request).pipe(
      tap((user) => {
        sessionStorage.setItem('user', JSON.stringify(user));
        this.currentUser.set(user);
      }),
      catchError((err) => {
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  private loadUserFromStorage(): AuthResponse | null {
    const stored = sessionStorage.getItem('user');
    if (stored) {
      try {
        return JSON.parse(stored);
      } catch {
        sessionStorage.removeItem('user');
      }
    }
    return null;
  }
}
