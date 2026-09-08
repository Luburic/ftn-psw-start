import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateTourDto, TourDto, TransportTimeDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }
}
