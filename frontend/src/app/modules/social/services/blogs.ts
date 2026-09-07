import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PageResult } from '../../../shared/api/page-result';
import { BlogDto, CreateBlogDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class Blogs {
  private readonly http = inject(HttpClient);

  private readonly selectedId = signal<string | null>(null);

  readonly published = httpResource<PageResult<BlogDto>>(
    () => `${BASE_URL}/published?page=1&pageSize=20`,
  );

  readonly mine = httpResource<BlogDto[]>(() => `${BASE_URL}/mine`);

  readonly detail = httpResource<BlogDto>(() => {
    const id = this.selectedId();
    return id === null ? undefined : `${BASE_URL}/${id}`;
  });

  load(id: string): void {
    this.selectedId.set(id);
  }

  async create(dto: CreateBlogDto): Promise<BlogDto> {
    const blog = await firstValueFrom(this.http.post<BlogDto>(BASE_URL, dto));
    this.mine.reload();
    return blog;
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
    this.mine.reload();
    this.published.reload();
  }

  async addComment(id: string, text: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/comments`, { text }));
    this.detail.reload();
  }

  async updateComment(id: string, commentId: string, text: string): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/comments/${commentId}`, { text }));
    this.detail.reload();
  }

  async deleteComment(id: string, commentId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/comments/${commentId}`));
    this.detail.reload();
  }
}
