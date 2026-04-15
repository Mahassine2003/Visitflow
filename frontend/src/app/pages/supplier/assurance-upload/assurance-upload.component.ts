import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InsuranceUploadComponent } from '../../../shared/components/insurance-upload/insurance-upload.component';

@Component({
  selector: 'app-assurance-upload',
  standalone: true,
  imports: [CommonModule, InsuranceUploadComponent],
  templateUrl: './assurance-upload.component.html',
  styleUrl: './assurance-upload.component.scss',
})
export class AssuranceUploadComponent {}
