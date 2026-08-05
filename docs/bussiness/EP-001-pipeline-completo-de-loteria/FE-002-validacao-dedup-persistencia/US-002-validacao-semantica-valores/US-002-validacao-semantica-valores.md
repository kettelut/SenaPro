# US-002: Validação Semântica de Faixas de Valores

**As a** consumidor do pipeline de ingestão (FE-003, FE-004…)
**I want** que os valores numéricos dos DTOs estejam dentro das faixas válidas da Mega-Sena (dezenas 01–60, concurso > 0)
**So that** registros com valores impossíveis fisicamente são descartados antes de poluir o banco.

**Description:**
Após a validação estrutural ser aprovada, verificar se os valores numéricos respeitam as restrições semânticas do domínio: `Concurso` deve ser maior que zero; cada `Dezena` (1–6) deve estar entre 01 e 60. Linhas com valores fora da faixa são rejeitadas com registro de auditoria.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Dezenas dentro da faixa válida | DTO com `Concurso=2500`, `Dezena1-6` = `[05, 12, 23, 34, 45, 58]` | Pipeline executa validação semântica de valores | DTO é aprovado e prossegue para deduplicação |
| 2 | Dezena fora da faixa — abaixo do mínimo | DTO com `Dezena2 = 00` (abaixo de 01) | Pipeline executa validação semântica de valores | DTO é rejeitado; auditoria registra "Dezena 'Dezena2' fora da faixa: 00 (esperado 01–60)" |
| 3 | Dezena fora da faixa — acima do máximo | DTO com `Dezena5 = 61` (acima de 60) | Pipeline executa validação semântica de valores | DTO é rejeitado; auditoria registra "Dezena 'Dezena5' fora da faixa: 61 (esperado 01–60)" |
| 4 | Concurso nulo ou zero | DTO com `Concurso = 0` | Pipeline executa validação semântica de valores | DTO é rejeitado; auditoria registra "Concurso deve ser maior que zero" |
| 5 | Concurso negativo | DTO com `Concurso = -1` | Pipeline executa validação semântica de valores | DTO é rejeitado; auditoria registra "Concurso deve ser maior que zero" |
| 6 | Todas as dezenas dentro da faixa — lote inteiro | Lote de 200 DTOs com todas as dezenas entre 01–60 e concurso > 0 | Pipeline executa validação semântica no lote | Todos aprovados, zero rejeições |

**Business Value:**
Impede que valores fisicamente impossíveis (dezena 0 ou 70, concurso 0) sejam persistidos, mantendo a integridade referencial dos dados.

**Justification:**
Valores numéricos fora da faixa são erros de fonte (planilha corrompida, coluna deslocada). Detectá-los na validação semântica evita poluição do banco com dados inválidos. `[Assumption: Mega-Sena usa 6 dezenas de 01 a 60; outras loterias podem ter faixas diferentes no futuro.]`

**Success Criteria:**
- Todas as dezenas fora da faixa 01–60 são rejeitadas.
- Todos os concursos com valor ≤ 0 são rejeitados.
