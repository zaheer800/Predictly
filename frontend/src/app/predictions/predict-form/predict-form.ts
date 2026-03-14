import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api';
import { Match, BonusQuestion, Prediction } from '../../core/models';

@Component({
  selector: 'app-predict-form',
  imports: [ReactiveFormsModule],
  templateUrl: './predict-form.html',
  styles: ``,
})
export class PredictForm implements OnInit {
  @Input() match!: Match;
  @Input() bonusQuestions: BonusQuestion[] = [];
  @Input() existing: Prediction | null = null;
  @Output() saved = new EventEmitter<Prediction>();

  form!: FormGroup;
  saving = false;
  error = '';
  success = false;

  constructor(private fb: FormBuilder, private api: ApiService) {}

  ngOnInit() {
    const bonusControls: Record<string, any> = {};
    this.bonusQuestions.forEach(q => {
      const existing = this.existing?.bonusAnswers.find(a => a.bonusQuestionCatalogId === q.id);
      bonusControls[`bonus_${q.id}`] = [existing?.answerValue ?? ''];
    });

    this.form = this.fb.group({
      predictedWinner: [this.existing?.predictedWinner ?? ''],
      ...bonusControls,
    });
  }

  options(q: BonusQuestion): string[] {
    try { return JSON.parse(q.options ?? '[]'); } catch { return []; }
  }

  submit() {
    this.saving = true;
    this.error = '';
    this.success = false;

    const bonusAnswers = this.bonusQuestions
      .filter(q => this.form.value[`bonus_${q.id}`])
      .map(q => ({ bonusQuestionCatalogId: q.id, answerValue: this.form.value[`bonus_${q.id}`] }));

    this.api.upsertPrediction(this.match.id, this.form.value.predictedWinner, bonusAnswers).subscribe({
      next: p => { this.saving = false; this.success = true; this.saved.emit(p); },
      error: err => {
        this.saving = false;
        this.error = err.status === 409 ? 'Match is locked — predictions closed.' : 'Failed to save prediction.';
      },
    });
  }
}
