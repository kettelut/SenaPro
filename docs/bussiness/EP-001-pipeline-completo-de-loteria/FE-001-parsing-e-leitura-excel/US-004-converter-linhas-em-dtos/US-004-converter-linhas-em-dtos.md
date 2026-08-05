# US-004: Converter linhas do Excel em DTOs tipados

**As a** sistema de parsing
**I want** ler cada linha de dados e converter as células em um `LotteryResultDto` com campos fortemente tipados (`int Concurso`, `DateTime DataSorteio`, `int[] Dezenas`)
**So that** a camada de validação receba dados prontos para uso sem necessidade de conversão adicional

**Description:**
Para cada linha do arquivo (exceto cabeçalho), o parser deve ler as colunas na ordem esperada (`Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6`) e mapeá-las ao DTO. A conversão de tipos (string → int, string/numérico → DateTime) deve ser feita com tolerância a formatos locais (ex.: data em PT-BR).

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Linha padrão é convertida corretamente | Uma linha contém: `Concurso=1234`, `Data Sorteio="05/08/2026"`, `Dezena1-6=01,02,03,04,05,06` | O parser processa a linha | O DTO resultante tem `Concurso=1234`, `DataSorteio=DateTime(2026,8,5)`, `Dezenas=[1,2,3,4,5,6]` |
| 2 | Data em formato numérico (Excel serial) é convertida | Uma célula de data contém o valor numérico `45312` (serial Excel) | O parser lê a célula | A propriedade `DataSorteio` contém a data equivalente correta via `DateTime.FromOADate()` |
| 3 | Dezenas com valores inválidos são reportadas | Uma linha tem `Dezena3=70` (fora do intervalo válido 1–60) | O parser processa a linha | A linha é incluída no resultado com um campo `ParsingErrors` contendo a mensagem "Dezena3: valor 70 fora do intervalo válido 1-60" |
| 4 | Linha vazia é ignorada silenciosamente | Uma linha contém apenas células nulas/vazias | O parser processa a linha | A linha não gera um DTO e não lança exceção |

**Business Value:**
Elimina a necessidade de a camada downstream (validação/persistência) conhecer detalhes de formato Excel. Dados chegam prontos para uso.

**Justification:**
A conversão de tipos é feita no parser porque ele conhece o formato de origem. Valores inválidos são reportados via campo de erros, não por exceção, para permitir processamento parcial do arquivo. `[Assumption: Ordem das colunas segue o modelo Mega-Sena-Example.xlsx.]`

**Success Criteria:**
- Teste automatizado valida conversão de cada tipo (int, DateTime, int[]).
- Linhas inválidas são reportadas sem quebrar o parsing do restante do arquivo.

---

## Referências

| Field | Value |
|-------|-------|
| **DTO de destino** | `LotteryResultDto` (SenaPro.Application) |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
