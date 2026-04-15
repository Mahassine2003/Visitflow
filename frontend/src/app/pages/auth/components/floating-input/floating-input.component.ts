import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';

@Component({
  selector: 'app-floating-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './floating-input.component.html',
  styleUrl: './floating-input.component.scss',
})
export class FloatingInputComponent {
  @Input({ required: true }) control!: FormControl;
  @Input({ required: true }) label!: string;
  @Input() type: string = 'text';
  @Input() icon = 'pi pi-user';
  @Input() autocomplete = '';
  @Input() inputId = '';
  /** When true and type is password, toggles visibility */
  @Input() showToggle = false;

  showPassword = false;

  get effectiveType(): string {
    if (this.showToggle && this.type === 'password') {
      return this.showPassword ? 'text' : 'password';
    }
    return this.type;
  }

  get showValidIcon(): boolean {
    if (this.type === 'password') return false;
    return (
      this.control.valid &&
      this.control.touched &&
      !!this.control.value
    );
  }
}
