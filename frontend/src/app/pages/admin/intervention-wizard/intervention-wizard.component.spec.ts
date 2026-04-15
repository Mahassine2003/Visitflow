import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { InterventionWizardComponent } from './intervention-wizard.component';

describe('InterventionWizardComponent', () => {
  let component: InterventionWizardComponent;
  let fixture: ComponentFixture<InterventionWizardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InterventionWizardComponent, HttpClientTestingModule]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InterventionWizardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
