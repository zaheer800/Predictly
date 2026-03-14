import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  Tournament,
  Match,
  Prediction,
  LeaderboardEntry,
  BonusQuestion,
} from '../models';
import { environment } from '../../../environments/environment';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // ── Tournaments ────────────────────────────────────────────────────────
  getTournaments() {
    return this.http.get<Tournament[]>(`${BASE}/tournaments`);
  }

  getTournament(id: string) {
    return this.http.get<Tournament>(`${BASE}/tournaments/${id}`);
  }

  createTournament(name: string) {
    return this.http.post<Tournament>(`${BASE}/tournaments`, { name });
  }

  activateTournament(id: string) {
    return this.http.post<void>(`${BASE}/tournaments/${id}/activate`, {});
  }

  // ── Matches ────────────────────────────────────────────────────────────
  getMatchesByTournament(tournamentId: string) {
    return this.http.get<Match[]>(`${BASE}/matches/tournament/${tournamentId}`);
  }

  getMatch(id: string) {
    return this.http.get<Match>(`${BASE}/matches/${id}`);
  }

  completeMatch(id: string, winnerTeam: string, bonusResults: { bonusQuestionCatalogId: number; actualValue: string }[]) {
    return this.http.post<void>(`${BASE}/matches/${id}/complete`, { winnerTeam, bonusResults });
  }

  // ── Predictions ────────────────────────────────────────────────────────
  upsertPrediction(matchId: string, predictedWinner: string, bonusAnswers: { bonusQuestionCatalogId: number; answerValue: string }[]) {
    return this.http.put<Prediction>(`${BASE}/predictions`, { matchId, predictedWinner, bonusAnswers });
  }

  getMyPrediction(matchId: string) {
    return this.http.get<Prediction>(`${BASE}/predictions/my/${matchId}`);
  }

  getMatchPredictions(matchId: string) {
    return this.http.get<Prediction[]>(`${BASE}/predictions/match/${matchId}`);
  }

  // ── Leaderboard ────────────────────────────────────────────────────────
  getTournamentLeaderboard(tournamentId: string) {
    return this.http.get<LeaderboardEntry[]>(`${BASE}/leaderboard/tournament/${tournamentId}`);
  }

  getGlobalLeaderboard() {
    return this.http.get<LeaderboardEntry[]>(`${BASE}/leaderboard/global`);
  }

  // ── Admin ──────────────────────────────────────────────────────────────
  getBonusCatalog() {
    return this.http.get<BonusQuestion[]>(`${BASE}/admin/bonus-catalog`);
  }

  getMatchBonusQuestions(matchId: string) {
    return this.http.get<BonusQuestion[]>(`${BASE}/admin/matches/${matchId}/bonus-config`);
  }

  assignMatchBonusQuestions(matchId: string) {
    return this.http.post<void>(`${BASE}/admin/matches/${matchId}/assign-bonus-questions`, {});
  }

  setTournamentBonusConfig(tournamentId: string, ids: number[]) {
    return this.http.put<void>(`${BASE}/admin/tournaments/${tournamentId}/bonus-config`, {
      bonusQuestionCatalogIds: ids,
    });
  }
}
