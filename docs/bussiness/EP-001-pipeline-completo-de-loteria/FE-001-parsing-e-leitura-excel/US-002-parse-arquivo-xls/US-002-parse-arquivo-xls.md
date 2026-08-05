# US-002: Abrir e ler arquivo .xls (BIFF binário) com sucesso

**As a** usuário do sistema
**I want** enviar um arquivo Excel antigo no formato `.xls` (BIFF binário) contendo resultados de loteria e ter seus dados extraídos corretamente
**So that** possa importar dados históricos que ainda podem estar em formato legado da Caixa

**Description:**
O sistema deve detectar o tipo de arquivo pelo cabeçalho (magic bytes `D0 CF 11 E0`) e usar um parser compatível com BIFF binário para ler as linhas, retornando DTOs idênticos aos do `.xlsx`.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Arquivo .xls válido é lido com sucesso | Um arquivo `.xls` legítimo está disponível no disco | O usuário seleciona e envia o arquivo pelo UI | O sistema retorna uma lista de `LotteryResultDto` idêntica em estrutura àquela produzida por `.xlsx` |
| 2 | Datas em formato numérico BIFF são convertidas | O arquivo `.xls` armazena datas como números serializados (ex.: `45312`) | O parsing executa a leitura | A propriedade `DataSorteio` é convertida corretamente via `DateTime.FromOADate()` e contém a data real do sorteio |
| 3 | Números de concurso com zeros à esquerda são preservados | A célula `Concurso` no `.xls` está formatada como texto (`"0123456"`) | O parsing executa a leitura | O valor é lido e armazenado como inteiro `123456` (sem perda de informação) |

**Business Value:**
Garante backward compatibility com arquivos antigos da Caixa, ampliando o escopo de compatibilidade do sistema.

**Justification:**
Muitas instituições ainda exportam em `.xls`. O formato BIFF binário requer um parser diferente (ex.: NPOI ou OldExcelReader). `[Assumption: Estrutura de colunas é idêntica ao .xlsx.]`

**Success Criteria:**
- Teste automatizado passa ao ler um arquivo `.xls` sinteticamente criado com estrutura Mega-Sena.
- Datas e números são convertidos corretamente sem perda de informação.

---

## Referências

| Field | Value |
|-------|-------|
| **Bibliotecas candidatas** | NPOI, OldExcelReader (para BIFF) |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
