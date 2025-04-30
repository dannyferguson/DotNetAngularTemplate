import {Component, inject, signal} from '@angular/core';
import {RouterLink} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [
    RouterLink,
    ReactiveFormsModule
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css'
})
export class LoginPageComponent {
  private authService = inject(AuthService);

  public errors = signal<string[]>([]);
  public success = signal(false);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  })

  onSubmit() {
    this.errors.set([]);
    const newErrors: string[] = [];

    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.getRawValue();
      if (!email || !password) {
        return;
      }
      this.success.set(true);
      this.authService.login(email, password);
    }
  }
}
