import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ToastState {
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _state = new BehaviorSubject<ToastState | null>(null);
  readonly state$ = this._state.asObservable();

  private dismissTimer: ReturnType<typeof setTimeout> | null = null;

  show(message: string, type: 'success' | 'error' | 'info' = 'success', durationMs = 3000): void {
    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
      this.dismissTimer = null;
    }
    this._state.next({ message, type });
    this.dismissTimer = setTimeout(() => {
      this._state.next(null);
      this.dismissTimer = null;
    }, durationMs);
  }

  dismiss(): void {
    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
      this.dismissTimer = null;
    }
    this._state.next(null);
  }
}
