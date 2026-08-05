# US-008: Desempenho do Pipeline Completo

**As a** usuário final que realiza ingestão em lote
**I want** que o pipeline completo (validação + dedup + persistência) processe 10.000 linhas em menos de 3 segundos
**So that** a ingestão de grandes volumes de dados não bloqueia operações ou cause timeouts.

**Description:**
Garantir que o pipeline inteiro — desde a entrada dos DTOs até a conclusão da persistência — processa 10.000 linhas válidas em menos de 3 segundos. Isso inclui todas as etapas: validação estrutural, semântica (valores e datas), deduplicação intra-arquivo, deduplicação contra banco e persistência via batch insert.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | 10.000 linhas válidas — tempo total | Lote de 10.000 DTOs válidos e novos; banco com índice único em `Concurso` | Pipeline completo é executado | Tempo total (entrada → persistência concluída) < 3 segundos |
| 2 | 50.000 linhas — escala | Lote de 50.000 DTOs válidos e novos; banco com índice único em `Concurso` | Pipeline completo é executado | Tempo total < 15 segundos (escalabilidade linear) |
| 3 | Pipeline com 50% de rejeições | Lote de 10.000 linhas onde ~5.000 são rejeitadas em etapas variadas | Pipeline completo é executado | Tempo total < 3 segundos (rejeições não impactam significativamente) |
| 4 | Pipeline com alta taxa de duplicata contra banco | Lote de 10.000 linhas onde ~8.000 já existem no banco | Pipeline completo é executado | Tempo total < 3 segundos (consulta de existência é eficiente) |
| 5 | Regressão de performance — teste automatizado | Teste de integração com lote de 10.000 linhas | Teste executa e mede tempo | Assert: `totalTimeMs < 3000` |

**Business Value:**
Ingestão rápida permite processamento em tempo razoável mesmo para arquivos grandes, sem necessidade de filas assíncronas ou timeouts.

**Justification:**
O SLA de 3 segundos para 10k linhas foi definido como critério de sucesso da FE-002. Sem esse limite, o pipeline pode se tornar gargalo em cenários de ingestão frequente (ex.: após cada sorteio). `[Assumption: Banco de dados local com SSD; sem carga concorrente significativa.]`

**Success Criteria:**
- Pipeline processa 10.000 linhas válidas em < 3 segundos.
- Tempo escala linearmente até 50.000 linhas (< 15s).
- Testes de performance automatizados garantem regressão.
