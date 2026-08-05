# US-001: Abrir e ler arquivo .xlsx com sucesso

**As a** usuário do sistema
**I want** enviar um arquivo Excel no formato `.xlsx` (Open XML) contendo resultados de loteria e ter seus dados extraídos corretamente
**So that** posso prosseguir para validação e importação dos resultados sem intervenção manual

**Description:**
O sistema deve detectar o tipo de arquivo pelo cabeçalho (magic bytes), abrir a primeira sheet e ler todas as linhas de dados, retornando uma lista de DTOs (`LotteryResultDto`) com campos tipados: `int Concurso`, `DateTime DataSorteio`, `int[] Dezenas`.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Arquivo .xlsx válido é lido com sucesso | Um arquivo `.xlsx` legítimo da Caixa está disponível no disco | O usuário seleciona e envia o arquivo pelo UI | O sistema retorna uma lista de `LotteryResultDto` com todas as linhas (exceto cabeçalho) e sem erros |
| 2 | Cabeçalho é ignorado | O arquivo Excel possui linha de cabeçalho na primeira linha | O parsing executa a leitura | A primeira linha (cabeçalhos: Concurso, Data Sorteio, Dezena1..Dezena6) não aparece nos DTOs resultantes |
| 3 | Colunas são mapeadas corretamente | O arquivo possui colunas `Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6` | O parsing executa a leitura | Cada DTO contém: `Concurso` como int, `DataSorteio` como DateTime, `Dezenas` como array de 6 ints |

**Business Value:**
Permite que o pipeline ingira dados reais da Caixa (formato Open XML moderno) sem perda de informação.

**Justification:**
Formato `.xlsx` é o padrão atual de exportação da Caixa. Usar bibliotecas modernas (ex.: EPPlus ou ClosedXML) evita problemas com BIFF binário. `[Assumption: O arquivo segue a estrutura do modelo Mega-Sena-Example.xlsx fornecido.]`

**Success Criteria:**
- Teste automatizado passa ao ler o `Mega-Sena-Example.xlsx` de referência.
- Todas as linhas (exceto cabeçalho) são convertidas em DTOs sem perda de dados.

---

## Referências

| Field | Value |
|-------|-------|
| **Arquivo de modelo** | [Mega-Sena-Example.xlsx](Mega-Sena-Example.xlsx) |
| **Bibliotecas candidatas** | EPPlus, ClosedXML |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
