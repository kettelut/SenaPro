# Level 4 Example — Epic + Features + User Stories + Tasks

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

### US-004: Retry Failed Ingestions with Backoff

**As a** system operator
**I want** failed lottery result extractions to be automatically retried with exponential backoff
**So that** transient source outages don't cause permanent data gaps

**Description:**
When an ingestion attempt fails (network error, API downtime, malformed response), the pipeline retries up to 3 times with delays of 1min → 5min → 15min. After all retries fail, the job is marked failed and an alert is sent.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | First retry succeeds | The first attempt fails with a transient error | The pipeline retries after 1 minute | The record is persisted successfully; no alert is sent |
| 2 | All retries fail | All 3 attempts fail | The pipeline completes the job | The job status is "Failed"; an alert is sent to operators with the source name and last error message |
| 3 | Source recovers mid-retry | Attempts 1–2 fail, attempt 3 succeeds | — | No alert is sent; ingestion is marked successful |

**Business Value:** Prevents data gaps during temporary outages without operator intervention.

**Justification:** External APIs (especially government sources) experience periodic downtime. Without retries, each outage creates a permanent gap in historical data. `[Assumption: Maximum total retry window is 21 minutes; operators are alerted after that.]`

**Success Criteria:**
- ≥95% of transient failures resolve within the retry window (no operator intervention needed).
- Alerts for persistent failures reach operators within 5 minutes of final failure.

#### TS-007: Implement Retry Policy with Exponential Backoff

**Description:**
Create a `RetryPolicy` class using Polly library that implements exponential backoff (1min → 5min → 15min) with jitter. Configure it to retry on transient failures (HTTP 5xx, timeout exceptions) but not on permanent errors (4xx client errors).

**Type:** Backend

**Related User Story:** US-004

**Acceptance Criteria:**
- Retry policy applies exponential backoff with configurable multipliers.
- Jitter is added to prevent thundering herd on source recovery.
- Only transient failure types trigger retries; 4xx errors fail immediately.

---

#### TS-008: Integrate Retry Policy into Ingestion Pipeline

**Description:**
Wrap the Mega-Sena connector call with the retry policy. Ensure the pipeline's orchestration logic correctly handles retry outcomes (success → persist, all-retries-failed → alert). Add unit tests for the integration point.

**Type:** Backend

**Related User Story:** US-004

**Acceptance Criteria:**
- Ingestion pipeline uses retry policy for all external API calls.
- Tests verify that transient failures trigger retries and permanent failures do not.
- Integration test simulates a source outage followed by recovery within the retry window.

---

#### TS-009: Configure Alerting for Persistent Failures

**Description:**
Set up alerting (via Serilog sink to email/Slack/webhook) when all retry attempts fail. Include in the alert: lottery type, last error message, timestamp, and link to ingestion health dashboard.

**Type:** Observability

**Related User Story:** US-004

**Acceptance Criteria:**
- Alert fires within 5 minutes of final retry failure.
- Alert includes all diagnostic information for rapid triage.
- Alert configuration is externalized (appsettings.json) and environment-specific.

---

## FE-002: Data Storage & Validation Layer

**Description:**
Database schema design, entity modeling, and validation framework for lottery data. Handles normalization (different sources may use different field names or formats), deduplication, historical versioning, and referential integrity between draws, lotteries, and results.

**Business Value:**
- Guarantees data consistency across all consumers of the system.
- Enables reliable statistical analysis by ensuring clean, well-structured stored data.

**Justification:** Raw lottery data from different sources has structural inconsistencies (date formats, number ordering, field naming). A normalization layer prevents downstream corruption and makes historical queries predictable. `[Assumption: PostgreSQL is the target database.]`

**Success Criteria:**
- All persisted records pass schema validation before being written.
- Duplicate draws from multiple sources are detected and merged automatically.
- Query performance for date-range lookups stays under 100ms with ≥5 years of data.

### US-007: Validate Incoming Records Against Schema

**As a** data engineer
**I want** every incoming lottery record validated before persistence
**So that** corrupted or malformed data never reaches the database

**Description:**
Validation rules per lottery type: numbers must be within range, no duplicates in a single draw, date must match expected format, contest number is sequential. Validation runs in the ingestion pipeline before the database write.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Valid record passes | Numbers [5, 12, 23, 34, 45, 50], date = "2024-01-10" | Validation runs | Record passes all rules |
| 2 | Duplicate numbers rejected | Numbers [5, 12, 12, 34, 45, 50] | Validation runs | ValidationError raised with "duplicate number: 12" |
| 3 | Out-of-range number rejected | Numbers [5, 12, 23, 34, 45, 61] for Mega-Sena (max 60) | Validation runs | ValidationError raised with "number 61 exceeds range [1-60]" |

**Business Value:** Prevents data corruption at the earliest possible point.

**Justification:** Validation in the pipeline catches issues before they reach consumers; application-level validation alone is insufficient if direct DB access exists. `[Assumption: Each lottery type has its own number range and rules.]`

**Success Criteria:**
- 100% of records validated before persistence.
- Invalid records rejected with descriptive error messages within 50ms.

#### TS-010: Create Validation Rules Engine

**Description:**
Implement a `ValidationRulesEngine` that applies lottery-specific validation rules (number range, uniqueness, date format, sequential contest numbers). Design as a strategy pattern so new lottery types can add custom rules without modifying core logic.

