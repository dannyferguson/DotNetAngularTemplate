import {Routes} from '@angular/router';
import {IndexPageComponent} from './pages/index-page/index-page.component';
import {LoginPageComponent} from './pages/login-page/login-page.component';
import {RegisterPageComponent} from './pages/register-page/register-page.component';

export const routes: Routes = [
  {
    path: '',
    component: IndexPageComponent
  },
  {
    path: 'login',
    component: LoginPageComponent
  },
  {
    path: 'register',
    component: RegisterPageComponent
  }
];
