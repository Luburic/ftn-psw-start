import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../../components/tour-card/tour-card';
import { Tours } from '../../services/tours';

@Component({
  imports: [RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly tours = inject(Tours);
  protected readonly auth = inject(Auth);

  protected readonly nameFilter = signal('');
  protected readonly difficultyFilter = signal('');

  protected readonly visibleTours = computed<TourDto[]>(() => {
    const all = this.tours.published.value()?.items ?? [];
    const name = this.nameFilter().toLowerCase();
    const difficulty = this.difficultyFilter();
    return all.filter(
      (tour) =>
        tour.name.toLowerCase().includes(name) &&
        (difficulty === '' || tour.difficulty === difficulty),
    );
  });
}
