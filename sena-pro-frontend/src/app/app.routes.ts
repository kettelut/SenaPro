import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home';
import { SorteiosComponent } from './pages/sorteios/sorteios';
import { GeradorComponent } from './pages/gerador/gerador';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'sorteios', component: SorteiosComponent },
  { path: 'gerador', component: GeradorComponent },
  { path: '**', redirectTo: 'home' }
];

