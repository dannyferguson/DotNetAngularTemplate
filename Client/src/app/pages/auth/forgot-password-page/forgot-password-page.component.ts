import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../../services/auth.service';
import {SubmitButtonComponent} from '../../../components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../../components/form-elements/text-input/text-input.component';
import {AlertBannerComponent} from '../../../components/alert-banner/alert-banner.component';

@Component({
  selector: 'app-forgot-password-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent,
    AlertBannerComponent
  ],
  templateUrl: './forgot-password-page.component.html'
})
export class ForgotPasswordPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);
  protected errorMessage = signal<string | undefined>(undefined);
  protected successMessage = signal<string | undefined>(undefined);

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
      this.errorMessage.set(undefined);
      this.submitButton.loading.set(true);

      const { email } = this.forgotPasswordForm.getRawValue();
      if (!email) {
        this.submitting.set(false);
        return;
      }

      this.authService.forgotPassword(email).subscribe({
        next: (response) => {
          this.reset();

          if (!response.success) {
            this.errorMessage.set(response.message);
            return;
          }

          this.forgotPasswordForm.reset();
          this.successMessage.set(response.message);
        }
      });
    }
  }

  private reset(): void {
    this.submitting.set(false);
    this.submitButton.loading.set(false);
    this.successMessage.set(undefined);
    this.errorMessage.set(undefined);
  }
}
