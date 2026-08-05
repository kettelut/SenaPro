# US-004: Deduplicação Intra-Arquivo

**As a** operador de ingestão em lote
**I want** que duplicatas dentro do mesmo arquivo Excel sejam detectadas e removidas antes da persistência
**So that** o banco não receba registros idênticos provenientes de uma única fonte.

**Description:**
Após as validações estrutural e semântica, identificar linhas com `Concurso` repetido dentro do lote corrente (mesmo arquivo) e manter apenas a primeira ocorrência válida. Linhas duplicadas são descartadas silenciosamente (sem registro de auditoria — é esperado que arquivos possam conter repetições).

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Duplicata exata dentro do arquivo | Lote com Concurso 2500 aparecendo nas linhas 3 e 7, valores idênticos | Pipeline executa deduplicação intra-arquivo | Apenas a linha 3 (primeira ocorrência) é mantida; linha 7 é descartada |
| 2 | Duplicata parcial — mesma data, dezenas diferentes | Lote com Concurso 2501 na linha 1 (`Dezena1-6 = [01..06]`) e linha 8 (`Dezena1-6 = [10..15]`) | Pipeline executa deduplicação intra-arquivo | Apenas a primeira ocorrência (linha 1) é mantida; linha 8 é descartada |
| 3 | Sem duplicatas no arquivo | Lote de 100 linhas com todos os concursos únicos | Pipeline executa deduplicação intra-arquivo | Todas as 100 linhas são mantidas |
| 4 | Duplicata após rejeição estrutural | Concurso 2500 na linha 3 (válido) e linha 9 (rejeitado por coluna ausente) | Pipeline executa deduplicação intra-arquivo | Apenas a linha 3 é processada; linha 9 já foi eliminada na validação |
| 5 | Três ou mais ocorrências do mesmo Concurso | Concurso 2600 nas linhas 1, 4 e 12 com valores idênticos | Pipeline executa deduplicação intra-arquivo | Apenas a primeira (linha 1) é mantida; linhas 4 e 12 são descartadas |

**Business Value:**
Reduz o volume de dados redundantes antes da persistência, economizando espaço no banco e evitando lógica de tratamento de duplicatas no downstream.

**Justification:**
Arquivos Excel oficiais podem conter cabeçalhos repetidos ou células duplicadas por erro de formatação. A deduplicação intra-arquivo é a primeira linha de defesa contra duplicidade. `[Assumption: Chave primária de deduplicação é o campo 'Concurso'.]`

**Success Criteria:**
- Zero registros com mesmo `Concurso` persistem no banco após a etapa de dedup.
- Apenas a primeira ocorrência de cada Concurso é mantida (ordem do arquivo).
