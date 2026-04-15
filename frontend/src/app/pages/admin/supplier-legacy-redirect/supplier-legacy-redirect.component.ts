import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

/** Redirige les anciennes routes /suppliers/new et /suppliers/:id vers la page unique /suppliers (query params). */
@Component({
  selector: 'app-supplier-legacy-redirect',
  standalone: true,
  template: '',
})
export class SupplierLegacyRedirectComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    const mode = this.route.snapshot.data['mode'] as 'create' | 'detail' | undefined;
    if (mode === 'create') {
      this.router.navigate(['/app/suppliers'], { queryParams: { new: '1' }, replaceUrl: true });
      return;
    }
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.router.navigate(['/app/suppliers'], { queryParams: { id }, replaceUrl: true });
    } else {
      this.router.navigate(['/app/suppliers'], { replaceUrl: true });
    }
  }
}
