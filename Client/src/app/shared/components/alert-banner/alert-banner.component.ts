import {Component, input} from '@angular/core';

@Component({
  selector: 'app-alert-banner',
  imports: [],
  templateUrl: './alert-banner.component.html'
})
export class AlertBannerComponent {
  error = input<string | undefined>();
  warning = input<string | undefined>();
  success = input<string | undefined>();
  info = input<string | undefined>();
}
