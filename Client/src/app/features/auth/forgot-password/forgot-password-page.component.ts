import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SubmitButtonComponent} from '../../../shared/components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../../shared/components/form-elements/text-input/text-input.component';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-forgot-password-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent
  ],
  templateUrl: './forgot-password-page.component.html'
})
export class ForgotPasswordPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);
  private alertBannerService = inject(SnackBarService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);

  forgotPasswordForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email])
  });

  onSubmit() {
    if (this.submitting()) {
      return;
    }

    this.forgotPasswordForm.markAllAsTouched();

    if (this.forgotPasswordForm.valid) {
      this.submitting.set(true);
      this.submitButton.loading.set(true);

      const { email } = this.forgotPasswordForm.getRawValue();
      if (!email) {
        this.submitting.set(false);
        return;
      }

      this.authService.forgotPassword(email).subscribe({
        next: (result) => {
          this.reset();

          if (!result.isSuccess) {
            this.alertBannerService.fire('error', result.errorMessage);
            return;
          }

          this.forgotPasswordForm.reset();
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
