import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '';

  get<T>(url: string, params?: Record<string, any>): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${url}`, { params });
  }

  post<T>(url: string, body: unknown): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${url}`, body);
  }

  put<T>(url: string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${url}`, body);
  }

  patch<T>(url: string, body: unknown): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}${url}`, body);
  }

  delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${url}`);
  }

  /** Blob typé depuis Content-Type (permet d’ouvrir PDF / images dans un onglet au lieu de télécharger). */
  getBlob(url: string): Observable<Blob> {
    return this.http
      .get(`${this.baseUrl}${url}`, { responseType: 'blob', observe: 'response' })
      .pipe(
        map((res) => {
          const body = res.body;
          if (!body) return new Blob();
          const headerType = res.headers.get('content-type')?.split(';')[0]?.trim();
          if (
            headerType &&
            (!body.type || body.type === 'application/octet-stream')
          ) {
            return new Blob([body], { type: headerType });
          }
          return body;
        }),
      );
  }
}
