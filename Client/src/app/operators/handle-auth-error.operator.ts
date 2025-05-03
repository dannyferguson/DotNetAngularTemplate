import {MonoTypeOperatorFunction, of} from 'rxjs';
import {catchError} from 'rxjs/operators';
import {HttpErrorResponse} from '@angular/common/http';
import {AuthResponse} from '../models/responses/auth-response.model';

export function handleAuthError(
  defaultError = 'Something went wrong.',
  rateLimitMessage = 'Too many requests. Please try again later.'
): MonoTypeOperatorFunction<AuthResponse> {
  return catchError((error: HttpErrorResponse) => {
    let message = error.error?.message || defaultError;

    if (error.status === 429) {
      message = rateLimitMessage;
    }

    return of({
      success: false,
      message
    } as AuthResponse);
  });
}
