import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SummaryResponse } from '../interface/summaryResponse';
import { ChartDataInterface } from '../interface/chart-data';
import { TopProducts } from '../interface/top-products';
import { InventoryStatus } from '../interface/inventory-status';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private apiUrl: string = 'http://localhost:5263';
  private http = inject(HttpClient); // استخدام HttpClient أسهل وأفضل
  //api/Dashboard/summary?month=5&year=2024
  getSummary(month: number, year: number): Observable<SummaryResponse> {
    return this.http.get<SummaryResponse>(`${this.apiUrl}/api/Dashboard/summary?month=${month}&year=${year}`);
  }
  //api/Dashboard/Chart-Data
  getChartData(): Observable<ChartDataInterface> {
    return this.http.get<ChartDataInterface>(`${this.apiUrl}/api/Dashboard/Chart-Data`);
  }
  //api/Dashboard/top-products
  getTopProducts(): Observable<TopProducts[]> {
    return this.http.get<TopProducts[]>(`${this.apiUrl}/api/Dashboard/top-products`);
  }
  //api/Dashboard/inventory-status
  getInventoryStatus(): Observable<InventoryStatus> {
    return this.http.get<InventoryStatus>(`${this.apiUrl}/api/Dashboard/inventory-status`);
  }


}
