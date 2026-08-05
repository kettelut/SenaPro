# Level 2 Example — Epic + Features

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

---

## FE-002: Data Storage & Validation Layer

**Description:**
Database schema design, entity modeling, and validation framework for lottery data. Handles normalization (different sources may use different field names or formats), deduplication, historical versioning, and referential integrity between draws, lotteries, and results.

**Business Value:**
- Guarantees data consistency across all consumers of the system.
- Enables reliable statistical analysis by ensuring clean, well-structured stored data.

**Justification:**
Raw lottery data from different sources has structural inconsistencies (date formats, number ordering, field naming). A normalization layer prevents downstream corruption and makes historical queries predictable. `[Assumption: PostgreSQL is the target database.]`

**Success Criteria:**
- All persisted records pass schema validation before being written.
- Duplicate draws from multiple sources are detected and merged automatically.
- Query performance for date-range lookups stays under 100ms with ≥5 years of data.

---

## FE-003: Internal Data Access API

**Description:**
Internal RESTful service exposing lottery data to other system components (statistics engine, game generator, frontend). Provides CRUD operations on draws, historical result queries by date range or lottery type, and bulk export endpoints for reporting.

**Business Value:**
- Decouples the storage layer from consumers; each consumer evolves independently.
- Enables real-time data availability without direct database access.

**Justification:**
Other features (statistical analysis, game generation) need reliable, performant access to lottery data. A dedicated API layer provides versioning, rate limiting, and clear contracts between components. `[Assumption: Internal consumers are other SenaPro services; external API is out of scope for this epic.]`

**Success Criteria:**
- API returns 200 responses in <200ms (p95) under normal load.
- All endpoints include request validation and return consistent error formats.
- API documentation (OpenAPI/Swagger) is auto-generated and kept in sync with code changes.

---

## FE-004: Configuration & Source Management

**Description:**
Administrative interface for managing lottery sources, schedules, and connector configurations. Allows operators to enable/disable data sources, adjust polling intervals, configure validation rules per source, and view ingestion health dashboards without code deployment.

**Business Value:**
- Empowers operations teams to manage the system without developer intervention.
- Reduces operational risk by making configuration changes auditable and reversible.

**Justification:**
As new lottery types are added or existing sources change (URL updates, API versioning), operators need a way to adapt the system quickly. Hard-coded configurations force every change through a full deployment cycle. `[Assumption: This feature targets internal operators; not end-user facing.]`

**Success Criteria:**
- Operators can add a new lottery source configuration in <15 minutes.
- Configuration changes take effect within 1 minute without service restart.
- All configuration changes are logged with operator identity and timestamp for audit.

---

## Hierarchy

```
EP-001: Centralized Lottery Data Management Platform
├── FE-001: Data Ingestion Pipeline
├── FE-002: Data Storage & Validation Layer
├── FE-003: Internal Data Access API
└── FE-004: Configuration & Source Management
```
