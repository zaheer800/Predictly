import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api';
import { Tournament } from '../../core/models';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-tournament-list',
  imports: [RouterLink],
  templateUrl: './tournament-list.html',
  styles: ``,
})
export class TournamentList implements OnInit {
  tournaments: Tournament[] = [];
  loading = true;
  newName = '';
  creating = false;

  constructor(public auth: AuthService, private api: ApiService) {}

  ngOnInit() {
    this.api.getTournaments().subscribe({
      next: t => { this.tournaments = t; this.loading = false; },
      error: () => this.loading = false,
    });
  }

  create() {
    if (!this.newName.trim()) return;
    this.creating = true;
    this.api.createTournament(this.newName.trim()).subscribe({
      next: t => { this.tournaments.unshift(t); this.newName = ''; this.creating = false; },
      error: () => this.creating = false,
    });
  }

  activate(id: string) {
    this.api.activateTournament(id).subscribe(() => {
      const t = this.tournaments.find(x => x.id === id);
      if (t) t.status = 'Active';
    });
  }
}
