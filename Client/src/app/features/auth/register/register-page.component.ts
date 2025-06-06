import {Component, inject, signal, ViewChild} from '@angular/core';
import {RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SubmitButtonComponent} from '../../../shared/components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../../shared/components/form-elements/text-input/text-input.component';
import {passwordsMatchValidator} from '../passwords-match.validator';
import {FormContainerComponent} from '../../../shared/components/form-elements/form-container/form-container.component';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-register-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent,
    FormContainerComponent
  ],
  templateUrl: './register-page.component.html'
})
export class RegisterPageComponent {
  private authService = inject(AuthService);
  private alertBannerService = inject(SnackBarService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);

  registerForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]),
    passwordConfirmation: new FormControl('', [Validators.required])
  }, {
    validators: [passwordsMatchValidator]
  })

  onSubmit() {
    if (this.submitting()) {
      return;
    }

    this.registerForm.markAllAsTouched();

    if (this.registerForm.valid) {
      this.submitting.set(true);
      this.submitButton.loading.set(true);

      const { email, password } = this.registerForm.getRawValue();
      if (!email || !password) {
        this.submitting.set(false);
        return;
      }

      this.authService.register(email, password).subscribe({
        next: (result) => {
          this.reset();

          if (!result.isSuccess) {
            this.alertBannerService.fire('error', result.errorMessage);
            return;
          }

          this.registerForm.reset();
          this.alertBannerService.fire('success', result.successMessage);
        }
      });
    }
  }

  private reset(): void {
    this.submitting.set(false);
    this.submitButton.loading.set(false);
  }
}
