import {Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {AlertBannerComponent} from '../../../shared/components/alert-banner/alert-banner.component';
import {ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../auth.service';

@Component({
  selector: 'app-confirm-email-page',
  imports: [
    AlertBannerComponent,
    ReactiveFormsModule
  ],
  templateUrl: './confirm-email-page.component.html'
})
export class ConfirmEmailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  protected submitting = signal(true);
  protected errorMessage = signal<string | undefined>(undefined);
  protected successMessage = signal<string | undefined>(undefined);

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    if (!code) {
      this.submitting.set(false);
      this.errorMessage.set('Invalid link.');
      return;
    }
    this.authService.confirmEmail(code).subscribe({
      next: result => {
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
    });
  }
}
