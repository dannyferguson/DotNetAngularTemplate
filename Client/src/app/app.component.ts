import {Component} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {NavComponent} from './shared/components/layout/nav/nav.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavComponent],
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'Client';
}
