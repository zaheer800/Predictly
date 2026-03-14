import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api';
import { Tournament, Match } from '../../core/models';

@Component({
  selector: 'app-tournament-detail',
  imports: [RouterLink],
  templateUrl: './tournament-detail.html',
  styles: ``,
})
export class TournamentDetail implements OnInit {
  tournament: Tournament | null = null;
  matches: Match[] = [];
  loading = true;

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.getTournament(id).subscribe(t => {
      this.tournament = t;
      this.api.getMatchesByTournament(id).subscribe(m => {
        this.matches = m;
        this.loading = false;
      });
    });
  }
}
