import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { BlogCard } from '../../components/blog-card/blog-card';
import { Blogs } from '../../services/blogs';

@Component({
  imports: [BlogCard, RouterLink],
  selector: 'app-blog-list',
  styleUrl: './blog-list.scss',
  templateUrl: './blog-list.html',
})
export class BlogList {
  protected readonly blogs = inject(Blogs);
  protected readonly auth = inject(Auth);
}
