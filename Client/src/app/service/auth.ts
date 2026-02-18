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
  private token = 'auth_token';
  addEmployee(employee: Employee): Observable<any> {

    return this.http.post(`${this.apiUrl}/api/Account/addEmployee`, employee);
  }
  login(employee: Employee): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/Account/login`, employee).pipe(
      map((res) => {
        if (res.token) {
          localStorage.setItem(this.token, res.token);
          console.log('Token Saved:', res.token);
        } else {
          console.warn('Login failed:', res.message);
        }

        return res;
      }),
    );
  }
  getToken(): string {
    return localStorage.getItem(this.token) || '';
  }
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const currentTime = Math.floor(Date.now() / 1000);
      if (decoded.exp && decoded.exp < currentTime) {
        this.logout();
        return false;
      }
      return true;
    } catch (error) {
      console.error('Failed to decode token', error);
      this.logout();
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
    return this.getUserRoles().includes('Owner');
  }
  isEmployee(): boolean {
    return this.getUserRoles().includes('Employee');
  }
  saveToken(token: string): void {
    localStorage.setItem(this.token, token);
  }
  getuserEmail(): string {
    const token = this.getToken();
    if (!token) return '';
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded.email || '';
    } catch (error) {
      console.error('Failed to decode token', error);
      return '';

    }
  }
}
