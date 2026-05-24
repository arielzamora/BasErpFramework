import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: string;
  codigo: string;
  nombre: string;
  precio: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  // URL base dinámica que se inyectará en producción, o usa localhost para desarrollo
  private get baseUrl(): string {
    const envUrl = (window as any).env?.apiUrl;
    return envUrl ? `${envUrl}/api/productos` : 'https://localhost:7197/api/productos';
  }

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  create(product: Partial<Product>): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, product);
  }

  update(id: string, product: Partial<Product>): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, product);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  semanticSearch(query: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/semantic-search`, {
      params: { q: query }
    });
  }
}
