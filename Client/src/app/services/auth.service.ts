import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, catchError, Observable, of, retry} from 'rxjs';
import {AuthResponse} from "../models/responses/auth-response.model";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private DEFAULT_ERROR_MESSAGE = 'Something went wrong. Please try again.';

  public login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/login', {email: email, password: password}).pipe(
      retry(2),
      catchError((error: HttpErrorResponse) => {
        const message = error.error?.message || this.DEFAULT_ERROR_MESSAGE;

        return of({
          success: false,
          message
        } as AuthResponse);
      })
    );
  }

  public register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/register', {email: email, password: password}).pipe(
      retry(2),
      catchError((error: HttpErrorResponse) => {
        const message = error.error?.message || this.DEFAULT_ERROR_MESSAGE;

        return of({
          success: false,
          message
        } as AuthResponse);
      })
    );
  }
}
