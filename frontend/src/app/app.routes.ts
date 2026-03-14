import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./auth/register/register').then(m => m.Register) },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./tournaments/tournament-list/tournament-list').then(m => m.TournamentList) },
      { path: 'tournaments/:id', loadComponent: () => import('./tournaments/tournament-detail/tournament-detail').then(m => m.TournamentDetail) },
      { path: 'matches/:id', loadComponent: () => import('./matches/match-detail/match-detail').then(m => m.MatchDetail) },
      { path: 'leaderboard', loadComponent: () => import('./leaderboard/leaderboard-page/leaderboard-page').then(m => m.LeaderboardPage) },
      {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/admin-panel/admin-panel').then(m => m.AdminPanel),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
