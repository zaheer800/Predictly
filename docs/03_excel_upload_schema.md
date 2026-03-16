# BettingPoolApp — Admin Excel Upload Schema

> Admins upload a single `.xlsx` file containing **two sheets**.  
> Sheet 1 sets up upcoming matches. Sheet 2 submits final results for completed matches.  
> Both sheets are processed independently via `POST /api/v1/admin/upload`.

---

## File Conventions

| Property | Value |
|---|---|
| **File format** | `.xlsx` (Excel 2007+) |
| **Max file size** | 5 MB |
| **Date/time format** | `YYYY-MM-DD HH:mm` (UTC, 24-hr clock) |
| **Header row** | Row 1 — must match column names exactly (case-insensitive) |
| **Data starts** | Row 2 |
| **Empty rows** | Ignored |
| **Storage** | Uploaded to Azure Blob Storage; processed server-side |

---

## Sheet 1 — `MatchSetup`

**Purpose:** Create or update upcoming matches, including which 3 bonus questions are assigned.

### Column Definitions

| # | Column Name | Data Type | Required | DB Column(s) | Validation Rules |
|---|---|---|---|---|---|
| A | `tournament_id` | Integer | ✅ Yes | `matches.tournament_id` | Must exist in `tournaments` table; status must be `active` |
| B | `team_home` | String (max 100) | ✅ Yes | `matches.team_home` | Cannot equal `team_away` |
| C | `team_away` | String (max 100) | ✅ Yes | `matches.team_away` | Cannot equal `team_home` |
| D | `venue` | String (max 200) | ❌ Optional | `matches.venue` | Free text |
| E | `match_start_time` | DateTime | ✅ Yes | `matches.match_start_time` | Format `YYYY-MM-DD HH:mm`; must be future UTC time |
| F | `bonus_q1_key` | String | ✅ Yes | `match_bonus_selection.bonus_question_id` | Must be a valid `question_key` in `bonus_question_catalog`; must be configured for this tournament |
| G | `bonus_q2_key` | String | ✅ Yes | `match_bonus_selection.bonus_question_id` | Same as above; must differ from `bonus_q1_key` |
| H | `bonus_q3_key` | String | ✅ Yes | `match_bonus_selection.bonus_question_id` | Same as above; must differ from `bonus_q1_key` and `bonus_q2_key` |

### Business Rules

- All three bonus question keys must be **distinct**
- All three bonus question keys must belong to the **tournament's configured bonus list** (`tournament_bonus_config`)
- If a match with the same `tournament_id` + `team_home` + `team_away` + `match_start_time` already exists and is `scheduled`, the row is treated as an **update**
- Matches in `completed` or `cancelled` status are **skipped with a warning**

### Example Rows

| tournament_id | team_home | team_away | venue | match_start_time | bonus_q1_key | bonus_q2_key | bonus_q3_key |
|---|---|---|---|---|---|---|---|
| 3 | India | Australia | MCG, Melbourne | 2025-03-15 09:30 | total_sixes | total_match_runs | toss_winner_bats_or_bowls |
| 3 | England | South Africa | Lord's, London | 2025-03-17 14:00 | total_wickets_fallen | highest_individual_score | match_goes_to_super_over |
| 3 | New Zealand | Pakistan | Eden Park, Auckland | 2025-03-19 08:00 | total_fours | winning_margin_runs | top_scorer_team |

---

## Sheet 2 — `MatchResults`

**Purpose:** Submit the final outcome for completed matches, including actual bonus answers.

### Column Definitions

