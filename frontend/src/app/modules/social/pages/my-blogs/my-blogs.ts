import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { Blogs } from '../../services/blogs';

@Component({
  imports: [RouterLink],
  selector: 'app-my-blogs',
  styleUrl: './my-blogs.scss',
  templateUrl: './my-blogs.html',
})
export class MyBlogs {
  protected readonly blogs = inject(Blogs);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(blogId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogs.publish(blogId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
