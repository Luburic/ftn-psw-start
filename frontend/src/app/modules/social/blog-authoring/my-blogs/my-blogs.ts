import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [RouterLink],
  selector: 'app-my-blogs',
  styleUrl: './my-blogs.scss',
  templateUrl: './my-blogs.html',
})
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(blogId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(blogId);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
