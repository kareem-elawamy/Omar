import { Routes } from '@angular/router';
import { AddEmployee } from './Components/add-employee/add-employee';
import { Login } from './Components/login/login';
import { Dashboard } from './Components/dashboard/dashboard';
import { Pos } from './Components/pos/pos';

export const routes: Routes = [
  { path: 'add-employee', component: AddEmployee },
  { path: 'login', component: Login },
  { path: 'dashboard', component: Dashboard },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login', pathMatch: 'full' },
  { path: 'pos', component: Pos }
];
