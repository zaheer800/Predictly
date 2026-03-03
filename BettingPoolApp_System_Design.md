# BettingPoolApp --- System Design Document

## Version

v1.0\
Public Deployment Architecture\
Azure-Based Implementation

------------------------------------------------------------------------

# 1. Product Overview

## 1.1 Purpose

BettingPoolApp is a public, points-based cricket prediction platform
designed for family, friends, and colleagues (\~100 initial users).

The application:

-   Allows users to predict match winners
-   Includes predefined bonus questions
-   Calculates scores deterministically
-   Maintains tournament and global leaderboards
-   Does not include any payment or gambling functionality

------------------------------------------------------------------------

## 1.2 Scope

### In Scope

-   User authentication
-   Cricket tournaments
-   Match predictions
-   3 random bonus questions per match
-   Percentage-based bonus scoring
-   Tournament leaderboard
-   Global leaderboard
-   Admin Excel match upload
-   Azure-based public deployment

### Out of Scope

-   Real-money betting
-   Wallets or payments
-   Player-based predictions
-   Live APIs
-   Fantasy sports scoring
-   Mid-tournament bonus changes

------------------------------------------------------------------------

# 2. Business Rules

## 2.1 Prediction Rules

-   Users can modify predictions until match start time
-   Lock occurs at `match_start_time` (UTC)
-   Lock enforced at database level
-   `finalized_at` updated on each edit
-   Predictions visible to others only after match completion

------------------------------------------------------------------------

## 2.2 Bonus System

-   10 predefined bonus question types (system-level catalog)
-   Each tournament configures allowed bonus types
-   Each match randomly selects 3 bonus questions
-   Bonus answers are optional
-   No configuration changes allowed after tournament activation

------------------------------------------------------------------------

## 2.3 Scoring Rules

### Winner Points

-   Fixed constant (e.g., 10)
-   Exact match required

### Numeric Bonus Formula

    percentage_difference = abs(predicted - actual) / actual
    raw_score = max_points * (1 - percentage_difference)
    score = max(0, raw_score)
    final_score = floor(score)

Special Case: - If actual == 0: - predicted == 0 → full points -
otherwise → 0

### Multiple Choice Bonus

-   Exact match → full points
-   Otherwise → 0

------------------------------------------------------------------------

## 2.4 Ranking Model (Model C)

Ranking order:

1.  Total points (descending)
2.  Earliest average `finalized_at` (ascending)
3.  Bonus participation count (descending)
4.  User ID (ascending fallback)

------------------------------------------------------------------------

# 3. System Architecture

## 3.1 Infrastructure

Angular PWA\
↓\
Azure Static Web Apps\
↓\
.NET 8 Web API (Single Instance)\
↓\
Azure Database for PostgreSQL (Flexible Server)\
↓\
Azure Blob Storage (Excel uploads)

------------------------------------------------------------------------

## 3.2 Backend Architecture

Layered Architecture:

Controllers\
Services (Business Logic)\
Domain\
Persistence (EF Core)\
Raw SQL (Leaderboard & Aggregation)

Hybrid persistence approach: - EF Core for CRUD - Raw SQL for
leaderboard queries and heavy aggregations

------------------------------------------------------------------------

# 4. Database Design (PostgreSQL)

Core tables:

1.  users\
2.  tournaments\
3.  matches\
4.  bonus_question_catalog\
5.  tournament_bonus_config\
6.  match_bonus_selection\
7.  predictions\
8.  prediction_bonus_answers\
9.  match_bonus_results\
10. prediction_scores\
11. tournament_leaderboard\
12. global_leaderboard

------------------------------------------------------------------------

# 5. API Design (REST v1)

Base Route:

`/api/v1`

Modules:

Auth\
Users\
Tournaments\
Matches\
Predictions\
Admin\
Leaderboard

------------------------------------------------------------------------

# 6. Scoring Engine Design

Execution Flow:

1.  Admin submits match result\
2.  Begin DB transaction\
3.  Validate match not already completed\
4.  Validate winner and bonus results\
5.  Load all predictions\
6.  Compute winner + bonus scores\
7.  Insert into prediction_scores\
8.  Mark match completed\
9.  Commit transaction\
10. Rebuild leaderboards

------------------------------------------------------------------------

# 7. Leaderboard Architecture

Precomputed strategy.\
Full rebuild after each completed match.

Stored values:

-   total_points\
-   average_finalized_at\
-   bonus_participation_count\
-   rank

Ranking logic:

    rank() over (
        order by
            total_points desc,
            avg_finalized_at asc,
            bonus_participation_count desc,
            user_id asc
    )

------------------------------------------------------------------------

# 8. Lock Enforcement

-   Lock enforced at database level
-   Conditional update with match_start_time check
-   Database time (now()) is authoritative
-   Rows affected validation prevents race condition

------------------------------------------------------------------------

# 9. Security Model

-   JWT authentication
-   Role-based authorization
-   Admin route protection
-   No client-side trust
-   UTC timestamps everywhere

------------------------------------------------------------------------

# 10. Deployment Model

-   Azure Static Web Apps (Frontend)
-   Azure App Service (.NET API)
-   Azure PostgreSQL Flexible Server
-   Azure Blob Storage
-   Automated backups enabled

------------------------------------------------------------------------

# 11. Design Philosophy

-   Deterministic over clever
-   Precomputed over real-time aggregation
-   Database integrity over frontend logic
-   Simplicity over premature optimization
-   Governance over flexibility

------------------------------------------------------------------------

End of Document
