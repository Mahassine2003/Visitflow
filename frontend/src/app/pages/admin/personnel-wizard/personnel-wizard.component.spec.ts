import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { PersonnelWizardComponent } from './personnel-wizard.component';

describe('PersonnelWizardComponent', () => {
  let component: PersonnelWizardComponent;
  let fixture: ComponentFixture<PersonnelWizardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonnelWizardComponent, HttpClientTestingModule, RouterTestingModule]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PersonnelWizardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
