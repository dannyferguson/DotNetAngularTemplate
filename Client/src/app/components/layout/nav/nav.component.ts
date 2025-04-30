import {Component, inject} from '@angular/core';
import {AsyncPipe, NgClass} from '@angular/common';
import {AuthService} from '../../../services/auth.service';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-nav',
  imports: [
    NgClass,
    AsyncPipe,
    RouterLink
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  private authService = inject(AuthService);

  public isAuthenticated$ = this.authService.isAuthenticated$;

  public accountMenuOpen = false;
  public mobileMenuOpen = false;

  onToggleAccountMenu(): void {
    this.accountMenuOpen = !this.accountMenuOpen;
  }

  onToggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }
}
