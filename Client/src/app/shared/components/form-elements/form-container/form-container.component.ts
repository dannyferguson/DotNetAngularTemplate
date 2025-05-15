import {Component, input} from '@angular/core';

@Component({
  selector: 'app-form-container',
  imports: [],
  templateUrl: './form-container.component.html',
  styleUrl: './form-container.component.css'
})
export class FormContainerComponent {
  title = input.required<string>();
}
