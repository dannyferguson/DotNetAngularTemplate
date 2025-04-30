import {Component, inject, signal, ViewChild} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {SubmitButtonComponent} from '../../components/buttons/submit-button/submit-button.component';

@Component({
  selector: 'app-login-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent
  ],
  templateUrl: './login-page.component.html'
})
export class LoginPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  public submitting = signal(false);
  public errors = signal<string[]>([]);
  public success = signal(false);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  })

  onSubmit() {
    if (this.submitting()) {
      return;
    }
    this.submitButton.setLoading(true);
    this.submitting.set(true);
    this.errors.set([]);

    const newErrors: string[] = [];

    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.getRawValue();
      if (!email || !password) {
        if (!email) {
          newErrors.push('Email is required');
        }
        if (!password) {
          newErrors.push('Password is required');
        }
        return;
      }

      if (newErrors.length > 0) {
        this.errors.set(newErrors);
        this.submitting.set(false);
        this.submitButton.setLoading(false);
        return;
      }

      this.authService.login(email, password).subscribe({
        next: (_) => {
          this.submitButton.setSuccess(true);
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 2500);
        },
        error: err => {
          this.submitting.set(false);
          this.submitButton.setLoading(false);
          console.log(err);
        }
      });
    }
  }
}
