import {Routes} from '@angular/router';
import {IndexPageComponent} from './features/index/index-page.component';
import {LoginPageComponent} from './features/auth/login/login-page.component';
import {RegisterPageComponent} from './features/auth/register/register-page.component';
import {LogoutPageComponent} from './features/auth/logout/logout-page.component';
import {ForgotPasswordPageComponent} from './features/auth/forgot-password/forgot-password-page.component';
import {
  ForgotPasswordConfirmationPage
} from './features/auth/forgot-password/forgot-password-confirmation-page.component';

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
