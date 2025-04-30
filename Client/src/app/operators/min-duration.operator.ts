import { MonoTypeOperatorFunction, throwError, timer } from 'rxjs';
import { catchError, delayWhen, switchMap } from 'rxjs/operators';

/*
      Custom RxJs operator that allows you to specify a minimum duration of an operation. Both on success and error.
      This is useful for UX in cases like form submissions being too fast
 */
export function minDuration<T>(ms: number): MonoTypeOperatorFunction<T> {
  const start = Date.now();

  return source$ => source$.pipe(
    catchError(err =>
      timer(Math.max(0, ms - (Date.now() - start))).pipe(
        switchMap(() => throwError(() => err))
      )
    ),
    delayWhen(() => timer(Math.max(0, ms - (Date.now() - start))))
  );
}
