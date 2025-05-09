import {MonoTypeOperatorFunction, of} from 'rxjs';
import {catchError} from 'rxjs/operators';
import {HttpErrorResponse} from '@angular/common/http';
import {ApiResult} from '../../shared/models/api-result.model';

export function handleAuthError(
  defaultError = 'Something went wrong.',
  rateLimitMessage = 'Too many requests. Please try again later.'
): MonoTypeOperatorFunction<ApiResult> {
  return catchError((error: HttpErrorResponse) => {
    let message = error.error?.errorMessage || defaultError;

    if (error.status === 429) {
      message = rateLimitMessage;
    }

    return of({
      isSuccess: false,
      successMessage: '',
      errorMessage: message,
    } as ApiResult);
  });
}
