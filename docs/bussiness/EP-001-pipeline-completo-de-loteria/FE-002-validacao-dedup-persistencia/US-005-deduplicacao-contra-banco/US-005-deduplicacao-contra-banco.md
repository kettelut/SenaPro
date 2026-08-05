# US-005: Deduplicação Contra Banco de Dados

**As a** operador de ingestão em lote
**I want** que registros já existentes no banco (mesmo `Concurso`) sejam detectados e ignorados silenciosamente durante a persistência
**So that** o pipeline é idempotente — pode ser re-executado sem duplicar dados.

**Description:**
Antes de inserir os DTOs aprovados, consultar o banco para identificar quais `Concurso` já existem. Registros duplicados são ignorados silenciosamente (sem registro de auditoria). Apenas registros novos são persistidos via insert em lote.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Concurso já existe no banco | Lote com Concurso 2500; registro idêntico já persistido no banco | Pipeline executa persistência | Registro não é inserido; zero efeitos colaterais |
| 2 | Todos os concursos são novos | Lote com 50 concursos únicos que não existem no banco | Pipeline executa persistência | Todos os 50 registros são inseridos via batch insert |
| 3 | Mix de novos e duplicados | Lote com 100 concursos; 70 já existem, 30 são novos | Pipeline executa persistência | Apenas 30 registros novos são inseridos; 70 ignorados silenciosamente |
| 4 | Re-execução do pipeline (idempotência) | Pipeline executado uma vez com lote X; executado novamente com mesmo lote X | Pipeline é re-executado | Zero duplicatas no banco após a segunda execução |
| 5 | Concurso novo entre duplicados existentes | Lote: [Concurso 100 (novo), 200 (existente), 300 (novo), 400 (existente)] | Pipeline executa persistência | Apenas concursos 100 e 300 são inseridos; 200 e 400 ignorados |

**Business Value:**
Permite re-execução segura do pipeline sem risco de duplicação, essencial para retrabalho ou correções na fonte.

**Justification:**
O pipeline pode ser executado múltiplas vezes (dados atualizados, correções). A deduplicação contra banco garante idempotência — um requisito crítico para pipelines de ingestão em produção. `[Assumption: Consulta de existência é feita por campo 'Concurso' com índice único.]`

**Success Criteria:**
- Zero duplicatas no banco após qualquer execução do pipeline.
- Pipeline é idempotente: re-execução não altera o estado do banco.
