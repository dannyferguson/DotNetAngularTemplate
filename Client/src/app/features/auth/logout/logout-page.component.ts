import {AfterViewInit, Component, inject, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../auth.service';
import {AlertBannerComponent} from '../../../shared/components/alert-banner/alert-banner.component';
import {Router} from '@angular/router';
import {minDuration} from '../../../shared/operators/min-duration.operator';

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
      next: (result) => {
        this.submitting.set(false);

        if (!result.isSuccess) {
          this.errorMessage.set(result.errorMessage);
          return;
        }

        this.successMessage.set(result.successMessage);
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    })
  }
}
