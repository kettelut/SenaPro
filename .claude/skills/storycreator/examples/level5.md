# Level 5 Example — Epic + Features + User Stories + Tasks + Test Cases

**Input:**
> We need a system to manage lottery data: import results from official sources, store them centrally, and make them available for analysis. Currently everything is scattered across spreadsheets and manual checks.

---

## EP-001: Centralized Lottery Data Management Platform

**Description:**
A unified platform that ingests lottery results from official external sources (government APIs, web scraping), stores them in a normalized database, and exposes them through internal services for downstream analysis and reporting. Replaces the current fragmented spreadsheet-based workflow with automated pipelines and a single source of truth.

**Business Value:**
- Eliminates manual data entry errors across all lottery result sources.
- Reduces time-to-insight for statistical analysis from days to minutes.
- Provides audit-ready data lineage from source to consumption.
- Enables future analytics, reporting, and game-generation features on a reliable data foundation.

**Justification:**
The current process relies on manual CSV imports and spreadsheet reconciliation across multiple teams. This creates version conflicts, delayed analyses, and no audit trail. A centralized ingestion layer with automated validation resolves these issues while providing the data infrastructure needed for subsequent feature development (statistical analysis, game generation). `[Assumption: Primary lottery types are Mega-Sena, Quina, and Lotofácil — common Brazilian lotteries.]`

**Success Criteria:**
- All supported lottery results are automatically ingested at least once per draw day with zero manual intervention.
- Data validation catches ≥99% of corrupted or malformed source records before persistence.
- Historical data (last 5 years) is migrated and available in the new system within the first sprint.
- Internal API returns lottery results in under 200ms for 95th-percentile queries.

---

## FE-001: Data Ingestion Pipeline

**Description:**
Automated pipeline that connects to official lottery result sources, fetches draw data on schedule (after each draw), validates the incoming payload against a schema, and persists validated records into the central database. Includes retry logic, error logging, and source health monitoring.

**Business Value:**
- Removes manual extraction step; results are available within minutes of the official publication.
- Ensures data quality at the point of entry through built-in validation rules.

**Justification:**
Without automated ingestion, the system cannot achieve its purpose as a "single source of truth." Each lottery type requires its own connector because sources differ (REST API for some, HTML scraping for others). `[Assumption: Official Caixa APIs are accessible without authentication.]`

**Success Criteria:**
- Pipeline runs successfully ≥99% of scheduled executions per month.
- Failed extractions trigger an alert within 5 minutes and retry up to 3 times with exponential backoff.
- New lottery types can be added as connectors in ≤2 days of development.

### US-001: Ingest Mega-Sena Results from Official API

**As a** system operator
**I want** automated ingestion of Mega-Sena draw results from the official Caixa API
**So that** data is available for analysis without manual intervention

**Description:**
The pipeline periodically fetches Mega-Sena results from the official source, validates the response, and stores it in the database.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Successful ingestion | The API is available and returns valid data | The pipeline runs its scheduled job | Results are persisted with correct numbers, date, and contest number |
| 2 | API unavailable | The API returns HTTP 503 | The pipeline runs | An error is logged and the job is retried after exponential backoff (max 3 attempts) |
| 3 | Malformed response | The API returns JSON with missing fields | The pipeline processes the response | The record is rejected, an alert is sent to operators, and processing continues for other lotteries |

**Business Value:** Ensures Mega-Sena data — the most popular lottery in Brazil — is always current.

**Justification:** Mega-Sena is the default lottery type; if it doesn't work, the system fails its primary use case. `[Assumption: The API endpoint URL may change; configuration should be externalized.]`

**Success Criteria:**
- Mega-Sena results are ingested within 30 minutes of official publication on draw days (Wed, Sat).
- Zero data loss over a 30-day period under normal operating conditions.

#### TS-001: Design Mega-Sena Connector Interface

