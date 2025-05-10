import {HttpClient} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, map, Observable, of, switchMap, tap} from 'rxjs';
import {handleAuthError} from './handle-auth-error.operator';
import {ApiResult} from '../../shared/models/api-result.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  public login(email: string, password: string): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/login', {email: email, password: password}).pipe(
      switchMap(result => {
        if (!result.isSuccess) {
          return of(result);
        }

        this.isAuthenticatedSubject.next(true);
        return this.http.get('/api/v1/auth/me').pipe(
          map(() => result)
        );
      }),
      handleAuthError()
    );
  }

  public register(email: string, password: string): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/register', {email: email, password: password}).pipe(
      handleAuthError()
    );
  }

  public forgotPassword(email: string): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/forgot-password', {email: email}).pipe(
      handleAuthError()
    );
  }

  public forgotPasswordConfirmation(code: string, email: string, password: string): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/forgot-password-confirmation', {code: code, email: email, password: password}).pipe(
      handleAuthError()
    )
  }

  public checkAuth(): Observable<ApiResult> {
    return this.http.get<ApiResult>('/api/v1/auth/me').pipe(
      tap(result => {
        this.isAuthenticatedSubject.next(result.isSuccess)
      }),
      handleAuthError()
    );
  }

  public logout(): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/logout', {}).pipe(
      tap(result => {
        this.isAuthenticatedSubject.next(!result.isSuccess)
      }),
      handleAuthError()
    );
  }

  public confirmEmail(code: string): Observable<ApiResult> {
    return this.http.post<ApiResult>('/api/v1/auth/confirm-email', {code: code}).pipe(
      handleAuthError()
    )
  }
}
