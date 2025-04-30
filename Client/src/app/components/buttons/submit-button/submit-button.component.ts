import {Component, input, signal} from '@angular/core';

@Component({
  selector: 'app-submit-button',
  imports: [],
  templateUrl: './submit-button.component.html'
})
export class SubmitButtonComponent {
  public buttonText = input.required<string>();
  public loadingText = input.required<string>();
  public successText = input.required<string>();

  protected loading = signal<boolean>(false);
  protected success = signal<boolean>(false);

  public setSuccess(success: boolean): void {
    this.loading.set(false);
    this.success.set(success);
  }

  public setLoading(loading: boolean): void {
    this.loading.set(loading);
    this.success.set(false);
  }
}
