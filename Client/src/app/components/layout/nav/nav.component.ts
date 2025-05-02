import {Component, inject} from '@angular/core';
import {AsyncPipe, NgClass} from '@angular/common';
import {AuthService} from '../../../services/auth.service';
import {RouterLink, RouterLinkActive} from '@angular/router';

@Component({
  selector: 'app-nav',
    imports: [
        NgClass,
        AsyncPipe,
        RouterLink,
        RouterLinkActive
    ],
  templateUrl: './nav.component.html'
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
