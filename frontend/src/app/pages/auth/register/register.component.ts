import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormControl,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AppRole, AuthService } from '../../../core/auth/auth.service';
import { FloatingInputComponent } from '../components/floating-input/floating-input.component';
import {
  FloatingSelectComponent,
  FloatingSelectOption,
} from '../components/floating-select/floating-select.component';
import {
  authFieldsEnter,
  authFormShake,
  authSuccessPop,
} from '../auth-page-animations';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    FloatingInputComponent,
    FloatingSelectComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  animations: [authFieldsEnter, authFormShake, authSuccessPop],
})
export class RegisterComponent {
  loading = false;
  error: string | null = null;
  shakeState: 'idle' | 'shake' = 'idle';
  showSuccess = false;

  roleOptions: FloatingSelectOption[] = [
    { value: 'ADMIN', label: 'Admin', icon: 'pi pi-star' },
    { value: 'HSE', label: 'HSE', icon: 'pi pi-shield' },
    { value: 'RH', label: 'RH', icon: 'pi pi-users' },
    { value: 'USER', label: 'User', icon: 'pi pi-user' },
  ];

  form = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
    role: ['USER', [Validators.required]],
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {}

  get nameCtrl(): FormControl {
    return this.form.get('name') as FormControl;
  }

  get emailCtrl(): FormControl {
    return this.form.get('email') as FormControl;
  }

  get passwordCtrl(): FormControl {
    return this.form.get('password') as FormControl;
  }

  get confirmCtrl(): FormControl {
    return this.form.get('confirmPassword') as FormControl;
  }

  get roleCtrl(): FormControl {
    return this.form.get('role') as FormControl;
  }

  get name() {
    return this.form.get('name');
  }

  get email() {
    return this.form.get('email');
  }

  get password() {
    return this.form.get('password');
  }

  get confirmPassword() {
    return this.form.get('confirmPassword');
  }

  get role() {
    return this.form.get('role');
  }

  /** 0–100 strength estimate for UI bar only */
  get passwordStrength(): number {
    const v = (this.password?.value as string) ?? '';
    let score = 0;
    if (v.length >= 6) score += 25;
    if (v.length >= 10) score += 15;
    if (/[A-Z]/.test(v)) score += 15;
    if (/[a-z]/.test(v)) score += 15;
    if (/[0-9]/.test(v)) score += 15;
    if (/[^A-Za-z0-9]/.test(v)) score += 15;
    return Math.min(100, score);
  }

  get strengthLabel(): string {
    const s = this.passwordStrength;
    if (s < 35) return 'Weak';
    if (s < 70) return 'Fair';
    return 'Strong';
  }

  get strengthClass(): string {
    const s = this.passwordStrength;
    if (s < 35) return 'is-weak';
    if (s < 70) return 'is-fair';
    return 'is-strong';
  }

  get confirmMatch(): boolean {
    return (
      this.password?.value === this.confirmPassword?.value &&
      !!this.confirmPassword?.value
    );
  }

  private triggerShake(): void {
    this.shakeState = 'shake';
    window.setTimeout(() => {
      this.shakeState = 'idle';
    }, 550);
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      this.triggerShake();
      return;
    }

    if (this.password?.value !== this.confirmPassword?.value) {
      this.error = 'Passwords do not match.';
      this.triggerShake();
      return;
    }

    this.loading = true;
    this.error = null;
    this.showSuccess = false;

    const { name, email, password, role } = this.form.value;

    this.authService
      .register({
        name: name!,
        email: email!,
        password: password!,
        role: (role as AppRole) ?? 'USER',
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.showSuccess = true;
          window.setTimeout(() => {
            this.router.navigate(['/login']);
          }, 800);
        },
        error: (err) => {
          this.error =
            err?.error?.message ??
            'An error occurred while creating the account.';
          this.loading = false;
          this.triggerShake();
        },
      });
  }
}
