import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home').then((m) => m.Home) },
  { path: 'players', loadComponent: () => import('./players/players').then((m) => m.Players) },
  { path: 'teams', loadComponent: () => import('./teams/teams').then((m) => m.Teams) },
];
