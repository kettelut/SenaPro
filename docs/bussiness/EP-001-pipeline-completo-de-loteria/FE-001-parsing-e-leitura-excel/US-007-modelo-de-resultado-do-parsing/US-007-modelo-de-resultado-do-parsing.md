# US-007: Retornar resultado estruturado com estatísticas e erros

**As a** sistema de pipeline
**I want** receber um objeto de resultado (`ParsingResult`) que contenha os DTOs válidos, contagem de linhas processadas, lista de erros e status geral do parsing
**So that** a camada downstream (FE-002) possa decidir se continua ou reporta problemas ao usuário

**Description:**
O parser deve retornar um objeto `ParsingResult` (ou `ParseResult`) que encapsula: a lista de DTOs válidos, contagem total de linhas processadas, contagem de linhas ignoradas (vazias/inválidas), lista de erros detalhados por linha e status geral (`Success`, `PartialFailure`, `Failed`).

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Resultado completo é retornado para arquivo válido | Um `.xlsx` com 100 linhas válidas é enviado | O parsing executa | `ParsingResult` contém: `Status=Success`, `ValidRows.Count=100`, `IgnoredRows=0`, `Errors=[]` |
| 2 | Resultado indica falha parcial | Um arquivo com 80 linhas válidas e 5 inválidas é enviado | O parsing executa | `ParsingResult` contém: `Status=PartialFailure`, `ValidRows.Count=80`, `IgnoredRows=5`, `Errors=[5 itens]` |
| 3 | Resultado indica falha total | Um arquivo corrompido é enviado | O parsing tenta abrir | `ParsingResult` contém: `Status=Failed`, `ValidRows.Count=0`, `Errors=[mensagem de erro detalhada]` |
| 4 | Contagem de linhas inclui cabeçalho | Um arquivo com 50 linhas de dados + 1 cabeçalho é enviado | O parsing executa | `TotalRowsProcessed=51` (ou campo equivalente que conte todas as linhas lidas) |

**Business Value:**
Fornece visibilidade completa sobre o que foi processado, facilitando debugging e comunicação com o usuário final.

**Justification:**
O resultado estruturado é mais informativo que uma simples lista de DTOs ou exceção. Permite ao pipeline decidir entre continuar (PartialFailure) ou abortar (Failed). `[Assumption: O status PartialFailure indica que dados válidos foram extraídos apesar de problemas.]`

**Success Criteria:**
- `ParsingResult` contém todos os campos necessários para decisão downstream.
- Testes validam cada cenário de resultado (Success, PartialFailure, Failed).

---

## Referências

| Field | Value |
|-------|-------|
| **Modelo** | `ParsingResult` / `ParseResult` (SenaPro.Application) |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
