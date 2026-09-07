import { Component, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogComments } from '../../components/blog-comments/blog-comments';
import { CommentEdit } from '../../models/comment-edit';
import { Blogs } from '../../services/blogs';

@Component({
  imports: [BlogComments, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  protected readonly blogs = inject(Blogs);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly error = signal<string | null>(null);

  constructor() {
    effect(() => this.blogs.load(this.id()));
  }

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogs.addComment(this.id(), text);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    try {
      await this.blogs.updateComment(this.id(), edit.commentId, edit.text);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogs.deleteComment(this.id(), commentId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }
}
