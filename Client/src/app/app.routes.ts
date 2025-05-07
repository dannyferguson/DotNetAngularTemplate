import {Routes} from '@angular/router';
import {IndexPageComponent} from './pages/general/index-page/index-page.component';
import {LoginPageComponent} from './pages/auth/login-page/login-page.component';
import {RegisterPageComponent} from './pages/auth/register-page/register-page.component';
import {LogoutPageComponent} from './pages/auth/logout-page/logout-page.component';
import {ForgotPasswordPageComponent} from './pages/auth/forgot-password-page/forgot-password-page.component';
import {
  ForgotPasswordConfirmationPage
} from './pages/auth/forgot-password-confirmation-page/forgot-password-confirmation-page.component';

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
  },
  {
    path: 'logout',
    component: LogoutPageComponent
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordPageComponent
  },
  {
    path: 'forgot-password-confirmation',
    component: ForgotPasswordConfirmationPage
  }
];
