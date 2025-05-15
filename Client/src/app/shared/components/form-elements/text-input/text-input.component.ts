import {Component, ElementRef, Input, input, ViewChild} from '@angular/core';
import {FormControl, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatError, MatFormField, MatInput, MatLabel} from '@angular/material/input';

type TextInputType = 'text' | 'email' | 'password';
type AutoCompleteType = 'off' | 'tel' | 'new-password' | 'current-password' | 'one-time-code' | 'email';

@Component({
  selector: 'app-text-input',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatLabel,
    MatInput,
    MatFormField,
    MatError
  ],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.css',
})
export class TextInputComponent {
  label = input.required<string>();
  type = input.required<TextInputType>();
  id = input.required<string>();
  autocomplete = input.required<AutoCompleteType>();
  required = input<boolean>(false);
  readonly = input<boolean>(false);
  buttonText = input<string>();
  buttonLink = input<string>();

  @Input({ required: true }) control!: FormControl;

  @ViewChild('inputRef') inputRef!: ElementRef<HTMLInputElement>;
}
