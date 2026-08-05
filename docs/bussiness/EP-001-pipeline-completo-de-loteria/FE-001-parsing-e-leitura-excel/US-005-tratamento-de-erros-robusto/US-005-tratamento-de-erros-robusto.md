# US-005: Tratar erros de arquivo corrompido ou inválido sem crash

**As a** usuário do sistema
**I want** receber uma mensagem de erro clara e amigável quando o arquivo Excel estiver corrompido, vazio ou em formato incompatível
**So that** eu possa corrigir o problema (trocar de arquivo) sem perder dados já processados

**Description:**
Antes e durante o parsing, o sistema deve capturar exceções específicas (arquivo corrompido, formato não suportado, stream inválido) e convertê-las em um resultado estruturado com código de erro e mensagem legível. O pipeline deve continuar processando linhas válidas mesmo quando algumas falham.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Arquivo corrompido é detectado | Um `.xlsx` com conteúdo binário inválido é enviado | O parsing tenta abrir o arquivo | Um `LotteryParsingException` (tipo específico) é lançado com mensagem "Arquivo Excel corrompido ou ilegível" e código de erro `PARSE_CORRUPTED_FILE` |
| 2 | Arquivo vazio não causa crash | Um `.xlsx` sem linhas de dados (apenas cabeçalho) é enviado | O parsing executa a leitura | O resultado contém uma lista vazia de DTOs com status `Success` e mensagem informativa "Arquivo sem dados para importar" |
| 3 | Stream nulo ou fechado lança exceção clara | Um stream nulo é passado ao parser | O parsing é iniciado | Uma `ArgumentNullException` ou `InvalidOperationException` é lançada com mensagem "Stream de entrada não pode ser nulo ou fechado" |
| 4 | Erro em linha individual não para o processamento | Uma linha contém dados inválidos (ex.: data fora do intervalo) | O parser continua lendo | A linha falha é reportada no campo `ParsingErrors` do resultado; as demais linhas válidas são retornadas normalmente |

**Business Value:**
Melhora a experiência do usuário final ao fornecer feedback claro sobre problemas de arquivo, evitando frustração e perda de dados.

**Justification:**
O tratamento de erros deve ser granular: erros de estrutura (arquivo corrompido) param o parsing; erros de dado (linha inválida) são isolados para processamento parcial. `[Assumption: O usuário pode enviar qualquer arquivo com extensão .xlsx ou .xls.]`

**Success Criteria:**
- Testes unitários cobrem cada cenário de erro listado acima.
- Nenhum crash não tratado ocorre em cenários de entrada maliciosa/corrompida.

---

## Referências

| Field | Value |
|-------|-------|
| **Exceção customizada** | `LotteryParsingException` (SenaPro.Domain) |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
