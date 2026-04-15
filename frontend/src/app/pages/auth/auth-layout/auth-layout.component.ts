import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { authShellRouteAnimation } from '../auth-route-animations';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.scss',
  animations: [authShellRouteAnimation],
})
export class AuthLayoutComponent {
  parallaxX = 0;
  parallaxY = 0;

  onVisualMove(event: MouseEvent): void {
    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    const cx = (event.clientX - rect.left) / rect.width - 0.5;
    const cy = (event.clientY - rect.top) / rect.height - 0.5;
    this.parallaxX = cx * 14;
    this.parallaxY = cy * 10;
  }

  onVisualLeave(): void {
    this.parallaxX = 0;
    this.parallaxY = 0;
  }

  prepareRoute(outlet: RouterOutlet): string {
    return outlet?.activatedRouteData?.['animation'] ?? 'authLogin';
  }
}
