import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateCommentDto, UpdateCommentDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogReading {
  private readonly http = inject(HttpClient);

  async addComment(id: string, dto: CreateCommentDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/comments`, dto));
  }

  async updateComment(id: string, commentId: string, dto: UpdateCommentDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/comments/${commentId}`, dto));
  }

  async deleteComment(id: string, commentId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/comments/${commentId}`));
  }
}
