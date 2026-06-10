import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Application } from '../models/application.model';

@Injectable({
  providedIn: 'root',
})
export class ApplicationService {
  private http = inject(HttpClient);
  private baseUrl = '/api/applications';
  getAll(userId?: string): Observable<Application[]> {
    if (userId) {
      return this.http.get<Application[]>(this.baseUrl + `?userId=${encodeURIComponent(userId)}`);
    }
    return this.http.get<Application[]>(this.baseUrl);
  }

  getById(id: string): Observable<Application> {
    return this.http.get<Application>(`${this.baseUrl}/${id}`);
  }

  create(app: Partial<Application>): Observable<Application> {
    return this.http.post<Application>(this.baseUrl, app);
  }

  update(id: string, app: Partial<Application>): Observable<Application> {
    return this.http.put<Application>(`${this.baseUrl}/${id}`, app);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
