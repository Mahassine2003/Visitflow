import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterTestingModule } from '@angular/router/testing';

import { SupplierLayoutComponent } from './supplier-layout.component';

describe('SupplierLayoutComponent', () => {
  let component: SupplierLayoutComponent;
  let fixture: ComponentFixture<SupplierLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        SupplierLayoutComponent,
        HttpClientTestingModule,
        NoopAnimationsModule,
        RouterTestingModule,
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SupplierLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