**Type:** Backend

**Related User Story:** US-007

**Acceptance Criteria:**
- Rules engine validates all mandatory fields before persistence.
- New lottery types implement `IValidationRule` interface to add custom rules.
- Validation completes in <50ms per record.

---

#### TS-011: Write Unit Tests for Validation Engine

**Description:**
Create xUnit tests covering valid records, duplicate numbers, out-of-range numbers, invalid dates, and missing fields for each supported lottery type. Test both the core engine and individual rule implementations.

**Type:** Testing

**Related User Story:** US-007

**Acceptance Criteria:**
- ≥12 unit tests covering all validation scenarios per lottery type.
- Tests run in <3 seconds total.
- Each test verifies a specific validation rule independently.

---

## FE-003: Internal Data Access API

**Description:**
Internal RESTful service exposing lottery data to other system components (statistics engine, game generator, frontend). Provides CRUD operations on draws, historical result queries by date range or lottery type, and bulk export endpoints for reporting.

**Business Value:**
- Decouples the storage layer from consumers; each consumer evolves independently.
- Enables real-time data availability without direct database access.

**Justification:** Other features (statistical analysis, game generation) need reliable, performant access to lottery data. A dedicated API layer provides versioning, rate limiting, and clear contracts between components. `[Assumption: Internal consumers are other SenaPro services; external API is out of scope for this epic.]`

**Success Criteria:**
- API returns 200 responses in <200ms (p95) under normal load.
- All endpoints include request validation and return consistent error formats.
- API documentation (OpenAPI/Swagger) is auto-generated and kept in sync with code changes.

### US-009: Query Lottery Results by Date Range

**As a** statistics engine consumer
**I want** to retrieve all lottery results within a date range for a given lottery type
**So that** I can perform historical analysis without querying the database directly

**Description:**
GET `/api/lottery-results?lotteryType=MegaSena&startDate=2024-01-01&endDate=2024-12-31` returns a paginated list of draws with their results. Supports sorting by contest number (ascending/descending).

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Valid range query | Data exists for Jan–Dec 2024 | Request with valid dates | Returns matching draws, paginated at 50 per page, sorted by contest number ascending |
| 2 | Empty result set | No data in the requested range | Request made | Returns 200 with empty array `[]` and total = 0 |
| 3 | Invalid date format | Dates in wrong format (e.g., "01/01/2024") | Request made | Returns 400 with error message "Invalid date format. Use YYYY-MM-DD." |

**Business Value:** Enables statistical analysis by providing filtered historical data access.

**Justification:** Consumers need flexible querying; returning all data and filtering client-side is inefficient at scale. `[Assumption: Default sort order is ascending by contest number; max page size is 200.]`

**Success Criteria:**
- Response time <100ms for queries returning ≤100 records.
- Query returns correct results within 1 second even with ≥5 years of data filtered.

#### TS-012: Implement Date Range Query Endpoint

**Description:**
Create the `GET /api/lottery-results` endpoint accepting `lotteryType`, `startDate`, `endDate`, `page`, and `pageSize` query parameters. Use EF Core with proper filtering, pagination (`Skip/Take`), and ordering. Return paginated response DTO with total count metadata.

**Type:** API

**Related User Story:** US-009

**Acceptance Criteria:**
- Endpoint accepts all documented query parameters with validation.
- Response includes `total`, `page`, `pageSize`, and `items` array.
- Date parameters validated as YYYY-MM-DD format; invalid dates return 400.
- Default pageSize is 50, max is 200.

---

#### TS-013: Add Database Indexes for Query Performance

**Description:**
Add composite indexes on `LotteryResults` table: `(LotteryType, DrawDate)` for date-range queries and `(ContestNumber)` for sequential lookups. Verify query execution plans show index usage after migration.

**Type:** Database

**Related User Story:** US-009

**Acceptance Criteria:**
- Indexes created via EF Core migration.
- Query execution plan shows index scan (not table scan) for date-range queries.
- Performance verified with ≥5 years of simulated data.

---

#### TS-014: Write Integration Tests for Query Endpoint

**Description:**
Create integration tests using `WebApplicationFactory` that spin up the full API stack, seed test data, and verify endpoint behavior: valid queries return correct results, pagination works, invalid parameters return proper error codes.

**Type:** Testing

**Related User Story:** US-009

**Acceptance Criteria:**
- ≥6 integration tests covering happy path, edge cases, and error scenarios.
- Tests use in-memory database (no external dependencies).
- All tests complete in <10 seconds total.

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
│   │   └── TS-006: Add Structured Logging to Connector
│   └── US-004: Retry Failed Ingestions with Backoff
│       ├── TS-007: Implement Retry Policy with Exponential Backoff
│       ├── TS-008: Integrate Retry Policy into Ingestion Pipeline
│       └── TS-009: Configure Alerting for Persistent Failures
├── FE-002: Data Storage & Validation Layer
│   └── US-007: Validate Incoming Records Against Schema
│       ├── TS-010: Create Validation Rules Engine
│       └── TS-011: Write Unit Tests for Validation Engine
└── FE-003: Internal Data Access API
    └── US-009: Query Lottery Results by Date Range
        ├── TS-012: Implement Date Range Query Endpoint
        ├── TS-013: Add Database Indexes for Query Performance
        └── TS-014: Write Integration Tests for Query Endpoint
```