| # | Column Name | Data Type | Required | DB Column(s) | Validation Rules |
|---|---|---|---|---|---|
| A | `match_id` | Integer | ✅ Yes | `matches.id` | Must exist; status must be `scheduled` (not already `completed`) |
| B | `winner` | String (max 100) | ✅ Yes | `matches.winner` | Must equal `team_home` or `team_away` exactly |
| C | `bonus_q1_key` | String | ✅ Yes | `match_bonus_results` (lookup) | Must match the `bonus_q1_key` assigned to this match in `match_bonus_selection` |
| D | `bonus_q1_answer` | String or Number | ✅ Yes | `match_bonus_results.actual_numeric` or `actual_choice` | Numeric if `answer_type = numeric`; must match an option if `answer_type = multiple_choice` |
| E | `bonus_q2_key` | String | ✅ Yes | `match_bonus_results` (lookup) | Must match the `bonus_q2_key` assigned to this match |
| F | `bonus_q2_answer` | String or Number | ✅ Yes | `match_bonus_results.actual_numeric` or `actual_choice` | Same type rules as above |
| G | `bonus_q3_key` | String | ✅ Yes | `match_bonus_results` (lookup) | Must match the `bonus_q3_key` assigned to this match |
| H | `bonus_q3_answer` | String or Number | ✅ Yes | `match_bonus_results.actual_numeric` or `actual_choice` | Same type rules as above |

### Business Rules

- `match_id` must exist and be in `scheduled` status — already `completed` matches are **rejected**
- `winner` must be an **exact string match** to `team_home` or `team_away` in that match row
- Bonus question keys in columns C, E, G must **exactly match** the 3 keys previously assigned to this match (order does not need to match)
- Numeric answers must be **≥ 0**; decimal values are allowed (stored as `NUMERIC(12,2)`)
- Multiple choice answers must **exactly match** one of the `options` values from `bonus_question_catalog`
- Submitting results triggers the **scoring engine** and **leaderboard rebuild**

### Example Rows

| match_id | winner | bonus_q1_key | bonus_q1_answer | bonus_q2_key | bonus_q2_answer | bonus_q3_key | bonus_q3_answer |
|---|---|---|---|---|---|---|---|
| 101 | India | total_sixes | 18 | total_match_runs | 342 | toss_winner_bats_or_bowls | Bat |
| 102 | South Africa | total_wickets_fallen | 14 | highest_individual_score | 87 | match_goes_to_super_over | No |
| 103 | New Zealand | total_fours | 31 | winning_margin_runs | 0 | top_scorer_team | Home |

---

## Field-to-Table Mapping Summary

| Excel Field | Table | Column |
|---|---|---|
| `tournament_id` | `matches` | `tournament_id` |
| `team_home` | `matches` | `team_home` |
| `team_away` | `matches` | `team_away` |
| `venue` | `matches` | `venue` |
| `match_start_time` | `matches` | `match_start_time` |
| `bonus_q1/2/3_key` | `match_bonus_selection` | `bonus_question_id` (via `question_key` lookup) + `display_order` |
| `match_id` | `matches` | `id` |
| `winner` | `matches` | `winner` |
| `bonus_q1/2/3_answer` (numeric) | `match_bonus_results` | `actual_numeric` |
| `bonus_q1/2/3_answer` (choice) | `match_bonus_results` | `actual_choice` |

---

## Upload Processing Flow

```
Admin uploads .xlsx
        │
        ▼
Parse Sheet 1 (MatchSetup)
  → Validate each row
  → INSERT/UPDATE matches
  → INSERT match_bonus_selection (3 rows per match)
        │
        ▼
Parse Sheet 2 (MatchResults)
  → Validate each row
  → INSERT match_bonus_results
  → UPDATE matches SET status = 'completed', winner = ...
  → Trigger Scoring Engine
  → Rebuild tournament_leaderboard
  → Rebuild global_leaderboard
        │
        ▼
Return upload summary response:
  {
    "matches_created": N,
    "matches_updated": N,
    "results_processed": N,
    "warnings": [...],
    "errors": [...]
  }
```

---

## Error Handling

| Scenario | Behaviour |
|---|---|
| Unknown `tournament_id` | Row rejected; error logged |
| `team_home` equals `team_away` | Row rejected |
| `match_start_time` in the past | Row rejected |
| Invalid `bonus_q_key` | Row rejected |
| Duplicate bonus keys in same row | Row rejected |
| `match_id` already completed | Row skipped with warning |
| `winner` not matching either team | Row rejected |
| Wrong `answer_type` for bonus answer | Row rejected |
| Partial sheet failure | Other valid rows still processed |

---

*End of Excel Upload Schema*
