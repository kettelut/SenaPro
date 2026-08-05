# US-003: Validação Semântica de Datas

**As a** consumidor do pipeline de ingestão (FE-003, FE-004…)
**I want** que as datas dos sorteios sejam válidas e não futurísticas
**So that** registros com datas impossíveis ou incorretas não poluem o histórico.

**Description:**
Após a validação estrutural ser aprovada, verificar se `Data Sorteio` é uma data válida (formato correto, dia/mês/ano coerentes) e se não está no futuro em relação à data atual do sistema. Linhas com datas inválidas ou futuras são rejeitadas com registro de auditoria.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Data válida e no passado | DTO com `Data Sorteio = 2024-06-15` (data atual: 2026-08-05) | Pipeline executa validação semântica de datas | DTO é aprovado e prossegue para deduplicação |
| 2 | Data válida — dia 31 em mês com 30 dias | DTO com `Data Sorteio = 2024-02-31` (fevereiro não tem 31 dias) | Pipeline executa validação semântica de datas | DTO é rejeitado; auditoria registra "Data inválida: 2024-02-31" |
| 3 | Data no futuro | DTO com `Data Sorteio = 2030-01-01` (data atual: 2026-08-05) | Pipeline executa validação semântica de datas | DTO é rejeitado; auditoria registra "Data no futuro: 2030-01-01" |
| 4 | Data nula após parsing | DTO com `Data Sorteio = null` (coluna existente mas vazia) | Pipeline executa validação semântica de datas | DTO é rejeitado; auditoria registra "Coluna 'Data Sorteio' ausente ou nula" |
| 5 | Formato de data incorreto | DTO com `Data Sorteio = "15/06/2024"` (formato DD/MM/YYYY quando esperado YYYY-MM-DD) | Pipeline executa validação semântica de datas | DTO é rejeitado; auditoria registra "Formato de data inválido: 15/06/2024" |
| 6 | Data válida — dia 30 em mês com 30 dias | DTO com `Data Sorteio = 2024-04-30` (abril tem 30 dias) e data atual: 2026-08-05 | Pipeline executa validação semântica de datas | DTO é aprovado |

**Business Value:**
Garante a consistência temporal do histórico de sorteios, impedindo que datas futuras ou inválidas corrompam análises estatísticas downstream.

**Justification:**
Datas de sorteio no futuro são impossíveis e indicam erro de fonte (planilha não atualizada, coluna deslocada). A validação previne poluição dos dados históricos. `[Assumption: Data atual do sistema é usada como referência; fusos horários seguem horário de Brasília.]`

**Success Criteria:**
- Todas as datas futuras em relação à data atual são rejeitadas.
- Todas as datas com formato inválido ou dia impossível (ex.: 31/02) são rejeitadas.