**Description:**
Define the `ILotteryConnector` interface and `MegaSenaConnector` class that handles HTTP communication with the official source. Include methods for fetching results, parsing responses, and detecting source schema changes.

**Type:** Backend

**Related User Story:** US-001

**Acceptance Criteria:**
- Interface supports pluggable connectors for any lottery type.
- Connector returns parsed result objects matching the internal `DrawResult` model.
- Connection timeout configured at 30 seconds; socket timeout at 60 seconds.

---

#### TS-002: Implement Mega-Sena Fetch Logic

**Description:**
Implement the HTTP client logic within `MegaSenaConnector` to call the official API, handle response codes (200/404/5xx), and deserialize JSON into domain objects. Include proper error mapping to domain exceptions (`SourceUnavailableException`, `MalformedResponseException`).

**Type:** Backend

**Related User Story:** US-001

**Acceptance Criteria:**
- HTTP GET returns parsed `MegaSenaResult` on 200.
- Returns `SourceUnavailableException` on 5xx errors.
- Returns `MalformedResponseException` when required fields are missing or invalid.
- Unit tests cover all three response scenarios.

---

#### TS-003: Implement Ingestion Scheduler

**Description:**
Configure the background job scheduler (using ASP.NET Core `IHostedService` + `Timer` or Hangfire) to trigger Mega-Sena ingestion on draw days (Wednesday and Saturday) at 21:30 BRT. Include health checks for the scheduler itself.

**Type:** Backend

**Related User Story:** US-001

**Acceptance Criteria:**
- Scheduler triggers exactly at 21:30 BRT on Wed/Sat.
- Scheduler is skipped on non-draw days without error.
- Health check endpoint reports scheduler status as "Healthy" when running correctly.

---

#### TS-004: Add Mega-Sena to Database Context

**Description:**
Create the EF Core entity `LotteryResult` with properties for lottery type, contest number, draw date, drawn numbers, and prize breakdown. Add migration and ensure the table has appropriate indexes on `(LotteryType, DrawDate)` for query performance.

**Type:** Database

**Related User Story:** US-001

**Acceptance Criteria:**
- Migration creates `LotteryResults` table with all required columns and constraints.
- Index on `(LotteryType, DrawDate)` improves date-range query performance.
- Entity includes `[Required]` validations for all mandatory fields.

---

#### TS-005: Write Unit Tests for Mega-Sena Connector

**Description:**
Create xUnit tests covering: successful response parsing, HTTP 5xx error handling, malformed JSON handling, and timeout behavior. Use `HttpMessageHandler` mocking (via Moq or `WebApplicationFactory`).

**Type:** Testing

**Related User Story:** US-001

**Acceptance Criteria:**
- ≥8 unit tests covering all response scenarios.
- Tests run in <2 seconds total.
- No external HTTP calls during test execution (all mocked).

---

#### TS-006: Add Structured Logging to Connector

**Description:**
Integrate Serilog with structured logging into the Mega-Sena connector. Log: request start/end, response status code, response time, record count, and any validation errors. Include correlation IDs for traceability.

**Type:** Observability

**Related User Story:** US-001

**Acceptance Criteria:**
- Every ingestion attempt produces at least one structured log entry.
- Log entries include: lottery type, timestamp, status (Success/Failed), record count, response time.
- Logs are queryable by date range and lottery type in <2 seconds.

---

### TC-001: Happy Path — Successful Mega-Sena Ingestion

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify that valid Mega-Sena data is correctly ingested end-to-end |
| **Priority**  | High                                     |
| **Type**      | Functional                               |
| **Preconditions** | Database is empty; Mega-Sena connector configured with valid API URL; scheduler enabled |

**Test Steps:**
1. Trigger Mega-Sena ingestion manually (bypass scheduler).
2. Simulate API returning valid JSON: `{"numbers": [5, 12, 23, 34, 45, 50], "contestNumber": 2650, "drawDate": "2024-06-15"}`.
3. Wait for ingestion to complete.
4. Query the database for contest number 2650.

