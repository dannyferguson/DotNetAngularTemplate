import { HttpClient } from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  combineLatest,
  combineLatestWith,
  delayWhen,
  map,
  of,
  retry,
  throwError,
  timer
} from 'rxjs';
import {Router} from '@angular/router';
import {minDuration} from '../operators/min-duration.operator';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  public login(email: string, password: string): void {
    const start = Date.now();

    this.http.post<any>('/api/v1/auth/login', {email: email, password: password}).pipe(
      retry(2),
      minDuration(2000)
    ).subscribe({
      next: (_) => {
        this.router.navigate(['/']);
      },
      error: err => {
        console.log(err);
      }
    });
  }
}
