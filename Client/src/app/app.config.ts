import {ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection} from '@angular/core';
import {provideRouter} from '@angular/router';

import {routes} from './app.routes';
import {AuthService} from './features/auth/auth.service';
import {HTTP_INTERCEPTORS, provideHttpClient} from '@angular/common/http';
import {CsrfInterceptor} from './interceptors/csrf.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({eventCoalescing: true}),
    provideRouter(routes),
    provideHttpClient(),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return authService.checkAuth();
    }),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CsrfInterceptor,
      multi: true
    }
  ],
};
