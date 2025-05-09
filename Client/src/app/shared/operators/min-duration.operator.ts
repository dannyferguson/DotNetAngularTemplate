import {MonoTypeOperatorFunction, of, timer} from 'rxjs';
import {delayWhen} from 'rxjs/operators';

/**
 * Ensures the observable waits at least `ms` milliseconds from subscription before emitting.
 * Useful for UX cases like showing a spinner for a minimum duration.
 *
 * @param ms Minimum duration in milliseconds from subscription to emission
 */
export function minDuration<T>(ms: number): MonoTypeOperatorFunction<T> {
  const start = Date.now();

  return delayWhen(() => {
    const elapsed = Date.now() - start;
    const remaining = ms - elapsed;
    return remaining > 0 ? timer(remaining) : of(null);
  });
}
