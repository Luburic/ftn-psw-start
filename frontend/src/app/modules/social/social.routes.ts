import { Routes } from '@angular/router';
import { BlogDetail } from './pages/blog-detail/blog-detail';
import { BlogList } from './pages/blog-list/blog-list';
import { CreateBlog } from './pages/create-blog/create-blog';
import { MyBlogs } from './pages/my-blogs/my-blogs';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: ':id', component: BlogDetail },
];
