import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api';
import { Match, BonusQuestion, Prediction } from '../../core/models';
import { PredictForm } from '../../predictions/predict-form/predict-form';

@Component({
  selector: 'app-match-detail',
  imports: [RouterLink, PredictForm],
  templateUrl: './match-detail.html',
  styles: ``,
})
export class MatchDetail implements OnInit {
  match: Match | null = null;
  bonusQuestions: BonusQuestion[] = [];
  predictions: Prediction[] = [];
  myPrediction: Prediction | null = null;
  loading = true;
  isLocked = false;

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.getMatch(id).subscribe(m => {
      this.match = m;
      this.isLocked = new Date(m.matchStartTime) <= new Date();

      this.api.getMatchBonusQuestions(id).subscribe(q => this.bonusQuestions = q);
      this.api.getMyPrediction(id).subscribe({ next: p => this.myPrediction = p, error: () => {} });

      if (m.status === 'Completed') {
        this.api.getMatchPredictions(id).subscribe(p => this.predictions = p);
      }

      this.loading = false;
    });
  }

  onPredictionSaved(p: Prediction) {
    this.myPrediction = p;
  }
}