**Expected Result:**
- Database contains one record with lottery type = "MegaSena", contest number = 2650, numbers = [5, 12, 23, 34, 45, 50], draw date = 2024-06-15.
- Log entry shows status = "Success" with record count = 1.
- No alert was sent.

---

### TC-002: Happy Path — Ingestion After Retry Succeeds

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify that ingestion succeeds on second retry after initial transient failure |
| **Priority**  | High                                     |
| **Type**      | Functional                               |
| **Preconditions** | Mega-Sena connector configured; source initially unavailable (simulated); retry policy active |

**Test Steps:**
1. Trigger Mega-Sena ingestion.
2. Simulate API returning HTTP 503 on first attempt.
3. Allow pipeline to retry after 1 minute delay.
4. Simulate API returning valid JSON on second attempt.
5. Verify database state and logs.

**Expected Result:**
- Database contains the ingested record (contest number matches).
- Log shows: attempt 1 = "Failed" (HTTP 503), attempt 2 = "Success".
- No alert was sent to operators.
- Total ingestion time ≤ 90 seconds.

---

### TC-003: Validation — Duplicate Numbers Rejected

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify that draws with duplicate numbers are rejected |
| **Priority**  | High                                     |
| **Type**      | Validation                               |
| **Preconditions** | Mega-Sena connector configured; validation rules active (no duplicates allowed) |

**Test Steps:**
1. Trigger Mega-Sena ingestion.
2. Simulate API returning JSON with duplicate numbers: `{"numbers": [5, 12, 12, 34, 45, 50], "contestNumber": 2651, "drawDate": "2024-06-18"}`.
3. Wait for validation to complete.
4. Query the database for contest number 2651.

**Expected Result:**
- No record exists in the database for contest 2651.
- Log entry shows status = "Failed" with error = "Duplicate number: 12".
- Alert sent to operators indicating validation failure.

---

### TC-004: Validation — Out-of-Range Number Rejected

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify that numbers outside the valid range [1–60] for Mega-Sena are rejected |
| **Priority**  | High                                     |
| **Type**      | Validation                               |
| **Preconditions** | Mega-Sena connector configured; validation rules active (range check: 1–60) |

**Test Steps:**
1. Trigger Mega-Sena ingestion.
2. Simulate API returning JSON with out-of-range number: `{"numbers": [5, 12, 23, 34, 45, 61], "contestNumber": 2652, "drawDate": "2024-06-22"}`.
3. Wait for validation to complete.
4. Query the database for contest number 2652.

**Expected Result:**
- No record exists in the database for contest 2652.
- Log entry shows status = "Failed" with error = "Number 61 exceeds valid range [1-60]".
- Alert sent to operators indicating validation failure.

---

### TC-005: Error — Source Permanently Unavailable (All Retries Fail)

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify behavior when the source is down for all retry attempts |
| **Priority**  | Medium                                   |
| **Type**      | Error                                    |
| **Preconditions** | Mega-Sena connector configured; source returns HTTP 503 on all attempts; retry policy active (max 3 retries) |

**Test Steps:**
1. Trigger Mega-Sena ingestion.
2. Simulate API returning HTTP 503 on all 3 attempts (with delays: 1min, 5min, 15min).
3. Wait for final failure and alert generation.
4. Check database state, logs, and alerts.

**Expected Result:**
- No record persisted to the database.
- Log shows 3 failed attempts with increasing delay intervals.
- Alert sent to operators within 5 minutes of final failure.
- Alert includes: lottery type ("MegaSena"), last error message, timestamp, and link to health dashboard.
- Job status in admin panel = "Failed".

---

### TC-006: Edge Case — API Returns Empty Array

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify handling when the source returns a valid but empty response (no results for today) |
| **Priority**  | Medium                                   |
| **Type**      | Edge Case                                |
| **Preconditions** | Mega-Sena connector configured; API is available but returns `{"numbers": [], "contestNumber": null}` |

