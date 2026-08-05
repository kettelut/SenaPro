# Level 3 Example — Epic + Features + User Stories

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
The pipeline periodically fetches Mega-Sena results from `https://loteriascaixa-api.herokuapp.com/api/megasena` (or equivalent official endpoint), validates the response, and stores it in the database.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Successful ingestion | The Caixa API is available and returns valid Mega-Sena data | The pipeline runs its scheduled job | Results are persisted with correct numbers, date, and contest number |
| 2 | API unavailable | The Caixa API returns HTTP 503 | The pipeline runs | An error is logged and the job is retried after exponential backoff (max 3 attempts) |
| 3 | Malformed response | The API returns a JSON with missing fields | The pipeline processes the response | The record is rejected, an alert is sent to operators, and processing continues for other lotteries |

**Business Value:** Ensures Mega-Sena data — the most popular lottery in Brazil — is always current.

**Justification:** Mega-Sena is the default lottery type; if it doesn't work, the system fails its primary use case. `[Assumption: The API endpoint URL may change; configuration should be externalized.]`

**Success Criteria:**
- Mega-Sena results are ingested within 30 minutes of official publication on draw days (Wed, Sat).
- Zero data loss over a 30-day period under normal operating conditions.

---

### US-002: Ingest Quina Results from Official API

**As a** system operator
**I want** automated ingestion of Quina draw results from the official source
**So that** all major lottery types have consistent data availability

**Description:**
Same pattern as US-001 but for the Quina lottery type, using its specific API endpoint.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Successful ingestion | The Quina data source is available | The pipeline runs on schedule (Mon–Sat) | Results are persisted correctly |
| 2 | Source returns empty array | The API returns an empty results list for a known draw date | The pipeline processes it | An alert is raised indicating potential source outage; no record is stored |

**Business Value:** Completes coverage of all major lottery types.

**Justification:** Quina draws daily (Mon–Sat); missing even one day creates gaps in statistical analysis. `[Assumption: Same API pattern as Mega-Sena with different endpoint.]`

**Success Criteria:**
- Quina results ingested within 30 minutes of official publication on draw days.
- Zero data loss over a 30-day period under normal conditions.

---

### US-003: Ingest Lotofácil Results from Official API

**As a** system operator
**I want** automated ingestion of Lotofácil results from the official source
**So that** users can access historical data for this frequently-played lottery

**Description:**
Same pattern as US-001 but for Lotofácil, which has different draw days (Mon–Fri) and a 25-number pool instead of 60.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Successful ingestion | The Lotofácil data source is available | The pipeline runs on schedule (Mon–Fri) | Results are persisted correctly |
| 2 | Source changed format | The API response schema differs from expected | The pipeline processes it | Validation fails, alert is raised, and the connector is flagged for update |

**Business Value:** Lotofácil has high participation; incomplete coverage would leave a significant user gap.

**Justification:** Lotofácil draws Mon–Fri (5 days/week), making it the most frequently updated lottery type in the system. `[Assumption: Draw day schedule may change; should be configurable.]`

**Success Criteria:**
- Lotofácil results ingested within 30 minutes of official publication on draw days.
- Zero data loss over a 30-day period under normal conditions.

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

---

### US-005: Log All Ingestion Events with Traceability

**As a** system administrator
**I want** every ingestion event logged with source, timestamp, status, and record count
**So that** I can audit data lineage and diagnose issues

**Description:**
Every pipeline execution logs structured events including: lottery type, source URL, HTTP status code, response time, number of records processed, validation pass/fail counts, and persistence result. Logs are stored in a structured format (JSON) for queryability.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Successful run logged | Pipeline completes ingestion of Mega-Sena with 4 records | — | A log entry is created with status "Success", record count = 4, and trace ID |
| 2 | Failed run logged | Ingestion fails due to network timeout | — | A log entry is created with status "Failed", error message, and retry count |
| 3 | Logs are queryable | Multiple ingestion events exist in the log store | An operator queries by date range | Results return within 2 seconds and include all structured fields |

**Business Value:** Provides audit trail for compliance and rapid incident diagnosis.

**Justification:** Without structured logging, diagnosing data issues requires manual investigation of multiple systems. `[Assumption: Structured logs are stored in the application's existing logging infrastructure (e.g., Serilog → file or external SIEM).]`

