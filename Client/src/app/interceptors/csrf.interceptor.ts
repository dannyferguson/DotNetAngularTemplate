import {Injectable} from '@angular/core';
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Observable} from 'rxjs';

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.getCookie('XSRF-TOKEN');

    if (token && req.method !== 'GET' && req.method !== 'HEAD') {
      req = req.clone({
        setHeaders: {
          'X-XSRF-TOKEN': token
        }
      });
    }

    return next.handle(req);
  }

  private getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp(`(^|;)\\s*${name}=([^;]*)`));
    return match ? decodeURIComponent(match[2]) : null;
  }
}
