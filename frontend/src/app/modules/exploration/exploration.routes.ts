import { Routes } from '@angular/router';
import { CreateTour } from './pages/create-tour/create-tour';
import { MyTours } from './pages/my-tours/my-tours';
import { TourList } from './pages/tour-list/tour-list';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'create', component: CreateTour },
];
