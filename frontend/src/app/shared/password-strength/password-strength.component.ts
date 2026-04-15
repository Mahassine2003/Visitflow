import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';

/** 0 = empty/weak … 4 = strong */
export function computePasswordStrength(password: string): number {
  if (!password) return 0;
  let score = 0;
  if (password.length >= 6) score++;
  if (password.length >= 10) score++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
  if (/\d/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;
  return Math.min(4, score);
}

@Component({
  selector: 'app-password-strength',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './password-strength.component.html',
  styleUrl: './password-strength.component.scss',
})
export class PasswordStrengthComponent {
  password = input<string>('');

  readonly strength = computed(() => computePasswordStrength(this.password()));

  readonly label = computed(() => {
    const s = this.strength();
    if (s <= 0) return 'Too short';
    if (s === 1) return 'Weak';
    if (s === 2) return 'Fair';
    if (s === 3) return 'Good';
    return 'Strong';
  });
}
