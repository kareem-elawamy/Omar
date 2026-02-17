import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Employee } from '../interface/employee';
import { map, Observable } from 'rxjs';
import { AuthResponse } from '../interface/AuthResponse';
import { jwtDecode } from 'jwt-decode';
export interface JwtPayload {
  role: string[];
  name: string;
  nameid: string;
  email: string;
  exp: number;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private apiUrl: string = 'http://localhost:5263';
  private http = inject(HttpClient); // استخدام HttpClient أسهل وأفضل
  private token = 'token'; // استرجاع التوكن من التخزين

  addEmployee(employee: Employee): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.token}`
    });

    return this.http.post(`${this.apiUrl}/api/Account/addEmployee`, employee, { headers });
  }
  login(employee: Employee): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/Account/login`, employee).pipe(
      map((res) => {
        if (res.isSuccess) {
          localStorage.setItem(this.token, res.tokens);
          console.log(res.tokens);
        }
        console.log(res.message);

        return res;
      }));
  }
  getToken(): string {
    return localStorage.getItem(this.token) || '';
  }
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const decoded = jwtDecode<JwtPayload>(token)
      const currentTime = Math.floor(Date.now() / 1000)
      if (decoded.exp && decoded.exp < currentTime) {
        this.logout();
        return false;
      }
      return true
    } catch (error) {
      console.error('Failed to decode token', error);
      this.logout()
      return false;
    }

  }
  logout(): void {
    localStorage.removeItem(this.token);
  }
  getUserRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded.role || [];
    } catch (error) {
      console.error('Failed to decode token', error);
      return [];
    }
  }
  getUserId(): string | '' {
    const token = this.getToken();
    if (token === '') return '';

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded.nameid || '';
    } catch (error) {
      console.error('Failed to decode token', error);
      return '';
    }
  }

  isAdmin(): boolean {
    return this.getUserRoles().includes('Admin');
  }
  isEmployee(): boolean {
    return this.getUserRoles().includes('Employee');
  }

}
