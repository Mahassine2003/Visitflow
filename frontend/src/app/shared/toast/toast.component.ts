import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss',
  animations: [
    trigger('toastEnter', [
      transition(':enter', [
        style({ transform: 'translate3d(120%, 0, 0)', opacity: 0 }),
        animate(
          '280ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({ transform: 'translate3d(0, 0, 0)', opacity: 1 })
        ),
      ]),
      transition(':leave', [
        animate(
          '200ms ease',
          style({ transform: 'translate3d(24px, 0, 0)', opacity: 0 })
        ),
      ]),
    ]),
  ],
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
}
