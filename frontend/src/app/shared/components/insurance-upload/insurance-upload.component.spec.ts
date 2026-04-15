import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { InsuranceUploadComponent } from './insurance-upload.component';

describe('InsuranceUploadComponent', () => {
  let component: InsuranceUploadComponent;
  let fixture: ComponentFixture<InsuranceUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InsuranceUploadComponent, HttpClientTestingModule]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InsuranceUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
