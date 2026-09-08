import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BlogDto } from '../../api/social-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();
}
