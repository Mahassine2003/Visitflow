import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Zone } from '../zones-list.component';

@Component({
  selector: 'app-zone-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './zone-card.component.html',
  styleUrl: './zone-card.component.scss',
})
export class ZoneCardComponent {
  @Input({ required: true }) zone!: Zone;
  @Output() onEdit = new EventEmitter<Zone>();
  @Output() onDelete = new EventEmitter<Zone>();

  hovered = false;
}
