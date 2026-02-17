import { Routes } from '@angular/router';
import { AddEmployee } from './Components/add-employee/add-employee';
import { Login } from './Components/login/login';

export const routes: Routes = [
  { path: 'add-employee', component: AddEmployee },
  { path: 'login', component: Login }
];
