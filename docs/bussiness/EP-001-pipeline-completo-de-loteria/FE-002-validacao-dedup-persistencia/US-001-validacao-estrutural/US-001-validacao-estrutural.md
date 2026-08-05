# US-001: Validação Estrutural dos DTOs

**As a** consumidor do pipeline de ingestão (FE-003, FE-004…)
**I want** que os DTOs produzidos pelo parsing passem por uma validação estrutural antes da deduplicação e persistência
**So that** apenas registros com formato correto prosseguem no pipeline, evitando erros downstream.

**Description:**
Verificar a presença das colunas obrigatórias (`Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6`) em cada DTO e confirmar que seus tipos são compatíveis (inteiro para Concurso/Dezenas, DateTime para Data Sorteio). Linhas com colunas faltantes ou tipos incompatíveis devem ser rejeitadas silenciosamente com registro de auditoria.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Colunas obrigatórias presentes e válidas | DTO com todas as colunas (`Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6`) preenchidas com tipos corretos | Pipeline executa validação estrutural | DTO é marcado como válido e prossegue |
| 2 | Coluna obrigatória ausente — Concurso | DTO sem a coluna `Concurso` | Pipeline executa validação estrutural | DTO é rejeitado; evento de auditoria é registrado com motivo "Coluna 'Concurso' ausente" |
| 3 | Coluna obrigatória ausente — Dezena | DTO com `Dezena3` ausente (as demais presentes) | Pipeline executa validação estrutural | DTO é rejeitado; evento de auditoria é registrado com motivo "Coluna 'Dezena3' ausente" |
| 4 | Tipo incompatível — Data Sorteio | DTO com `Data Sorteio` como string inválida (ex.: `"abc"`) | Pipeline executa validação estrutural | DTO é rejeitado; evento de auditoria é registrado com motivo "Tipo incompatível em 'Data Sorteio'" |
| 5 | Tipo incompatível — Concurso não inteiro | DTO com `Concurso` como valor decimal (ex.: `123.5`) | Pipeline executa validação estrutural | DTO é rejeitado; evento de auditoria é registrado com motivo "Tipo incompatível em 'Concurso'" |
| 6 | Todas as colunas válidas — lote inteiro | Lote de 500 DTOs todos com estrutura correta | Pipeline executa validação estrutural no lote | Todos os 500 DTOs são aprovados sem eventos de auditoria |

**Business Value:**
Elimina ruído no pipeline antes das etapas caras (dedup, persistência), economizando recursos e evitando falhas em downstream.

**Justification:**
A validação estrutural é o primeiro filtro obrigatório. Sem ela, dados com formato incorreto se propagam pelo pipeline causando exceções não tratadas ou lógica de negócio incorreta. `[Assumption: Tipos esperados são inteiros para Concurso/Dezenas e DateTime para Data Sorteio.]`

**Success Criteria:**
- 100% dos DTOs sem colunas obrigatórias são rejeitados antes da deduplicação.
- Eventos de auditoria são gerados para cada linha rejeitada, com motivo detalhado.
