export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  createdAt: string;
}

export interface Tournament {
  id: string;
  name: string;
  status: 'Draft' | 'Active' | 'Completed';
  createdAt: string;
}

export interface Match {
  id: string;
  tournamentId: string;
  teamA: string;
  teamB: string;
  matchStartTime: string;
  status: 'Scheduled' | 'Completed';
  winnerTeam: string | null;
}

export interface BonusQuestion {
  id: number;
  questionText: string;
  questionType: 'Numeric' | 'MultipleChoice';
  options: string | null; // JSON array string for MultipleChoice
  maxPoints: number;
}

export interface BonusAnswer {
  bonusQuestionCatalogId: number;
  answerValue: string;
}

export interface Prediction {
  id: string;
  userId: string;
  matchId: string;
  predictedWinner: string;
  bonusAnswers: BonusAnswer[];
  finalizedAt: string;
}

export interface PredictionScore {
  predictionId: string;
  matchId: string;
  userId: string;
  winnerScore: number;
  bonusScore: number;
  totalScore: number;
}

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  username: string;
  totalPoints: number;
  bonusParticipationCount: number;
  avgFinalizedAt: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  username: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}
