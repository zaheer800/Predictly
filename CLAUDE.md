# CLAUDE.md — Predictly AI Assistant Guide

> This file is the primary reference for AI assistants (Claude, Copilot, etc.) working on
> the Predictly codebase. Read this fully before making any changes.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [BMAD AI Development Framework](#2-bmad-ai-development-framework)
3. [Repository Structure](#3-repository-structure)
4. [Technology Stack](#4-technology-stack)
5. [Architecture & Design Patterns](#5-architecture--design-patterns)
6. [Database Design](#6-database-design)
7. [API Design Conventions](#7-api-design-conventions)
8. [Business Rules & Domain Logic](#8-business-rules--domain-logic)
9. [Security Model](#9-security-model)
10. [Development Workflows](#10-development-workflows)
11. [Coding Conventions](#11-coding-conventions)
12. [Testing Strategy](#12-testing-strategy)
13. [Deployment Model](#13-deployment-model)
14. [Design Philosophy](#14-design-philosophy)
15. [AI Assistant Rules](#15-ai-assistant-rules)

---

## 1. Project Overview

**Predictly** is a public, points-based cricket prediction platform designed for family,
friends, and colleagues (~100 initial users).

- Users predict match winners and answer bonus questions before match start
- Scores are calculated deterministically after each match
- Tournament and global leaderboards are precomputed (not real-time)
- No payments, wallets, or gambling — purely a fun prediction game

**Key Constraints:**
- All times are UTC
- Lock enforcement happens at the database level (not application level)
- Leaderboards are fully rebuilt after every match completion
- No configuration changes allowed once a tournament is activated

---

## 2. BMAD AI Development Framework

Predictly uses the **BMAD (Business, Model, API, Data) methodology** for AI-assisted
development. This framework structures how AI assistants engage with the codebase by
assigning specialized personas for each development phase.

### 2.1 What is BMAD?

BMAD is an agile AI development methodology where AI assistants take on defined roles
during different phases of the software development lifecycle. Each role has a specific
focus, responsibilities, and output expectations.

### 2.2 BMAD Roles & When to Use Them

#### Business Analyst (BA)
**Focus:** Requirements, user stories, acceptance criteria
**Trigger phrases:** "analyze requirements", "write user stories", "define acceptance criteria"
**Responsibilities:**
- Translate business needs into structured user stories
- Identify edge cases and business rule conflicts
- Produce Feature Requirement Documents (FRDs)
- Validate that implementations match the system design in `Predictly_System_Design.md`

#### Product Manager (PM)
**Focus:** Prioritization, scope, feature roadmap
**Trigger phrases:** "what should we build next", "scope this feature", "product roadmap"
**Responsibilities:**
- Prioritize features based on user impact and complexity
- Define MVP scope boundaries
- Resolve scope creep — refer back to `Predictly_System_Design.md` as the source of truth
- Flag anything Out of Scope (payments, live APIs, fantasy scoring)

#### Architect
**Focus:** System design, technical decisions, patterns
**Trigger phrases:** "design this", "how should we structure", "architecture decision"
**Responsibilities:**
- Enforce the layered architecture: Controllers → Services → Domain → Persistence
- Propose schema changes with full migration plans
- Ensure no raw SQL outside the Persistence/Repository layer (except leaderboard queries)
- Review EF Core vs. raw SQL usage decisions
- Validate Azure infrastructure decisions

#### Developer
**Focus:** Implementation, code generation, bug fixes
**Trigger phrases:** "implement", "write the code", "fix this bug", "add this feature"
**Responsibilities:**
- Implement features strictly within the established architecture
- Follow all coding conventions in Section 11
- Never bypass database-level lock enforcement
- Write code that is deterministic, not clever
- Reference existing patterns before introducing new ones

#### QA Engineer
**Focus:** Testing, edge cases, validation
**Trigger phrases:** "test this", "write tests", "what could go wrong"
**Responsibilities:**
- Write unit tests for scoring logic (most critical)
- Write integration tests for API endpoints
- Focus on edge cases: `actual == 0`, race conditions on lock, tie-breaking rules
- Validate that leaderboard ranking is deterministic

#### DevOps Engineer
**Focus:** CI/CD, deployment, infrastructure
**Trigger phrases:** "deploy", "CI pipeline", "Azure setup", "infrastructure"
**Responsibilities:**
- Configure GitHub Actions pipelines for .NET and Angular builds
- Manage Azure resource provisioning
- Set up environment-specific configurations (dev, staging, prod)
- Ensure secrets are in Azure Key Vault, never in code

### 2.3 BMAD Workflow for New Features

When implementing a new feature, follow this sequence:

```
1. BA Role  → Define requirements & acceptance criteria
2. PM Role  → Confirm scope, prioritize, check against system design
3. Architect Role → Design the technical approach (schema, API, service)
4. Developer Role → Implement following conventions
5. QA Role → Write and validate tests
6. DevOps Role → Deploy to appropriate environment
```

### 2.4 Invoking a BMAD Role

When starting a task, declare the role explicitly:

```
"Acting as the Architect: design the scoring engine service interface."
"Acting as the BA: write user stories for the bonus question system."
"Acting as the Developer: implement the prediction lock endpoint."
```

AI assistants must stay within the scope of the declared role and not mix concerns.

---

## 3. Repository Structure

```
Predictly/
├── CLAUDE.md                        # This file — AI assistant guide
├── README.md                        # Project summary
├── LICENSE                          # MIT License
├── Predictly_System_Design.md       # Authoritative system design document
│
├── src/                             # (To be created) Source code
│   ├── Predictly.API/               # .NET 8 Web API project
│   │   ├── Controllers/             # HTTP endpoints
│   │   ├── Middleware/              # Auth, error handling, logging
│   │   └── Program.cs               # App bootstrap
│   │
│   ├── Predictly.Application/       # Business logic layer
│   │   ├── Services/                # Application services
│   │   ├── DTOs/                    # Request/Response models
│   │   └── Interfaces/              # Service contracts
│   │
│   ├── Predictly.Domain/            # Domain models & business rules
│   │   ├── Entities/                # EF Core entities
│   │   ├── Enums/                   # Domain enumerations
│   │   └── Exceptions/              # Domain exceptions
│   │
│   ├── Predictly.Infrastructure/    # Data access layer
│   │   ├── Persistence/             # EF Core DbContext
│   │   ├── Repositories/            # Data access (EF Core + Raw SQL)
│   │   └── Migrations/              # EF Core migrations
│   │
│   └── Predictly.Tests/             # Test projects
│       ├── Unit/                    # Unit tests (scoring logic priority)
│       └── Integration/             # API integration tests
│
├── frontend/                        # (To be created) Angular PWA
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                # Auth, guards, interceptors
│   │   │   ├── features/            # Feature modules
│   │   │   │   ├── auth/
│   │   │   │   ├── tournaments/
│   │   │   │   ├── predictions/
│   │   │   │   ├── leaderboard/
│   │   │   │   └── admin/
│   │   │   └── shared/              # Reusable components, pipes, directives
│   │   ├── assets/
│   │   └── environments/
│   ├── angular.json
│   └── package.json
│
├── .github/
│   └── workflows/                   # CI/CD pipelines (to be created)
│
└── docs/                            # Additional documentation (to be created)
```

> **Note:** The `src/` and `frontend/` directories do not yet exist. When creating them,
> follow the structure above exactly.

---

## 4. Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Backend API | .NET Web API | 8.0 |
| ORM | Entity Framework Core | Latest stable |
| Database | PostgreSQL | Flexible Server (Azure) |
| Frontend | Angular PWA | Latest LTS |
| Cloud | Microsoft Azure | — |
| Auth | JWT Bearer Tokens | — |
| File Storage | Azure Blob Storage | — |
| Frontend Hosting | Azure Static Web Apps | — |
| API Hosting | Azure App Service | — |

---

## 5. Architecture & Design Patterns

### 5.1 Backend Layered Architecture

```
HTTP Request
    ↓
Controllers (Predictly.API)
    → Input validation, auth, HTTP concerns only
    ↓
Services (Predictly.Application)
    → Business logic, orchestration, transaction management
    ↓
Domain (Predictly.Domain)
    → Entities, value objects, domain rules
    ↓
Repositories (Predictly.Infrastructure)
    → EF Core for CRUD
    → Raw SQL for leaderboard & aggregation queries only
    ↓
PostgreSQL Database
```

**Rules:**
- Controllers must NOT contain business logic
- Services must NOT directly use `DbContext` — use repositories
- Raw SQL is permitted ONLY in the Infrastructure layer for leaderboard queries and
  heavy aggregations
- Domain entities must NOT reference infrastructure concerns

### 5.2 Hybrid Persistence Strategy

| Use Case | Method |
|---|---|
| Create/Read/Update/Delete | EF Core via repositories |
| Leaderboard computation | Raw SQL with `rank()` window function |
| Score aggregation | Raw SQL |
| Simple queries | EF Core LINQ |

### 5.3 Frontend Architecture

- Angular standalone components (no NgModules unless required)
- Feature-based folder structure (not type-based)
- Lazy-loaded feature routes
- HTTP interceptor for JWT attachment
- Environment-specific API base URL via `environment.ts`

---

## 6. Database Design

### 6.1 Core Tables

| # | Table | Purpose |
|---|---|---|
| 1 | `users` | Registered user accounts |
| 2 | `tournaments` | Cricket tournament definitions |
| 3 | `matches` | Match schedule and results |
| 4 | `bonus_question_catalog` | System-level bonus question types (10 types) |
| 5 | `tournament_bonus_config` | Allowed bonus types per tournament |
| 6 | `match_bonus_selection` | 3 randomly selected bonus Qs per match |
| 7 | `predictions` | User predictions (winner + finalized_at) |
| 8 | `prediction_bonus_answers` | User answers to match bonus questions |
| 9 | `match_bonus_results` | Actual results for bonus questions |
| 10 | `prediction_scores` | Computed scores per prediction |
| 11 | `tournament_leaderboard` | Precomputed tournament rankings |
| 12 | `global_leaderboard` | Precomputed global rankings |

### 6.2 Key Database Rules

- All timestamps stored as `TIMESTAMPTZ` (UTC)
- Lock enforcement uses `match_start_time` in conditional updates
- `now()` (database time) is authoritative — never application time for lock checks
- `finalized_at` updated on every prediction edit
- Leaderboards are fully rebuilt (TRUNCATE + INSERT) after each match result

### 6.3 Schema Change Protocol

1. Always create EF Core migrations — never modify the database manually in production
2. Migrations must be reversible (`Down()` method must be correct)
3. Propose schema changes to the Architect role before implementing
4. Never remove columns without a deprecation migration first

---

## 7. API Design Conventions

### 7.1 Base Route

```
/api/v1
```

### 7.2 Module Routes

| Module | Base Path | Auth Required |
|---|---|---|
| Auth | `/api/v1/auth` | No (login/register) |
| Users | `/api/v1/users` | Yes |
| Tournaments | `/api/v1/tournaments` | Yes |
| Matches | `/api/v1/matches` | Yes |
| Predictions | `/api/v1/predictions` | Yes |
| Admin | `/api/v1/admin` | Yes (Admin role) |
| Leaderboard | `/api/v1/leaderboard` | Yes |

### 7.3 Response Envelope

All API responses use a consistent envelope:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

Error response:
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "PREDICTION_LOCKED",
    "message": "Match has already started. Predictions are locked."
  }
}
```

### 7.4 HTTP Conventions

- `GET` — read-only, no side effects
- `POST` — create new resource
- `PUT` — full resource replacement
- `PATCH` — partial update (use sparingly)
- `DELETE` — soft delete preferred, hard delete only for admin cleanup
- Return `200` for successful reads/updates, `201` for creates, `204` for deletes
- Return `409 Conflict` for lock violations (prediction after match start)
- Return `422 Unprocessable Entity` for business rule violations

---

## 8. Business Rules & Domain Logic

### 8.1 Prediction Rules

- Predictions can be created or modified until `match_start_time` (UTC)
- Lock is enforced at the **database level** using conditional updates
- `finalized_at` is updated on every save/edit
- A prediction record can exist even with no bonus answers (bonus is optional)
- Predictions are hidden from other users until match is completed

### 8.2 Bonus System

- 10 predefined question types in `bonus_question_catalog` (system-level, not per tournament)
- Each tournament defines which bonus types are allowed (`tournament_bonus_config`)
- Each match randomly selects exactly 3 bonus questions from allowed types
- Bonus answers are optional — unanswered = 0 points
- Bonus configuration is frozen once a tournament is activated (no changes mid-tournament)

### 8.3 Scoring Formulas

**Winner Prediction:**
```
score = WINNER_POINTS (fixed constant, e.g., 10) if correct
score = 0 if incorrect
```

**Numeric Bonus:**
```csharp
if (actual == 0)
    return predicted == 0 ? maxPoints : 0;

double percentageDiff = Math.Abs(predicted - actual) / (double)actual;
double rawScore = maxPoints * (1 - percentageDiff);
return (int)Math.Floor(Math.Max(0, rawScore));
```

**Multiple Choice Bonus:**
```
score = maxPoints if exact match
score = 0 otherwise
```

### 8.4 Ranking Model (Model C)

Tiebreaker order (all applied in sequence):

1. `total_points` DESC
2. `avg_finalized_at` ASC (earlier predictions rank higher on tie)
3. `bonus_participation_count` DESC
4. `user_id` ASC (deterministic fallback)

### 8.5 Scoring Engine Execution Flow

```
1. Admin submits match result via POST /api/v1/admin/matches/{id}/result
2. Begin database transaction
3. Validate match is not already completed
4. Validate winner and bonus results are valid
5. Load all predictions for the match
6. Compute winner scores + bonus scores for each prediction
7. Bulk insert into prediction_scores
8. Mark match as completed
9. Commit transaction
10. Trigger full leaderboard rebuild (tournament + global)
```

**Critical:** Steps 2–9 must be atomic. If leaderboard rebuild (step 10) fails, it can be
retried independently — it is idempotent.

---

## 9. Security Model

- **Authentication:** JWT Bearer tokens
- **Authorization:** Role-based (`User`, `Admin`)
- **Admin routes** are protected at controller level with `[Authorize(Roles = "Admin")]`
- **No client-side trust:** All business rules validated server-side
- **Lock enforcement:** Database-level (`now() < match_start_time` in UPDATE WHERE clause)
- **UTC everywhere:** All timestamps stored and compared in UTC
- **Secrets management:** Connection strings and JWT secrets in Azure Key Vault (never in
  `appsettings.json` for production)
- **Excel uploads:** Validate file type, size, and content before processing

---

## 10. Development Workflows

### 10.1 Branch Strategy

```
main          → Production-ready code
develop       → Integration branch
feature/xxx   → Feature branches (merge to develop)
fix/xxx       → Bug fix branches (merge to develop)
hotfix/xxx    → Emergency production fixes (merge to main + develop)
```

### 10.2 Commit Message Format

```
<type>(<scope>): <short description>

Types: feat, fix, refactor, test, docs, chore, ci
Scope: api, frontend, db, scoring, leaderboard, auth, admin

Examples:
feat(scoring): implement numeric bonus scoring formula
fix(db): correct lock enforcement in prediction update
test(scoring): add edge case tests for actual == 0 scenario
docs(api): document prediction endpoints
```

### 10.3 Adding a New Feature — BMAD Checklist

```
[ ] BA: User stories written with acceptance criteria
[ ] PM: Scope confirmed, in-scope per system design
[ ] Architect: Technical design reviewed (schema, API, service interface)
[ ] Developer: Implementation follows layered architecture
[ ] Developer: No business logic in controllers
[ ] Developer: All times in UTC
[ ] QA: Unit tests written for business logic
[ ] QA: Integration tests written for API endpoints
[ ] DevOps: Deployment config updated if required
[ ] All: PR reviewed and approved
```

### 10.4 Running Locally (When Code Exists)

```bash
# Backend
cd src/Predictly.API
dotnet run

# Frontend
cd frontend
npm install
ng serve

# Database migrations
cd src/Predictly.Infrastructure
dotnet ef database update
```

---

## 11. Coding Conventions

### 11.1 .NET / C# Conventions

- Use `async/await` throughout — no synchronous database calls
- Prefer `record` types for DTOs and request/response models
- Use `IResult` pattern for minimal API responses (or `ActionResult<T>` for controllers)
- Global exception handling via middleware — no try/catch in controllers
- Use `CancellationToken` in all async service methods
- Constants (like `WINNER_POINTS`) go in `Predictly.Domain/Constants/`
- No magic numbers — all business constants must be named
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Use `required` keyword for mandatory properties in records/classes

### 11.2 Entity Framework Core Conventions

- Configure entities via `IEntityTypeConfiguration<T>` classes (not `OnModelCreating`)
- Use fluent API for relationships — not data annotations
- Always define explicit table names (snake_case to match PostgreSQL)
- UTC datetime handling: configure `UseTimestampTzType()` for all `DateTime` properties
- Never use `.Include()` without a specific need — avoid cartesian explosion

### 11.3 Angular / TypeScript Conventions

- Use standalone components (Angular 17+ style)
- Use signals for local component state where applicable
- `HttpClient` calls go in services only — never in components
- Use typed HTTP responses (`HttpClient.get<ApiResponse<T>>`)
- Environment URLs via `inject(environment)` pattern
- No `any` type — always define interfaces for API response shapes
- Feature state via Angular services with `signal()`

### 11.4 Database / SQL Conventions

- Table names: `snake_case`
- Column names: `snake_case`
- Primary keys: `id` (UUID or BIGSERIAL depending on table)
- Foreign keys: `referenced_table_id`
- Timestamp columns: always `TIMESTAMPTZ`
- Raw SQL queries: parameterized only — no string interpolation

---

## 12. Testing Strategy

### 12.1 Priority Order

1. **Scoring logic** — highest business risk, must be exhaustively tested
2. **Lock enforcement** — race condition risk
3. **Leaderboard ranking** — tiebreaker logic must be deterministic
4. **API endpoints** — contract testing
5. **Auth flows** — login, token expiry, role enforcement

### 12.2 Scoring Test Cases (Must Cover)

```
Numeric Bonus:
- actual > 0, predicted == actual → full points
- actual > 0, predicted close → partial points (floor applied)
- actual > 0, predicted far off → 0 (clamped)
- actual == 0, predicted == 0 → full points
- actual == 0, predicted != 0 → 0

Winner Prediction:
- correct winner → WINNER_POINTS
- incorrect winner → 0
- null/missing prediction → 0

Multiple Choice:
- exact match → full points
- wrong answer → 0
- no answer → 0
```

### 12.3 Leaderboard Test Cases (Must Cover)

```
- Single user ranking
- Equal points, different finalized_at → earlier wins
- Equal points, equal finalized_at, different bonus participation → more participation wins
- Fully equal except user_id → lower user_id wins (deterministic)
```

---

## 13. Deployment Model

| Resource | Service |
|---|---|
| Frontend | Azure Static Web Apps |
| Backend API | Azure App Service (.NET) |
| Database | Azure Database for PostgreSQL Flexible Server |
| File Storage | Azure Blob Storage |
| Secrets | Azure Key Vault |
| Backups | Automated (enabled on PostgreSQL Flexible Server) |

### 13.1 Environment Configuration

| Environment | Purpose |
|---|---|
| `local` | Developer local machine |
| `development` | Shared dev Azure environment |
| `production` | Live public deployment |

- Connection strings and secrets: Azure Key Vault in dev/prod
- `appsettings.Development.json`: local non-sensitive config only
- Never commit secrets to Git

---

## 14. Design Philosophy

These principles are **non-negotiable** and must guide every implementation decision:

| Principle | Meaning |
|---|---|
| **Deterministic over clever** | Scoring and ranking must produce the same result every time given the same inputs. No randomness post-match. |
| **Precomputed over real-time** | Leaderboards are computed once after each match. Never aggregate on-the-fly in response to user requests. |
| **Database integrity over frontend logic** | Business rules (especially lock enforcement) live in the database. Frontend cannot be trusted. |
| **Simplicity over premature optimization** | Don't add caching, queues, or complexity until there is a measured need. ~100 users. |
| **Governance over flexibility** | Bonus config is frozen post-activation. Prediction scores are immutable post-match. No ad-hoc overrides. |

---

## 15. AI Assistant Rules

These rules apply to all AI assistants (Claude, Copilot, etc.) working in this repository.

### 15.1 Always Do

- Read `Predictly_System_Design.md` before implementing any feature
- Declare your BMAD role at the start of any implementation task
- Follow the layered architecture strictly (see Section 5)
- Use UTC for all timestamps
- Use parameterized SQL — never string interpolation
- Keep scoring logic pure and testable (no side effects, no I/O)
- Rebuild leaderboards as a separate, idempotent step after scoring

### 15.2 Never Do

- Add payment, wallet, or gambling functionality (out of scope)
- Implement real-money or live sports API integrations
- Bypass database-level lock enforcement
- Put business logic in controllers
- Use application time for lock checks (use `now()` in database queries)
- Add real-time leaderboard aggregation queries (use precomputed tables)
- Commit secrets, connection strings, or API keys to the repository
- Modify existing migrations — always create new ones
- Allow tournament bonus config changes after tournament activation

### 15.3 When Uncertain

1. Re-read `Predictly_System_Design.md` — it is the source of truth
2. Apply the relevant BMAD role to the question (BA for requirements, Architect for design)
3. Default to the more conservative, simpler approach
4. Ask the developer/owner before introducing new patterns or third-party packages

### 15.4 Source of Truth Hierarchy

```
1. Predictly_System_Design.md     ← authoritative design decisions
2. CLAUDE.md (this file)          ← conventions and workflows
3. Existing code patterns         ← follow what's already established
4. Ask the owner                  ← when none of the above resolves ambiguity
```

---

*Last updated: 2026-03-08*
*Project: Predictly v1.0*
*Framework: BMAD AI Development Methodology*
