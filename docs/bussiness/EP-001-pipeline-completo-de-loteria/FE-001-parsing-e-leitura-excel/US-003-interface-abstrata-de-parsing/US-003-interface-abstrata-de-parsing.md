# US-003: Definir interface abstrata e factory de parsing

**As a** desenvolvedor do sistema
**I want** uma interface `ILotteryFileParser` com um método `ParseAsync(Stream, ParseContext)` e uma factory que detecte o formato automaticamente
**So that** possa trocar implementações (ex.: adicionar API direta no futuro) sem alterar o pipeline principal

**Description:**
O sistema deve definir uma abstração clara entre a detecção de formato e a execução do parsing. A interface expõe um único método assíncrono que recebe um `Stream` brutos e retorna `IReadOnlyList<LotteryResultDto>`. Uma factory (`LotteryFileParserFactory`) inspeciona o cabeçalho do stream para escolher a implementação correta.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Factory detecta .xlsx automaticamente | Um `Stream` com magic bytes de Open XML é passado para a factory | A factory executa a detecção | Uma instância de `XlsxLotteryParser` (ou equivalente) é retornada |
| 2 | Factory detecta .xls automaticamente | Um `Stream` com magic bytes BIFF (`D0 CF 11 E0`) é passado para a factory | A factory executa a detecção | Uma instância de `XlsLotteryParser` (ou equivalente) é retornada |
| 3 | Formato desconhecido lança exceção clara | Um arquivo com extensão `.csv` é passado para a factory | A factory tenta detectar o formato | Uma `NotSupportedException` com mensagem "Formato de arquivo não suportado: {extensão}" é lançada |
| 4 | Pipeline usa apenas a interface | O pipeline principal está implementado | Ele chama `parserFactory.Create(stream)` e depois `ParseAsync()` | O código do pipeline não referencia diretamente nenhuma implementação concreta |

**Business Value:**
Desacopla o pipeline de validação/persistência da escolha de parser, permitindo evolução futura (ex.: adicionar suporte a CSV ou API direta) sem reescrever regras de negócio.

**Justification:**
Separar interface de implementação segue SRP e facilita testes unitários (mock da interface). `[Assumption: Apenas .xlsx e .xls são suportados inicialmente.]`

**Success Criteria:**
- Interface `ILotteryFileParser` definida com contrato claro.
- Factory retorna implementações corretas para `.xlsx`, `.xls` e lança exceção clara para outros formatos.
- Pipeline depende apenas da interface, não de implementações concretas.

---

## Referências

| Field | Value |
|-------|-------|
| **Interface** | `ILotteryFileParser : IAsyncDisposable` (sugestão) |
| **Factory** | `LotteryFileParserFactory` |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
