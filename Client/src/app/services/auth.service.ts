import {HttpClient} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, Observable, tap} from 'rxjs';
import {AuthResponse} from "../models/responses/auth-response.model";
import {handleAuthError} from '../operators/handle-auth-error.operator';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  public login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/login', {email: email, password: password}).pipe(
      tap(response => {
        this.isAuthenticatedSubject.next(response.success)
      }),
      handleAuthError()
    );
  }

  public register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/register', {email: email, password: password}).pipe(
      handleAuthError()
    );
  }

  public forgotPassword(email: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/forgot-password', {email: email}).pipe(
      handleAuthError()
    );
  }

  public checkAuth(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>('/api/v1/auth/me').pipe(
      tap(response => {
        this.isAuthenticatedSubject.next(response.success)
      }),
      handleAuthError()
    );
  }

  public logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/logout', {}).pipe(
      tap(response => {
        this.isAuthenticatedSubject.next(!response.success)
      }),
      handleAuthError()
    );
  }
}
