import { Component, inject } from '@angular/core';
import { Auth } from '../../service/auth';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Employee } from '../../interface/employee';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private authService = inject(Auth);
  showPassword = false;
  router = inject(Router);
  loading = false;
  private fb = inject(FormBuilder);
  employee: Employee = { email: '', password: '' };
  loginForm = this.fb.group({
    email: [''],
    password: ['']
  });
  onSubmit() {
    if (this.loginForm.valid) {
      this.loading = true;
      const { email, password } = this.loginForm.value;
      this.employee.email = email || '';
      this.employee.password = password || '';
      console.log(this.loginForm.value);
      this.authService.login(this.employee).subscribe({
        // داخل onSubmit في ملف الـ login.ts
        next: (res) => {
          if (res.isSuccess) {
            // يفضل التأكد أن التوكن تم حفظه قبل التوجيه
            console.log('Login Success, Token stored');

            // فحص الأدوار بناءً على ما في التوكن (Owner)
            if (this.authService.isAdmin()) {
              this.router.navigate(['/dashboard']);
            } else if (this.authService.isEmployee()) {
              this.router.navigate(['/pos']);
            } else {
              // دور غير معروف أو افتراضي
              this.router.navigate(['/']);
            }
          }
        },
        error: (err) => {
          console.error('Error:', err);
          this.loading = false;
          alert('فشل تسجيل الدخول. يرجى التحقق من بياناتك.');
        }
      });
    }
  }
}
