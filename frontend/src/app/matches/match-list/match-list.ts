import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api';
import { Match } from '../../core/models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-match-list',
  imports: [RouterLink, DatePipe],
  templateUrl: './match-list.html',
  styles: ``,
})
export class MatchList implements OnInit {
  matches: Match[] = [];
  loading = true;

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const tournamentId = this.route.snapshot.paramMap.get('tournamentId')!;
    this.api.getMatchesByTournament(tournamentId).subscribe({
      next: m => { this.matches = m; this.loading = false; },
      error: () => this.loading = false,
    });
  }
}
