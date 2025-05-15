import {Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../auth.service';
import {SnackBarService} from '../../../shared/components/snackbar/snack-bar.service';

@Component({
  selector: 'app-confirm-email-page',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './confirm-email-page.component.html',
  styleUrl: 'confirm-email-page.component.css'
})
export class ConfirmEmailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private alertBannerService = inject(SnackBarService);

  protected submitting = signal(true);

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    if (!code) {
      this.submitting.set(false);
      this.alertBannerService.fire('error', 'Invalid link.');
      return;
    }
    this.authService.confirmEmail(code).subscribe({
      next: result => {
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
    });
  }
}
