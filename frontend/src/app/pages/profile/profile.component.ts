import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { AuthService } from '../../core/auth/auth.service';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../shared/toast/toast.service';
import { PasswordStrengthComponent } from '../../shared/password-strength/password-strength.component';

interface ProfileDto {
  id: number;
  fullName: string;
  email: string;
  role: string;
  avatarUrl?: string | null;
  createdAt?: string | null;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PasswordStrengthComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  animations: [
    trigger('profilePageEnter', [
      transition(':enter', [
        query(
          '.profile-id, .profile-panel',
          [
            style({ opacity: 0, transform: 'translateY(16px)' }),
            stagger(100, [
              animate(
                '420ms cubic-bezier(0.22, 1, 0.36, 1)',
                style({ opacity: 1, transform: 'translateY(0)' })
              ),
            ]),
          ],
          { optional: true }
        ),
      ]),
    ]),
    trigger('profileHeroEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('380ms 40ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
    trigger('tabEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(8px)' }),
        animate('260ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
    trigger('editActionsEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(6px)' }),
        animate('220ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
    trigger('photoPreviewEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.96)' }),
        animate('280ms cubic-bezier(0.22, 1, 0.36, 1)', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})
export class ProfileComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  profile = signal<ProfileDto | null>(null);
  loadError = signal('');
  profileLoading = signal(false);
  avatarSaving = signal(false);
  savingPersonal = signal(false);
  savingPassword = signal(false);
  deletingAccount = signal(false);

  isEditingPersonal = signal(false);
  activeTab = signal(0);
  dragOver = signal(false);
  deleteShake = signal(false);

  avatarFile = signal<File | null>(null);
  /** Preview from file picker / drop (data URL) */
  photoPreviewUrl = signal<string | null>(null);
  /** Cached server avatar URL for display when no new preview */
  private serverAvatarUrl = signal<string | null>(null);

  deleteConfirmControl = new FormControl('');

