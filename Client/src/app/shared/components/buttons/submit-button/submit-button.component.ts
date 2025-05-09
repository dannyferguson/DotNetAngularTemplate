import {Component, input, signal} from '@angular/core';

@Component({
  selector: 'app-submit-button',
  imports: [],
  templateUrl: './submit-button.component.html'
})
export class SubmitButtonComponent {
  public buttonText = input.required<string>();
  public loadingText = input.required<string>();
  public loading = signal<boolean>(false);
}