**Success Criteria:**
- 100% of ingestion events produce a log entry within 1 second of completion.
- Logs contain all required structured fields for traceability.
- Log query performance remains under 2 seconds for date-range filters over 6 months of data.

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

### US-006: Define Lottery Data Schema

**As a** database architect
**I want** a normalized schema for lottery draws, results, and metadata
**So that** all consumers have a consistent data model

**Description:**
Design and implement the database tables: `Lottery` (id, name, drawDays, numberRange), `Draw` (id, lotteryId, contestNumber, drawDate, status), `DrawResult` (id, drawId, numbers[], prizeBreakdown). Include indexes on foreign keys and query-hot fields.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Schema created | No tables exist | Migration runs | All three tables are created with correct types, constraints, and indexes |
| 2 | Data conforms to schema | A record violates a constraint (e.g., numbers > 50 for Mega-Sena) | Insert attempted | Insert is rejected with a clear constraint violation error |

**Business Value:** Foundation for all downstream features; incorrect schema propagates errors everywhere.

**Justification:** Schema design decisions made early are costly to change later. `[Assumption: Each lottery has a fixed number range and draw day schedule.]`

**Success Criteria:**
- Schema supports ≥5 years of data without performance degradation.
- All constraints enforced at the database level (not just application layer).

---

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

---

### US-008: Deduplicate Records from Multiple Sources

**As a** data engineer
**I want** the system to detect and merge duplicate draw records from different sources
**So that** we maintain a single authoritative record per draw

**Description:**
When two sources report the same lottery, contest number, and date, the system treats them as duplicates. The first successfully validated record is kept; subsequent attempts are compared for consistency. If they match, the duplicate is silently merged. If they differ, an alert is raised for manual review.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Identical records from two sources | Source A and Source B both return Mega-Sena contest 2600, date 2024-01-10 with same numbers | Both are ingested | One record is stored; the other is marked as duplicate and discarded |
| 2 | Conflicting records from two sources | Same lottery/date but different numbers | Both are ingested | First record stored; second triggers "conflict" alert for manual review |

**Business Value:** Prevents data inconsistency when multiple feeds report the same draw.

**Justification:** Some lotteries have redundant data sources (official API + third-party aggregator). Without deduplication, the database accumulates conflicting copies of the same draw. `[Assumption: Deduplication key is (lotteryType, contestNumber, drawDate).]`

**Success Criteria:**
- Zero duplicate records in the database for the same (lottery, date, contest) combination.
- Conflicts are detected and alerted within 1 minute of second ingestion attempt.

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

---

### US-010: Get Latest Draw Result by Lottery Type

**As a** game generator consumer
**I want** to fetch the most recent draw result for a specific lottery type
**So that** I can use it as a baseline for generating new games

**Description:**
GET `/api/lottery-results/latest/{lotteryType}` returns the single most recent draw with its full results. If no data exists, returns 404.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Data exists | Latest Mega-Sena draw is contest 2650, date 2024-06-15 | Request made | Returns the full draw object with status 200 |
| 2 | No data exists | No Mega-Sena draws in database | Request made | Returns 404 with message "No results found for lottery type: MegaSena" |

**Business Value:** Provides the baseline data needed by downstream features like game generation.

**Justification:** Many features need the "current state" of a lottery; caching this at the API layer avoids repeated database lookups. `[Assumption: "Latest" is defined by drawDate, not contestNumber.]`

**Success Criteria:**
- Response time <50ms (cached lookups).
- Returns correct latest draw even after multiple new draws are ingested on the same day.

---

### US-011: Generate OpenAPI Documentation Automatically

**As a** developer
**I want** API documentation to be auto-generated from code annotations
**So that** consumers always have up-to-date endpoint reference without manual doc maintenance

**Description:**
Integrate Swashbuckle (ASP.NET Core) to auto-generate OpenAPI 3.0 specification from controller attributes and XML comments. The Swagger UI is available at `/swagger` in Development environment only.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | New endpoint added | Developer adds a new controller method with `[HttpGet]` and XML doc comment | Swagger is accessed in Dev environment | The new endpoint appears in the Swagger UI with its description, parameters, and response types |
| 2 | Swagger disabled in Production | Environment is Production | Attempt to access `/swagger` | Returns 404; no documentation endpoint exposed |

**Business Value:** Eliminates documentation drift between code and docs.

