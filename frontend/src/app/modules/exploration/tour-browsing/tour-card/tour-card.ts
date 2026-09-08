import { Component, input } from '@angular/core';
import { TourDto } from '../../api/exploration-api-types';

@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
}
