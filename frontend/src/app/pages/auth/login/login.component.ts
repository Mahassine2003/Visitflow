import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, FormControl } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';
import { FloatingInputComponent } from '../components/floating-input/floating-input.component';
import {
  authFieldsEnter,
  authFormShake,
  authSuccessPop,
} from '../auth-page-animations';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    FloatingInputComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  animations: [authFieldsEnter, authFormShake, authSuccessPop],
})
export class LoginComponent {
  loading = false;
  error: string | null = null;
  shakeState: 'idle' | 'shake' = 'idle';
  showSuccess = false;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  get emailCtrl(): FormControl {
    return this.form.get('email') as FormControl;
  }

  get passwordCtrl(): FormControl {
    return this.form.get('password') as FormControl;
  }

  get email() {
    return this.form.get('email');
  }

  get password() {
    return this.form.get('password');
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

    this.loading = true;
    this.error = null;
    this.showSuccess = false;

    const { email, password } = this.form.value;

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: () => {
        this.loading = false;
        this.showSuccess = true;
        const redirectUrl =
          this.route.snapshot.queryParamMap.get('redirectUrl');
        window.setTimeout(() => {
          if (redirectUrl) {
            this.router.navigateByUrl(redirectUrl);
            return;
          }
          this.router.navigateByUrl(this.authService.getDefaultRoute());
        }, 720);
      },
      error: () => {
        this.error = 'Invalid email or password.';
        this.loading = false;
        this.triggerShake();
      },
    });
  }
}
