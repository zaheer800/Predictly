import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api';
import { LeaderboardEntry, Tournament } from '../../core/models';

@Component({
  selector: 'app-leaderboard-page',
  imports: [RouterLink],
  templateUrl: './leaderboard-page.html',
  styles: ``,
})
export class LeaderboardPage implements OnInit {
  global: LeaderboardEntry[] = [];
  tournament: LeaderboardEntry[] = [];
  tournaments: Tournament[] = [];
  selectedTournamentId = '';
  loading = true;

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.api.getGlobalLeaderboard().subscribe(e => { this.global = e; this.loading = false; });
    this.api.getTournaments().subscribe(t => { this.tournaments = t; });
  }

  loadTournamentLeaderboard() {
    if (!this.selectedTournamentId) return;
    this.api.getTournamentLeaderboard(this.selectedTournamentId).subscribe(e => this.tournament = e);
  }
}
