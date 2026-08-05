# Level 1 Example — Epic Only

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

## Hierarchy

```
EP-001: Centralized Lottery Data Management Platform
```
