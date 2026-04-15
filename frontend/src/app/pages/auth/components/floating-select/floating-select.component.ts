import { Component, ElementRef, HostListener, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';

export interface FloatingSelectOption {
  value: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-floating-select',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './floating-select.component.html',
  styleUrl: './floating-select.component.scss',
})
export class FloatingSelectComponent {
  @Input({ required: true }) control!: FormControl;
  @Input({ required: true }) label!: string;
  @Input() inputId = '';
  @Input() options: FloatingSelectOption[] = [];

  open = false;

  constructor(private host: ElementRef<HTMLElement>) {}

  get selected(): FloatingSelectOption | undefined {
    return this.options.find((o) => o.value === this.control.value);
  }

  toggle(): void {
    this.open = !this.open;
  }

  select(option: FloatingSelectOption): void {
    this.control.setValue(option.value);
    this.control.markAsTouched();
    this.open = false;
  }

  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.open = false;
    }
  }

  get showValidIcon(): boolean {
    return this.control.valid && this.control.touched && !!this.control.value;
  }
}
