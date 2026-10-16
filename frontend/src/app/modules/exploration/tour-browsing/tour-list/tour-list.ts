import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  protected readonly tours = httpResource<PageResult<TourDto>>(
    () => '/api/exploration/tours/published?page=1&pageSize=20',
  );

  private readonly model = signal({ name: '', difficulty: '' });

  protected readonly form = form(this.model);

  protected readonly visibleTours = computed<TourDto[]>(() => {
    const all = this.tours.value()?.items ?? [];
    const name = this.model().name.toLowerCase();
    const difficulty = this.model().difficulty;
    return all.filter(
      (tour) =>
        tour.name.toLowerCase().includes(name) &&
        (difficulty === '' || tour.difficulty === difficulty),
    );
  });
}
