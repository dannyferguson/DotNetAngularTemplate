import {Routes} from '@angular/router';
import {IndexPageComponent} from './pages/index-page/index-page.component';
import {LoginPageComponent} from './pages/login-page/login-page.component';

export const routes: Routes = [
  {
    path: '',
    component: IndexPageComponent
  },
  {
    path: 'login',
    component: LoginPageComponent
  }
];
