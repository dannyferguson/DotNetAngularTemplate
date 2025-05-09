import {AbstractControl, ValidationErrors} from '@angular/forms';

export function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const passwordConfirmation = control.get('passwordConfirmation');

  if (password === null || passwordConfirmation === null) {
    return null;
  }

  if (password?.value === '' || passwordConfirmation?.value === '') {
    return null;
  }

  const matches = password?.value === passwordConfirmation?.value;

  if (!matches) {
    passwordConfirmation?.setErrors({ passwordsMismatch: true }); // Set error on passwordConfirmation control so that we can display it under it
  }

  return null;
}
