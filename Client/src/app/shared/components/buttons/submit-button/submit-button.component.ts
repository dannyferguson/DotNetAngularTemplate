import {Component, input, signal} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {MatProgressSpinner} from '@angular/material/progress-spinner';

@Component({
  selector: 'app-submit-button',
  imports: [
    MatButton,
    MatProgressSpinner
  ],
  templateUrl: './submit-button.component.html',
  styleUrl: './submit-button.component.css'
})
export class SubmitButtonComponent {
  public buttonText = input.required<string>();
  public loadingText = input.required<string>();
  public loading = signal<boolean>(false);
}
