import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogReading {
  private readonly http = inject(HttpClient);

  async addComment(id: string, text: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/comments`, { text }));
  }

  async updateComment(id: string, commentId: string, text: string): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/comments/${commentId}`, { text }));
  }

  async deleteComment(id: string, commentId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/comments/${commentId}`));
  }
}