  personalForm = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
  });

  passwordForm = this.fb.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    },
    { validators: [this.passwordMatchValidator.bind(this)] }
  );

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const g = group as FormGroup;
    const n = g.get('newPassword')?.value;
    const c = g.get('confirmPassword')?.value;
    if (n == null || c == null || n === '' || c === '') return null;
    return n === c ? null : { passwordMismatch: true };
  }

  showPassword = signal({
    current: false,
    new: false,
    confirm: false,
  });

  readonly initials = computed(() => {
    const name = this.profile()?.fullName || this.profile()?.email || '';
    return name
      .split(' ')
      .filter((p) => p)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase())
      .join('');
  });

  readonly displayAvatarSrc = computed(() => {
    const preview = this.photoPreviewUrl();
    if (preview) return preview;
    const url = this.serverAvatarUrl();
    return url ? this.resolveAssetUrl(url) : null;
  });

  /** Photo enregistrée sur le serveur (URL absolue) — affichage dans l’onglet Profile Photo */
  readonly savedAvatarTabSrc = computed(() => {
    const u = this.serverAvatarUrl();
    return u ? this.resolveAssetUrl(u) : null;
  });

  readonly roleBadgeClass = computed(() => {
    const r = (this.profile()?.role || '').toUpperCase();
    if (r === 'ADMIN') return 'profile-id__badge--admin';
    if (r === 'USER') return 'profile-id__badge--user';
    return 'profile-id__badge--guard';
  });

  readonly memberSinceLabel = computed(() => {
    const raw = this.profile()?.createdAt;
    if (!raw) return '—';
    try {
      const d = new Date(raw);
      if (Number.isNaN(d.getTime())) return '—';
      return d.toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' });
    } catch {
      return '—';
    }
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  setTab(index: number): void {
    this.activeTab.set(index);
  }

  /** Clic sur l’avatar à gauche : ouvre l’onglet Profile Photo (zone drag & drop), pas le sélecteur de fichiers */
  openProfilePhotoTab(): void {
    this.setTab(1);
    queueMicrotask(() => {
      document.getElementById('profile-photo-panel')?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    });
  }

  /** Clic sur la zone (hors bouton) : parcourir les fichiers si pas de prévisualisation locale en cours */
  onProfilePhotoZoneClick(event: MouseEvent, input: HTMLInputElement): void {
    if ((event.target as HTMLElement).closest('button')) {
      return;
    }
    if (this.photoPreviewUrl()) {
      return;
    }
    input.click();
  }

  onProfilePhotoZoneKeydown(event: KeyboardEvent, input: HTMLInputElement): void {
    if (event.key !== 'Enter' && event.key !== ' ') {
      return;
    }
    event.preventDefault();
    if (this.photoPreviewUrl()) {
      return;
    }
    input.click();
  }

  toggleEditPersonal(): void {
    this.isEditingPersonal.update((v) => !v);
    if (!this.isEditingPersonal()) {
      this.patchPersonalFromProfile();
    }
  }

  cancelEditPersonal(): void {
    this.isEditingPersonal.set(false);
    this.patchPersonalFromProfile();
  }

  savePersonal(): void {
    const p = this.profile();
    if (!p || this.personalForm.invalid) return;
    this.savingPersonal.set(true);
    const body = {
      fullName: this.personalForm.value.fullName,
      email: this.personalForm.value.email,
    };
    /* POST /profile : même contrat que PUT ; évite 405 si proxy / hôte ancien sans PUT */
    this.api
      .post<{ id: number; fullName: string; email: string; role: string; createdAt?: string }>(
        `/api/users/${p.id}/profile`,
        body
      )
      .subscribe({
        next: (res) => {
          this.profile.update((cur) =>
            cur
              ? {
                  ...cur,
                  fullName: res.fullName ?? body.fullName ?? '',
                  email: res.email ?? body.email ?? '',
                  role: res.role ?? cur.role,
                  createdAt: res.createdAt ?? cur.createdAt,
                }
              : cur
          );
          this.toast.show('Profile updated successfully.', 'success');
          this.isEditingPersonal.set(false);
          this.savingPersonal.set(false);
        },
        error: (err: unknown) => {
          this.toast.show(this.httpErrorMessage(err, 'Could not save profile.'), 'error');
          this.savingPersonal.set(false);
        },
      });
  }

  onAvatarFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    this.applySelectedFile(file);
  }

  onChangePhotoClick(event: Event, input: HTMLInputElement): void {
    event.stopPropagation();
    input.click();
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
    const file = event.dataTransfer?.files?.[0] ?? null;
    this.applySelectedFile(file);
  }

  removePhotoSelection(): void {
    this.avatarFile.set(null);
    this.photoPreviewUrl.set(null);
  }

  saveAvatar(): void {
    const file = this.avatarFile();
    const p = this.profile();
    if (!file || !p) return;

    const formData = new FormData();
    // Nom de fichier explicite — aide la liaison [FromForm] IFormFile côté ASP.NET
    formData.append('file', file, file.name);

    this.avatarSaving.set(true);
    this.api.post<{ avatarUrl: string }>(`/api/users/${p.id}/avatar`, formData).subscribe({
      next: (res) => {
        const url = res.avatarUrl;
        this.serverAvatarUrl.set(url);
        this.profile.update((cur) => (cur ? { ...cur, avatarUrl: url } : cur));
        this.avatarFile.set(null);
        this.photoPreviewUrl.set(null);
        this.toast.show('Profile photo saved.', 'success');
        this.avatarSaving.set(false);
      },
      error: (err: unknown) => {
        const msg = this.httpErrorMessage(err, 'Could not upload photo.');
        this.toast.show(msg, 'error');
        this.avatarSaving.set(false);
      },
    });
  }

  togglePwField(which: 'current' | 'new' | 'confirm'): void {
    this.showPassword.update((s) => ({ ...s, [which]: !s[which] }));
  }

  submitPassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    const p = this.profile();
    if (!p) return;
    const v = this.passwordForm.getRawValue();
    this.savingPassword.set(true);
    this.api
      .post(`/api/users/${p.id}/change-password`, {
        currentPassword: v.currentPassword,
        newPassword: v.newPassword,
      })
      .subscribe({
        next: () => {
          this.toast.show('Password updated successfully.', 'success');
          this.passwordForm.reset();
          this.savingPassword.set(false);
        },
        error: (err: unknown) => {
          this.toast.show(
            this.httpErrorMessage(err, 'Could not change password. Check your current password or try again later.'),
            'error'
          );
          this.savingPassword.set(false);
        },
      });
  }

  deleteAccount(): void {
    if (this.deleteConfirmControl.value !== 'DELETE') {
      this.triggerDeleteShake();
      return;
    }
    const p = this.profile();
    if (!p) return;
    this.deletingAccount.set(true);
    this.api.delete(`/api/users/${p.id}`).subscribe({
      next: () => {
        this.toast.show('Account deleted.', 'success');
        this.deletingAccount.set(false);
        this.auth.logout();
      },
      error: () => {
        this.toast.show('Account deletion is not available or failed.', 'error');
        this.deletingAccount.set(false);
      },
    });
  }

  private triggerDeleteShake(): void {
    this.deleteShake.set(true);
    setTimeout(() => this.deleteShake.set(false), 500);
  }

  private loadProfile(): void {
    const current = this.auth.user();
    if (!current) return;
    const id = current.id;
    this.profileLoading.set(true);
    this.loadError.set('');
    this.api.get<ProfileDto>(`/api/users/${id}`).subscribe({
      next: (data) => {
        this.profile.set(data);
        this.serverAvatarUrl.set(data.avatarUrl ?? null);
        this.patchPersonalFromProfile();
        this.profileLoading.set(false);
      },
      error: () => {
        this.loadError.set('Unable to load profile.');
        this.profileLoading.set(false);
      },
    });
  }

  private patchPersonalFromProfile(): void {
    const p = this.profile();
    if (!p) return;
    this.personalForm.patchValue({
      fullName: p.fullName,
      email: p.email,
    });
  }

  private applySelectedFile(file: File | null): void {
    if (!file) return;
    const okTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!okTypes.includes(file.type)) {
      this.toast.show('Please choose a JPG, PNG, or WEBP image.', 'error');
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.toast.show('Image must be 2 MB or smaller.', 'error');
      return;
    }
    this.avatarFile.set(file);
    const reader = new FileReader();
    reader.onload = () => {
      this.photoPreviewUrl.set(reader.result as string);
    };
    reader.readAsDataURL(file);
  }

  private resolveAssetUrl(url: string): string {
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    if (url.startsWith('/')) {
      return `${typeof window !== 'undefined' ? window.location.origin : ''}${url}`;
    }
    return url;
  }

  private httpErrorMessage(err: unknown, fallback = 'Request failed.'): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error;
      if (typeof body === 'string' && body.trim()) {
        return body.length > 120 ? `${body.slice(0, 120)}…` : body;
      }
      if (body && typeof body === 'object' && 'message' in body) {
        const m = (body as { message?: string }).message;
        if (m) return m;
      }
      if (err.status === 0) {
        return 'Network error — is the API running?';
      }
      if (err.status === 401) {
        return 'Session expired — please sign in again.';
      }
      if (err.status === 403) {
        return 'You are not allowed to update this profile.';
      }
      if (err.status === 409) {
        return 'This email is already in use.';
      }
      if (err.status === 413) {
        return 'File is too large for the server.';
      }
      return err.message || `Error (${err.status}).`;
    }
    return fallback;
  }
}
