import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api';
import { BonusQuestion, Tournament } from '../../core/models';

@Component({
  selector: 'app-admin-panel',
  imports: [ReactiveFormsModule],
  templateUrl: './admin-panel.html',
  styles: ``,
})
export class AdminPanel implements OnInit {
  catalog: BonusQuestion[] = [];
  tournaments: Tournament[] = [];
  selectedTournamentId = '';
  matchResultForm: FormGroup;
  submitting = false;
  message = '';

  constructor(private api: ApiService, private fb: FormBuilder) {
    this.matchResultForm = this.fb.group({
      matchId: ['', Validators.required],
      winnerTeam: ['', Validators.required],
    });
  }

  ngOnInit() {
    this.api.getBonusCatalog().subscribe(c => this.catalog = c);
    this.api.getTournaments().subscribe(t => this.tournaments = t);
  }

  submitResult() {
    if (this.matchResultForm.invalid) return;
    this.submitting = true;
    this.message = '';
    const { matchId, winnerTeam } = this.matchResultForm.value;
    this.api.completeMatch(matchId, winnerTeam, []).subscribe({
      next: () => { this.message = 'Match completed and scores calculated.'; this.submitting = false; },
      error: () => { this.message = 'Error completing match.'; this.submitting = false; },
    });
  }

  assignBonusQuestions(matchId: string) {
    this.api.assignMatchBonusQuestions(matchId).subscribe({
      next: () => alert('Bonus questions assigned.'),
      error: () => alert('Failed to assign bonus questions.'),
    });
  }
}
