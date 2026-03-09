# BettingPoolApp — PostgreSQL Column Schemas

## Table of Contents
1. [users](#1-users)
2. [tournaments](#2-tournaments)
3. [matches](#3-matches)
4. [bonus_question_catalog](#4-bonus_question_catalog)
5. [tournament_bonus_config](#5-tournament_bonus_config)
6. [match_bonus_selection](#6-match_bonus_selection)
7. [predictions](#7-predictions)
8. [prediction_bonus_answers](#8-prediction_bonus_answers)
9. [match_bonus_results](#9-match_bonus_results)
10. [prediction_scores](#10-prediction_scores)
11. [tournament_leaderboard](#11-tournament_leaderboard)
12. [global_leaderboard](#12-global_leaderboard)

---

## 1. users

```sql
CREATE TABLE users (
    id              SERIAL PRIMARY KEY,
    username        VARCHAR(100)        NOT NULL UNIQUE,
    email           VARCHAR(255)        NOT NULL UNIQUE,
    password_hash   TEXT                NOT NULL,
    display_name    VARCHAR(150)        NOT NULL,
    role            VARCHAR(20)         NOT NULL DEFAULT 'user' CHECK (role IN ('user', 'admin')),
    is_active       BOOLEAN             NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ         NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ         NOT NULL DEFAULT now()
);

CREATE INDEX idx_users_email    ON users (email);
CREATE INDEX idx_users_username ON users (username);
```

---

## 2. tournaments

```sql
CREATE TABLE tournaments (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(255)        NOT NULL,
    description     TEXT,
    status          VARCHAR(20)         NOT NULL DEFAULT 'draft'
                        CHECK (status IN ('draft', 'active', 'completed')),
    start_date      TIMESTAMPTZ         NOT NULL,
    end_date        TIMESTAMPTZ         NOT NULL,
    created_by      INTEGER             NOT NULL REFERENCES users (id),
    created_at      TIMESTAMPTZ         NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ         NOT NULL DEFAULT now(),

    CONSTRAINT chk_tournament_dates CHECK (end_date > start_date)
);

CREATE INDEX idx_tournaments_status ON tournaments (status);
```

---

## 3. matches

```sql
CREATE TABLE matches (
    id                  SERIAL PRIMARY KEY,
    tournament_id       INTEGER             NOT NULL REFERENCES tournaments (id),
    team_home           VARCHAR(100)        NOT NULL,
    team_away           VARCHAR(100)        NOT NULL,
    venue               VARCHAR(200),
    match_start_time    TIMESTAMPTZ         NOT NULL,
    status              VARCHAR(20)         NOT NULL DEFAULT 'scheduled'
                            CHECK (status IN ('scheduled', 'completed', 'cancelled')),
    winner              VARCHAR(100),               -- NULL until result submitted
    created_by          INTEGER             NOT NULL REFERENCES users (id),
    created_at          TIMESTAMPTZ         NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ         NOT NULL DEFAULT now(),

    CONSTRAINT chk_teams_differ CHECK (team_home <> team_away)
);

CREATE INDEX idx_matches_tournament_id      ON matches (tournament_id);
CREATE INDEX idx_matches_match_start_time   ON matches (match_start_time);
CREATE INDEX idx_matches_status             ON matches (status);
```

---

## 4. bonus_question_catalog

```sql
CREATE TABLE bonus_question_catalog (
    id                  SERIAL PRIMARY KEY,
    question_key        VARCHAR(100)        NOT NULL UNIQUE,
    question_template   TEXT                NOT NULL,
    answer_type         VARCHAR(20)         NOT NULL
                            CHECK (answer_type IN ('numeric', 'multiple_choice')),
    options             JSONB,                          -- NULL for numeric types
    max_points          INTEGER             NOT NULL CHECK (max_points > 0),
    scoring_rule        VARCHAR(30)         NOT NULL
                            CHECK (scoring_rule IN ('percentage_based', 'exact_match')),
    is_active           BOOLEAN             NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ         NOT NULL DEFAULT now()
);
```

---

## 5. tournament_bonus_config

```sql
CREATE TABLE tournament_bonus_config (
    id                      SERIAL PRIMARY KEY,
    tournament_id           INTEGER     NOT NULL REFERENCES tournaments (id),
    bonus_question_id       INTEGER     NOT NULL REFERENCES bonus_question_catalog (id),
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_tournament_bonus UNIQUE (tournament_id, bonus_question_id)
);

CREATE INDEX idx_tbc_tournament_id ON tournament_bonus_config (tournament_id);
```

> **Rule:** No rows may be inserted or deleted once the tournament status is `active`.

---

## 6. match_bonus_selection

```sql
CREATE TABLE match_bonus_selection (
    id                      SERIAL PRIMARY KEY,
    match_id                INTEGER     NOT NULL REFERENCES matches (id),
    bonus_question_id       INTEGER     NOT NULL REFERENCES bonus_question_catalog (id),
    display_order           SMALLINT    NOT NULL CHECK (display_order BETWEEN 1 AND 3),
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_match_bonus         UNIQUE (match_id, bonus_question_id),
    CONSTRAINT uq_match_display_order UNIQUE (match_id, display_order)
);

CREATE INDEX idx_mbs_match_id ON match_bonus_selection (match_id);
```

> **Rule:** Exactly 3 rows per `match_id` (enforced at application layer; `display_order` unique constraint provides partial enforcement).

---

## 7. predictions

```sql
CREATE TABLE predictions (
    id              SERIAL PRIMARY KEY,
    user_id         INTEGER         NOT NULL REFERENCES users (id),
    match_id        INTEGER         NOT NULL REFERENCES matches (id),
    predicted_winner VARCHAR(100)   NOT NULL,
    finalized_at    TIMESTAMPTZ     NOT NULL DEFAULT now(),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),

    CONSTRAINT uq_user_match_prediction UNIQUE (user_id, match_id)
);

CREATE INDEX idx_predictions_user_id  ON predictions (user_id);
CREATE INDEX idx_predictions_match_id ON predictions (match_id);
```

> **Lock Enforcement (DB-level conditional update):**
> ```sql
> UPDATE predictions
> SET    predicted_winner = $1,
>        finalized_at     = now()
> WHERE  id       = $2
>   AND  user_id  = $3
>   AND  (SELECT match_start_time FROM matches WHERE id = match_id) > now();
> -- rows_affected == 0 → lock has triggered; reject the request
> ```

---

## 8. prediction_bonus_answers

```sql
CREATE TABLE prediction_bonus_answers (
    id                      SERIAL PRIMARY KEY,
    prediction_id           INTEGER         NOT NULL REFERENCES predictions (id),
    match_bonus_selection_id INTEGER        NOT NULL REFERENCES match_bonus_selection (id),
    answer_numeric          NUMERIC(12, 2),            -- populated for numeric type
    answer_choice           VARCHAR(100),              -- populated for multiple_choice type
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at              TIMESTAMPTZ     NOT NULL DEFAULT now(),

    CONSTRAINT uq_prediction_bonus_answer UNIQUE (prediction_id, match_bonus_selection_id),
    CONSTRAINT chk_answer_not_both_null
        CHECK (answer_numeric IS NOT NULL OR answer_choice IS NOT NULL)
);

CREATE INDEX idx_pba_prediction_id ON prediction_bonus_answers (prediction_id);
```

---

## 9. match_bonus_results

```sql
CREATE TABLE match_bonus_results (
    id                          SERIAL PRIMARY KEY,
    match_id                    INTEGER         NOT NULL REFERENCES matches (id),
    match_bonus_selection_id    INTEGER         NOT NULL REFERENCES match_bonus_selection (id),
    actual_numeric              NUMERIC(12, 2),
    actual_choice               VARCHAR(100),
    submitted_by                INTEGER         NOT NULL REFERENCES users (id),
    submitted_at                TIMESTAMPTZ     NOT NULL DEFAULT now(),

    CONSTRAINT uq_match_bonus_result UNIQUE (match_id, match_bonus_selection_id),
    CONSTRAINT chk_result_not_both_null
        CHECK (actual_numeric IS NOT NULL OR actual_choice IS NOT NULL)
);

CREATE INDEX idx_mbr_match_id ON match_bonus_results (match_id);
```

---

## 10. prediction_scores

```sql
CREATE TABLE prediction_scores (
    id                  SERIAL PRIMARY KEY,
    prediction_id       INTEGER     NOT NULL REFERENCES predictions (id) UNIQUE,
    user_id             INTEGER     NOT NULL REFERENCES users (id),
    match_id            INTEGER     NOT NULL REFERENCES matches (id),
    tournament_id       INTEGER     NOT NULL REFERENCES tournaments (id),
    winner_score        INTEGER     NOT NULL DEFAULT 0 CHECK (winner_score >= 0),
    bonus_score_1       INTEGER     NOT NULL DEFAULT 0 CHECK (bonus_score_1 >= 0),
    bonus_score_2       INTEGER     NOT NULL DEFAULT 0 CHECK (bonus_score_2 >= 0),
    bonus_score_3       INTEGER     NOT NULL DEFAULT 0 CHECK (bonus_score_3 >= 0),
    total_score         INTEGER     GENERATED ALWAYS AS
                            (winner_score + bonus_score_1 + bonus_score_2 + bonus_score_3) STORED,
    bonus_answered_count SMALLINT   NOT NULL DEFAULT 0 CHECK (bonus_answered_count BETWEEN 0 AND 3),
    scored_at           TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_ps_user_id       ON prediction_scores (user_id);
CREATE INDEX idx_ps_match_id      ON prediction_scores (match_id);
CREATE INDEX idx_ps_tournament_id ON prediction_scores (tournament_id);
```

---

## 11. tournament_leaderboard

```sql
CREATE TABLE tournament_leaderboard (
    id                          SERIAL PRIMARY KEY,
    tournament_id               INTEGER         NOT NULL REFERENCES tournaments (id),
    user_id                     INTEGER         NOT NULL REFERENCES users (id),
    total_points                INTEGER         NOT NULL DEFAULT 0,
    avg_finalized_at            TIMESTAMPTZ     NOT NULL,
    bonus_participation_count   INTEGER         NOT NULL DEFAULT 0,
    rank                        INTEGER         NOT NULL,
    last_updated_at             TIMESTAMPTZ     NOT NULL DEFAULT now(),

    CONSTRAINT uq_tournament_leaderboard UNIQUE (tournament_id, user_id)
);

CREATE INDEX idx_tl_tournament_id ON tournament_leaderboard (tournament_id);
CREATE INDEX idx_tl_rank          ON tournament_leaderboard (tournament_id, rank);
```

> **Rebuild SQL (executed after each match completion):**
> ```sql
> DELETE FROM tournament_leaderboard WHERE tournament_id = $1;
>
> INSERT INTO tournament_leaderboard
>     (tournament_id, user_id, total_points, avg_finalized_at,
>      bonus_participation_count, rank, last_updated_at)
> SELECT
>     ps.tournament_id,
>     ps.user_id,
>     SUM(ps.total_score)                             AS total_points,
>     AVG(p.finalized_at)                             AS avg_finalized_at,
>     SUM(ps.bonus_answered_count)                    AS bonus_participation_count,
>     RANK() OVER (
>         ORDER BY
>             SUM(ps.total_score)          DESC,
>             AVG(p.finalized_at)          ASC,
>             SUM(ps.bonus_answered_count) DESC,
>             ps.user_id                   ASC
>     )                                               AS rank,
>     now()
> FROM prediction_scores ps
> JOIN predictions p ON p.id = ps.prediction_id
> WHERE ps.tournament_id = $1
> GROUP BY ps.tournament_id, ps.user_id;
> ```

---

## 12. global_leaderboard

```sql
CREATE TABLE global_leaderboard (
    id                          SERIAL PRIMARY KEY,
    user_id                     INTEGER         NOT NULL REFERENCES users (id) UNIQUE,
    total_points                INTEGER         NOT NULL DEFAULT 0,
    avg_finalized_at            TIMESTAMPTZ     NOT NULL,
    bonus_participation_count   INTEGER         NOT NULL DEFAULT 0,
    rank                        INTEGER         NOT NULL,
    last_updated_at             TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE INDEX idx_gl_rank ON global_leaderboard (rank);
```

> **Rebuild SQL (executed after each match completion):**
> ```sql
> TRUNCATE global_leaderboard;
>
> INSERT INTO global_leaderboard
>     (user_id, total_points, avg_finalized_at,
>      bonus_participation_count, rank, last_updated_at)
> SELECT
>     ps.user_id,
>     SUM(ps.total_score)                             AS total_points,
>     AVG(p.finalized_at)                             AS avg_finalized_at,
>     SUM(ps.bonus_answered_count)                    AS bonus_participation_count,
>     RANK() OVER (
>         ORDER BY
>             SUM(ps.total_score)          DESC,
>             AVG(p.finalized_at)          ASC,
>             SUM(ps.bonus_answered_count) DESC,
>             ps.user_id                   ASC
>     )                                               AS rank,
>     now()
> FROM prediction_scores ps
> JOIN predictions p ON p.id = ps.prediction_id
> GROUP BY ps.user_id;
> ```

---

*End of Database Schema Document*
