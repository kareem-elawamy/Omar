import { Component, inject } from '@angular/core';
import { FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormBuilder } from '@angular/forms';
import { Auth } from '../../service/auth';
import { Employee } from '../../interface/employee';
@Component({
  selector: 'app-add-employee',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './add-employee.html',
  styleUrl: './add-employee.css',
})
export class AddEmployee {
  private fb = inject(FormBuilder);
  private authService = inject(Auth);
  employee: Employee = { email: '', password: '' };
  registerForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
    terms: [false, [Validators.requiredTrue]]
  }, {
    validators: this.passwordMatchValidator
  });
  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  onSubmit() {
    if (this.registerForm.valid) {
      const { email, password } = this.registerForm.value;
      this.employee.email = email;
      this.employee.password = password;
      console.log(this.registerForm.value);
      this.authService.addEmployee(this.employee).subscribe({
        next: (res) => {
          console.log('Success:', res);
          alert('تم إضافة الموظف بنجاح!');
        },
        error: (err) => {
          console.error('Error:', err);
        }
      });
    }
  }
}
