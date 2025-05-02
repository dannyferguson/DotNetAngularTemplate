import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {SubmitButtonComponent} from '../../components/buttons/submit-button/submit-button.component';
import {NgClass} from '@angular/common';
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
  private router = inject(Router);
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
        next: (_) => {
          this.successMessage.set('Success! Redirecting..')
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 2000);
        },
        error: err => {
          this.submitting.set(false);
          this.submitButton.loading.set(false);

          if (err.status === 401) {
            this.errorMessage.set('Invalid credentials. Please try again.');
            return;
          }

          this.errorMessage.set('A server error has occured. Please try again later.');
        }
      });
    }
  }
}
