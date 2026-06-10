import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { ApplicationListComponent } from './components/application-list/application-list.component';
import { ApplicationFormComponent } from './components/application-form/application-form.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    title: 'Sign In - JobTracker',
  },
  {
    path: 'register',
    component: RegisterComponent,
    title: 'Create Account - JobTracker',
  },
  {
    path: '',
    component: ApplicationListComponent,
    title: 'Applications - JobTracker',
    canActivate: [authGuard],
  },
  {
    path: 'add',
    component: ApplicationFormComponent,
    title: 'Add Application - JobTracker',
    canActivate: [authGuard],
  },
  {
    path: 'edit/:id',
    component: ApplicationFormComponent,
    title: 'Edit Application - JobTracker',
    canActivate: [authGuard],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