**Test Steps:**
1. Trigger Mega-Sena ingestion on a non-draw day (e.g., Sunday).
2. Simulate API returning empty numbers array: `{"numbers": [], "contestNumber": null, "drawDate": null}`.
3. Wait for pipeline to complete.
4. Check logs and alerts.

**Expected Result:**
- No record persisted (empty draw is not a valid result).
- Log shows status = "Skipped" with reason = "No results available".
- No alert sent (this is expected behavior on non-draw days).
- Pipeline continues to next lottery type without error.

---

### TC-007: Edge Case — Malformed JSON Response

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify handling when the API returns valid HTTP 200 but with malformed JSON body |
| **Priority**  | High                                     |
| **Type**      | Error                                    |
| **Preconditions** | Mega-Sena connector configured; source is available but returns invalid JSON |

**Test Steps:**
1. Trigger Mega-Sena ingestion.
2. Simulate API returning HTTP 200 with body: `{numbers: [5, 12, 23], contestNumber: "not-a-number"}` (missing quotes, invalid type).
3. Wait for pipeline to complete.
4. Check logs and alerts.

**Expected Result:**
- No record persisted.
- Log shows status = "Failed" with error containing JSON parse exception details.
- Alert sent to operators indicating malformed response from source.
- Connector is flagged for schema review.

---

### TC-008: Integration — End-to-End Ingestion Flow (Scheduler → Fetch → Validate → Persist)

| Field         | Value                                    |
|---------------|------------------------------------------|
| **Objective** | Verify the complete ingestion flow works end-to-end with all components interacting correctly |
| **Priority**  | High                                     |
| **Type**      | Integration                              |
| **Preconditions** | Full system deployed (API, database, scheduler); Mega-Sena connector configured; scheduler enabled for manual trigger |

**Test Steps:**
1. Seed the database with 3 existing Mega-Sena draws (contests 2648, 2649, 2650).
2. Trigger ingestion via admin panel (manual override).
3. Simulate API returning new valid data for contest 2651.
4. Wait for full pipeline completion (scheduler → fetch → validate → persist).
5. Query database and verify: (a) total count = 4, (b) new record has contest 2651, (c) no duplicates exist.

**Expected Result:**
- Database contains exactly 4 records with contests 2648–2651.
- New record has correct lottery type, numbers, date, and contest number.
- No duplicate records for any contest number.
- Structured log shows complete trace: scheduler trigger → fetch success → validation pass → persistence success.
- Response time from trigger to completion < 30 seconds.

---

## Hierarchy

```
EP-001: Centralized Lottery Data Management Platform
├── FE-001: Data Ingestion Pipeline
│   ├── US-001: Ingest Mega-Sena Results from Official API
│   │   ├── TS-001: Design Mega-Sena Connector Interface
│   │   ├── TS-002: Implement Mega-Sena Fetch Logic
│   │   ├── TS-003: Implement Ingestion Scheduler
│   │   ├── TS-004: Add Mega-Sena to Database Context
│   │   ├── TS-005: Write Unit Tests for Mega-Sena Connector
│   │   ├── TS-006: Add Structured Logging to Connector
│   │   ├── TC-001: Happy Path — Successful Mega-Sena Ingestion
│   │   ├── TC-002: Happy Path — Ingestion After Retry Succeeds
│   │   ├── TC-003: Validation — Duplicate Numbers Rejected
│   │   ├── TC-004: Validation — Out-of-Range Number Rejected
│   │   ├── TC-005: Error — Source Permanently Unavailable (All Retries Fail)
│   │   ├── TC-006: Edge Case — API Returns Empty Array
│   │   ├── TC-007: Edge Case — Malformed JSON Response
│   │   └── TC-008: Integration — End-to-End Ingestion Flow
```
