import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SubmitButtonComponent} from '../../../shared/components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../../shared/components/form-elements/text-input/text-input.component';
import {AlertBannerComponent} from '../../../shared/components/alert-banner/alert-banner.component';
import {FormContainerComponent} from '../../../shared/components/form-elements/form-container/form-container.component';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent,
    AlertBannerComponent,
    RouterLink,
    FormContainerComponent
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css'
})
export class LoginPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);
  protected errorMessage = signal<string | undefined>(undefined);
  protected successMessage = signal<string | undefined>(undefined);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  })

  onSubmit() {
    if (this.submitting()) {
      return;
    }

    this.loginForm.markAllAsTouched();

    if (this.loginForm.valid) {
      this.submitting.set(true);
      this.errorMessage.set(undefined);
      this.submitButton.loading.set(true);

      const { email, password } = this.loginForm.getRawValue();
      if (!email || !password) {
        this.submitting.set(false);
        return;
      }

      this.authService.login(email, password).subscribe({
        next: (result) => {
          this.reset();

          if (!result.isSuccess) {
            this.errorMessage.set(result.errorMessage);
            return;
          }

          this.loginForm.reset();
          this.successMessage.set(result.successMessage);
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 2000);
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
