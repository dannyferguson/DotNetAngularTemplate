import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, catchError, Observable, of, tap} from 'rxjs';
import {AuthResponse} from "../models/responses/auth-response.model";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private DEFAULT_ERROR_MESSAGE = 'Something went wrong. Please try again.';
  private DEFAULT_RATE_LIMIT_MESSAGE = 'You are sending too many requests. Please try again later.';

  public login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/login', {email: email, password: password}).pipe(
      tap(response => {
        this.isAuthenticatedSubject.next(response.success)
      }),
      catchError((error: HttpErrorResponse) => {
        let message = error.error?.message || this.DEFAULT_ERROR_MESSAGE;

        if (error.status === 429) {
          message = this.DEFAULT_RATE_LIMIT_MESSAGE;
        }

        return of({
          success: false,
          message
        } as AuthResponse);
      })
    );
  }

  public register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/register', {email: email, password: password}).pipe(
      catchError((error: HttpErrorResponse) => {
        let message = error.error?.message || this.DEFAULT_ERROR_MESSAGE;

        if (error.status === 429) {
          message = this.DEFAULT_RATE_LIMIT_MESSAGE;
        }

        return of({
          success: false,
          message
        } as AuthResponse);
      })
    );
  }

  public checkAuth(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>('/api/v1/auth/me').pipe(
      tap(response => {
        this.isAuthenticatedSubject.next(response.success)
      }),
      catchError((error: HttpErrorResponse) => {
        let message = error.error?.message || this.DEFAULT_ERROR_MESSAGE;

        if (error.status === 429) {
          message = this.DEFAULT_RATE_LIMIT_MESSAGE;
        }

        return of({
          success: false,
          message
        } as AuthResponse);
      })
    );
  }
}