**Justification:** Auto-generated docs ensure the contract never goes out of sync with implementation. `[Assumption: Swagger UI is only available in Development for security.]`

**Success Criteria:**
- All public endpoints documented with request/response schemas.
- Swagger UI accessible at `/swagger` in Development; inaccessible in Production.

---

## FE-004: Configuration & Source Management

**Description:**
Administrative interface for managing lottery sources, schedules, and connector configurations. Allows operators to enable/disable data sources, adjust polling intervals, configure validation rules per source, and view ingestion health dashboards without code deployment.

**Business Value:**
- Empowers operations teams to manage the system without developer intervention.
- Reduces operational risk by making configuration changes auditable and reversible.

**Justification:** As new lottery types are added or existing sources change (URL updates, API versioning), operators need a way to adapt the system quickly. Hard-coded configurations force every change through a full deployment cycle. `[Assumption: This feature targets internal operators; not end-user facing.]`

**Success Criteria:**
- Operators can add a new lottery source configuration in <15 minutes.
- Configuration changes take effect within 1 minute without service restart.
- All configuration changes are logged with operator identity and timestamp for audit.

### US-012: Add New Lottery Source via Admin Panel

**As a** system operator
**I want** to register a new lottery data source through an admin interface
**So that** I can onboard new lotteries without developer involvement

**Description:**
Admin POST `/api/admin/sources` accepts: name, apiUrl, pollIntervalMinutes, enabled flag, and validation rules. The new source becomes active after validation passes and the system restarts its scheduler to include it.

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Valid source added | Operator submits a complete, valid source configuration | Admin endpoint processes request | Source is created with status "Active"; scheduler picks it up within 1 minute |
| 2 | Invalid source rejected | Operator omits the apiUrl field | Admin endpoint processes request | Returns 400 with error "apiUrl is required" |

**Business Value:** Accelerates onboarding of new lottery types from days to minutes.

**Justification:** Each new lottery type currently requires a code change and deployment; this removes that bottleneck. `[Assumption: Source validation rules are predefined per lottery type.]`

**Success Criteria:**
- New sources become active within 1 minute of configuration save.
- Configuration changes require no service restart or redeployment.

---

### US-013: Monitor Ingestion Health Dashboard

**As a** system operator
**I want** to view real-time health metrics for all data ingestion pipelines
**So that** I can quickly identify and resolve source failures

**Description:**
Admin GET `/api/admin/health` returns current status of each lottery source: last successful ingestion time, consecutive failures count, average response time, and total records ingested today. Data is refreshed in real-time (≤30 seconds delay).

**Acceptance Criteria (Gherkin):**
| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Health check returns current data | Pipelines have run at least once today | Admin requests health status | Returns JSON with per-source metrics including lastSuccessAt, failureCount, avgResponseTimeMs |
| 2 | Source marked as unhealthy | A source has failed 5 consecutive times | Health check requested | That source's status is "Unhealthy" with the failure count and last error message |

**Business Value:** Reduces mean-time-to-resolution for ingestion failures.

**Justification:** Without a health dashboard, operators discover failures only when downstream features report missing data. `[Assumption: Threshold for "unhealthy" is 5 consecutive failures; configurable per source.]`

**Success Criteria:**
- Health metrics refreshed within 30 seconds of last pipeline execution.
- Dashboard loads in <1 second with all sources displayed.

---

## Hierarchy

```
EP-001: Centralized Lottery Data Management Platform
├── FE-001: Data Ingestion Pipeline
│   ├── US-001: Ingest Mega-Sena Results from Official API
│   ├── US-002: Ingest Quina Results from Official API
│   ├── US-003: Ingest Lotofácil Results from Official API
│   ├── US-004: Retry Failed Ingestions with Backoff
│   └── US-005: Log All Ingestion Events with Traceability
├── FE-002: Data Storage & Validation Layer
│   ├── US-006: Define Lottery Data Schema
│   ├── US-007: Validate Incoming Records Against Schema
│   └── US-008: Deduplicate Records from Multiple Sources
├── FE-003: Internal Data Access API
│   ├── US-009: Query Lottery Results by Date Range
│   ├── US-010: Get Latest Draw Result by Lottery Type
│   └── US-011: Generate OpenAPI Documentation Automatically
└── FE-004: Configuration & Source Management
    ├── US-012: Add New Lottery Source via Admin Panel
    └── US-013: Monitor Ingestion Health Dashboard
```
