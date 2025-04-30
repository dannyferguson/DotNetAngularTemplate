import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {SubmitButtonComponent} from '../../components/buttons/submit-button/submit-button.component';
import {NgClass} from '@angular/common';
import {TextInputComponent} from '../../components/form-elements/text-input/text-input.component';

@Component({
  selector: 'app-login-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent
  ],
  templateUrl: './login-page.component.html'
})
export class LoginPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);
  protected errorMessage = signal<string | undefined>(undefined);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  })

  onSubmit() {
    if (this.submitting()) {
      return;
    }

    this.markAllAsTouched();

    if (this.loginForm.valid) {
      this.submitting.set(true);
      this.errorMessage.set(undefined);
      this.submitButton.setLoading(true);

      const { email, password } = this.loginForm.getRawValue();
      if (!email || !password) {
        this.submitting.set(false);
        return;
      }

      this.authService.login(email, password).subscribe({
        next: (_) => {
          this.submitButton.setSuccess(true);
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 2000);
        },
        error: err => {
          this.submitting.set(false);
          this.submitButton.setLoading(false);

          if (err.status === 401) {
            this.errorMessage.set('Invalid credentials. Please try again.');
            return;
          }

          this.errorMessage.set('A server error has occured. Please try again later.');
        }
      });
    }
  }

  private markAllAsTouched(): void {
    this.loginForm.markAllAsTouched();
  }
}
