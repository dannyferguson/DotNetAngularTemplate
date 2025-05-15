import {Component, inject} from '@angular/core';
import {AsyncPipe} from '@angular/common';
import {AuthService} from '../../../../features/auth/auth.service';
import {MatToolbar, MatToolbarRow} from '@angular/material/toolbar';
import {MatAnchor, MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-nav',
  imports: [
    AsyncPipe,
    MatToolbar,
    MatToolbarRow,
    MatIcon,
    MatIconButton,
    RouterLink,
    MatAnchor,
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
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
