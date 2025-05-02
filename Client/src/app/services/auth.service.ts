import {HttpClient} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, Observable, retry} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  public login(email: string, password: string): Observable<any> {
    return this.http.post<any>('/api/v1/auth/login', {email: email, password: password}).pipe(
      retry(2)
    );
  }

  public register(email: string, password: string): Observable<any> {
    return this.http.post<any>('/api/v1/auth/register', {email: email, password: password}).pipe(
      retry(2)
    );
  }
}
