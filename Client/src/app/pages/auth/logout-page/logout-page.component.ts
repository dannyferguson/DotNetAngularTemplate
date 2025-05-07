import {AfterViewInit, Component, inject, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../../../services/auth.service';
import {AlertBannerComponent} from '../../../components/alert-banner/alert-banner.component';
import {minDuration} from '../../../operators/min-duration.operator';
import {Router} from '@angular/router';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    AlertBannerComponent
  ],
  templateUrl: './logout-page.component.html'
})
export class LogoutPageComponent implements AfterViewInit {
  private router = inject(Router);
  private authService = inject(AuthService);

  protected submitting = signal(true);
  protected errorMessage = signal<string | undefined>(undefined);
  protected successMessage = signal<string | undefined>(undefined);

  ngAfterViewInit(): void {
    this.authService.logout().pipe(
      minDuration(2000)
    ).subscribe({
      next: response => {
        this.submitting.set(false);

        if (!response.success) {
          this.errorMessage.set(response.message);
          return;
        }

        this.successMessage.set(response.message);
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    })
  }
}
