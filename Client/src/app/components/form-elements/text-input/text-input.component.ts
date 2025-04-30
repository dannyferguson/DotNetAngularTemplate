import {Component, ElementRef, forwardRef, input, ViewChild} from '@angular/core';
import {FormControl, FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule} from '@angular/forms';
import {NgClass} from '@angular/common';
import {RouterLink} from '@angular/router';

type TextInputType = 'text' | 'email' | 'password';
type AutoCompleteType = 'off' | 'tel' | 'new-password' | 'current-password' | 'one-time-code' | 'email';

@Component({
  selector: 'app-text-input',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    RouterLink
  ],
  templateUrl: './text-input.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TextInputComponent),
      multi: true,
    },
  ],
})
export class TextInputComponent {
  label = input.required<string>();
  type = input.required<TextInputType>();
  id = input.required<string>();
  autocomplete = input.required<AutoCompleteType>();
  required = input.required<boolean>();
  formControl = input.required<FormControl>();
  buttonText = input<string>();
  buttonLink = input<string>();


  @ViewChild('inputRef') inputRef!: ElementRef<HTMLInputElement>;

  value = '';
  disabled = false;

  onChange = (_: any) => {};
  onTouched = () => {};

  writeValue(obj: string): void {
    this.value = obj ?? '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.value = value;
    this.onChange(value);
  }
}
