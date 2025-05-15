import {AfterViewInit, Component, inject, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../auth.service';
import {Router} from '@angular/router';
import {minDuration} from '../../../shared/operators/min-duration.operator';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './logout-page.component.html'
})
export class LogoutPageComponent implements AfterViewInit {
  private router = inject(Router);
  private authService = inject(AuthService);
  private alertBannerService = inject(SnackBarService);

  protected submitting = signal(true);

  ngAfterViewInit(): void {
    this.authService.logout().pipe(
      minDuration(2000)
    ).subscribe({
      next: (result) => {
        this.submitting.set(false);

        if (!result.isSuccess) {
          this.alertBannerService.fire('error', result.errorMessage);
          return;
        }

        this.alertBannerService.fire('success', result.successMessage, 2500);
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    })
  }
}
