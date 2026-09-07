import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PageResult } from '../../../shared/api/page-result';
import { CreateTourDto, TourDto, TransportTimeDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class Tours {
  private readonly http = inject(HttpClient);

  readonly published = httpResource<PageResult<TourDto>>(
    () => `${BASE_URL}/published?page=1&pageSize=20`,
  );

  readonly mine = httpResource<TourDto[]>(() => `${BASE_URL}/mine`);

  async create(dto: CreateTourDto): Promise<TourDto> {
    const tour = await firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
    this.mine.reload();
    return tour;
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
    this.mine.reload();
    this.published.reload();
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
    this.mine.reload();
  }
}
