# BettingPoolApp — Bonus Question Catalog

> **10 predefined cricket-specific bonus question types** seeded into `bonus_question_catalog`.  
> Scoring rules apply as follows:
> - **percentage_based** (numeric): `floor(max(0, max_points × (1 − |predicted − actual| / actual)))`  
>   — Special case: if `actual = 0` and `predicted = 0` → full points; otherwise → 0  
> - **exact_match** (multiple choice): full points for exact match, 0 otherwise

---

## Catalog Table

| id | question_key | question_template | answer_type | options | max_points | scoring_rule |
|----|---|---|---|---|---|---|
| 1 | total_sixes | How many sixes will be hit in total during this match? | numeric | — | 10 | percentage_based |
| 2 | total_fours | How many fours will be hit in total during this match? | numeric | — | 10 | percentage_based |
| 3 | total_match_runs | What will be the total combined runs scored by both teams? | numeric | — | 10 | percentage_based |
| 4 | winning_margin_runs | By how many runs will the winning team win (if batting second loses)? | numeric | — | 10 | percentage_based |
| 5 | winning_margin_wickets | By how many wickets will the winning team win (if chasing)? | numeric | — | 10 | percentage_based |
| 6 | top_scorer_team | Which team will score the most runs? | multiple_choice | `["Home", "Away"]` | 5 | exact_match |
| 7 | toss_winner_bats_or_bowls | What will the toss-winning team choose to do? | multiple_choice | `["Bat", "Bowl"]` | 5 | exact_match |
| 8 | match_goes_to_super_over | Will this match go to a Super Over? | multiple_choice | `["Yes", "No"]` | 5 | exact_match |
| 9 | total_wickets_fallen | How many wickets will fall across both innings in total? | numeric | — | 10 | percentage_based |
| 10 | highest_individual_score | What will be the highest individual score by any batter in the match? | numeric | — | 10 | percentage_based |

---

## Detailed Definitions

### 1. `total_sixes`
| Field | Value |
|---|---|
| **question_key** | `total_sixes` |
| **question_template** | How many sixes will be hit in total during this match? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Counts sixes from both innings combined |

---

### 2. `total_fours`
| Field | Value |
|---|---|
| **question_key** | `total_fours` |
| **question_template** | How many fours will be hit in total during this match? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Counts fours from both innings combined |

---

### 3. `total_match_runs`
| Field | Value |
|---|---|
| **question_key** | `total_match_runs` |
| **question_template** | What will be the total combined runs scored by both teams? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Sum of both teams' final scores including extras |

---

### 4. `winning_margin_runs`
| Field | Value |
|---|---|
| **question_key** | `winning_margin_runs` |
| **question_template** | By how many runs will the winning team win (applicable when defending team wins)? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Only applies when the team batting first wins. Admin enters 0 if the chasing team wins; predicted = 0 then earns full points |

---

### 5. `winning_margin_wickets`
| Field | Value |
|---|---|
| **question_key** | `winning_margin_wickets` |
| **question_template** | By how many wickets will the winning team win (applicable when chasing team wins)? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Only applies when the chasing team wins. Admin enters 0 if defending team wins; predicted = 0 then earns full points |

---

### 6. `top_scorer_team`
| Field | Value |
|---|---|
| **question_key** | `top_scorer_team` |
| **question_template** | Which team will score the most runs? |
| **answer_type** | `multiple_choice` |
| **options** | `["Home", "Away"]` |
| **max_points** | 5 |
| **scoring_rule** | `exact_match` |
| **notes** | In case of a tie, admin picks the team whose total was posted first |

---

### 7. `toss_winner_bats_or_bowls`
| Field | Value |
|---|---|
| **question_key** | `toss_winner_bats_or_bowls` |
| **question_template** | What will the toss-winning team choose to do? |
| **answer_type** | `multiple_choice` |
| **options** | `["Bat", "Bowl"]` |
| **max_points** | 5 |
| **scoring_rule** | `exact_match` |
| **notes** | Result known at match start; locked before predictions close |

---

### 8. `match_goes_to_super_over`
| Field | Value |
|---|---|
| **question_key** | `match_goes_to_super_over` |
| **question_template** | Will this match go to a Super Over? |
| **answer_type** | `multiple_choice` |
| **options** | `["Yes", "No"]` |
| **max_points** | 5 |
| **scoring_rule** | `exact_match` |
| **notes** | Rare but high-excitement question; 5 points keeps it low-risk |

---

### 9. `total_wickets_fallen`
| Field | Value |
|---|---|
| **question_key** | `total_wickets_fallen` |
| **question_template** | How many wickets will fall across both innings in total? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Maximum possible is 20. Retired hurt counts as fallen |

---

### 10. `highest_individual_score`
| Field | Value |
|---|---|
| **question_key** | `highest_individual_score` |
| **question_template** | What will be the highest individual score by any batter in the match? |
| **answer_type** | `numeric` |
| **options** | `null` |
| **max_points** | 10 |
| **scoring_rule** | `percentage_based` |
| **notes** | Includes not-out scores. If tied, use the highest value from either innings |

---

## SQL Seed Script

```sql
INSERT INTO bonus_question_catalog
    (question_key, question_template, answer_type, options, max_points, scoring_rule, is_active)
VALUES
    ('total_sixes',
     'How many sixes will be hit in total during this match?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('total_fours',
     'How many fours will be hit in total during this match?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('total_match_runs',
     'What will be the total combined runs scored by both teams?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('winning_margin_runs',
     'By how many runs will the winning team win (applicable when defending team wins)?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('winning_margin_wickets',
     'By how many wickets will the winning team win (applicable when chasing team wins)?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('top_scorer_team',
     'Which team will score the most runs?',
     'multiple_choice', '["Home","Away"]'::jsonb, 5, 'exact_match', TRUE),

    ('toss_winner_bats_or_bowls',
     'What will the toss-winning team choose to do?',
     'multiple_choice', '["Bat","Bowl"]'::jsonb, 5, 'exact_match', TRUE),

    ('match_goes_to_super_over',
     'Will this match go to a Super Over?',
     'multiple_choice', '["Yes","No"]'::jsonb, 5, 'exact_match', TRUE),

    ('total_wickets_fallen',
     'How many wickets will fall across both innings in total?',
     'numeric', NULL, 10, 'percentage_based', TRUE),

    ('highest_individual_score',
     'What will be the highest individual score by any batter in the match?',
     'numeric', NULL, 10, 'percentage_based', TRUE);
```

---

## Points Summary

| Type | max_points | Scoring |
|---|---|---|
| Numeric questions (×7) | 10 each | Percentage-based partial credit |
| Multiple choice questions (×3) | 5 each | All-or-nothing |
| **Max possible per match** | **85** | (10 winner + up to 3 bonus questions drawn from above) |

> A single match draws **3 random bonus questions** from the tournament's configured subset, so the actual max per match = 10 (winner) + sum of max_points of the 3 selected questions.

---

*End of Bonus Question Catalog*
