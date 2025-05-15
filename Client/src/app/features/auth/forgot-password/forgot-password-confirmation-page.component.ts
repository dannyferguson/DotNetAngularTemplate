import {Component, inject, OnInit, signal, ViewChild} from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SubmitButtonComponent} from '../../../shared/components/buttons/submit-button/submit-button.component';
import {TextInputComponent} from '../../../shared/components/form-elements/text-input/text-input.component';
import {passwordsMatchValidator} from '../passwords-match.validator';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-forgot-password-confirmation-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    SubmitButtonComponent,
    TextInputComponent
  ],
  templateUrl: './forgot-password-confirmation-page.component.html'
})
export class ForgotPasswordConfirmationPage implements OnInit {
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private alertBannerService = inject(SnackBarService);

  @ViewChild(SubmitButtonComponent) submitButton!: SubmitButtonComponent;

  protected submitting = signal(false);

  forgotPasswordConfirmationForm = new FormGroup({
    code: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]),
    passwordConfirmation: new FormControl('', [Validators.required])
  }, {
    validators: [passwordsMatchValidator]
  });

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    if (code) {
      this.forgotPasswordConfirmationForm.controls['code'].setValue(code);
    }
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) {
      this.forgotPasswordConfirmationForm.controls['email'].setValue(email);
    }
  }

  onSubmit() {
    if (this.submitting()) {
      return;
    }

    this.forgotPasswordConfirmationForm.markAllAsTouched();

    if (this.forgotPasswordConfirmationForm.valid) {
      this.submitting.set(true);
      this.submitButton.loading.set(true);

      const { code, email, password } = this.forgotPasswordConfirmationForm.getRawValue();
      if (!code || !email || !password) {
        this.submitting.set(false);
        return;
      }

      this.authService.forgotPasswordConfirmation(code, email, password).subscribe({
        next: (result) => {
          this.reset();

          if (!result.isSuccess) {
            this.alertBannerService.fire('error', result.errorMessage);
            return;
          }

          this.forgotPasswordConfirmationForm.reset();
          this.alertBannerService.fire('success', result.successMessage, 2500);
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        }
      });
    }
  }

  private reset(): void {
    this.submitting.set(false);
    this.submitButton.loading.set(false);
  }
}
