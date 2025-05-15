import {Component, inject, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-confirm-email-page',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './confirm-email-page.component.html'
})
export class ConfirmEmailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private alertBannerService = inject(SnackBarService);

  ngOnInit(): void {
    this.alertBannerService.fire('info', 'Confirming your email. Please wait..');

    const code = this.route.snapshot.queryParamMap.get('code');
    if (!code) {
      this.alertBannerService.fire('error', 'Invalid link.');
      return;
    }
    this.authService.confirmEmail(code).subscribe({
      next: result => {
        if (!result.isSuccess) {
          this.alertBannerService.fire('error', result.errorMessage);
          return;
        }

        this.alertBannerService.fire('success', result.successMessage, 2500);
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    });
  }
}
