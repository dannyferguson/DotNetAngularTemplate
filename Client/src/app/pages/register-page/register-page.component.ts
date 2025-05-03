import {Component, inject, signal, ViewChild} from '@angular/core';
import {RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {SubmitButtonComponent} from '../../components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../components/form-elements/text-input/text-input.component';
import {AlertBannerComponent} from '../../components/alert-banner/alert-banner.component';
import {passwordsMatchValidator} from '../../validators/passwords-match.validator';

@Component({
  selector: 'app-register-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent,
    AlertBannerComponent
  ],
  templateUrl: './register-page.component.html'
})
export class RegisterPageComponent {
  private authService = inject(AuthService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);
  protected errorMessage = signal<string | undefined>(undefined);
  protected successMessage = signal<string | undefined>(undefined);

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
      this.errorMessage.set(undefined);
      this.submitButton.loading.set(true);

      const { email, password } = this.registerForm.getRawValue();
      if (!email || !password) {
        this.submitting.set(false);
        return;
      }

      this.authService.register(email, password).subscribe({
        next: (response) => {
          this.reset();

          if (!response.success) {
            this.errorMessage.set(response.message);
          }

          this.registerForm.reset();
          this.successMessage.set(response.message);
          return;
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
