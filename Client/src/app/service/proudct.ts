import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Category } from '../Enum/category';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Proudct {
  private apiUrl: string = 'http://localhost:5263';
  private http = inject(HttpClient);
  getProducts(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/api/Products`);
  }
  getProductsByCategory(category: Category) {
    return this.http.get(`${this.apiUrl}/api/Products/ByCategory/${category}`);
  }
  addProduct(product: Proudct): Observable<any> {
    return this.http.post(`${this.apiUrl}/api/Products`, product);
  }
  updateProduct(id: number, product: Proudct): Observable<any> {
    return this.http.put(`${this.apiUrl}/api/Products/${id}`, product);
  }
  UpdateStock(id: number, stock: number) {
    return this.http.patch(`${this.apiUrl}/api/Products/${id}/stock`, { stock });
  }
  deactivateProduct(id: number) {

    return this.http.delete(`${this.apiUrl}/api/Products/${id}`);
  }
}
