# Identificação de Concursos Repetidos

**ID:** EPIC-002

## Descrição

Este épico abrange a funcionalidade de **identificar concursos da Mega-Sena que possuem exatamente os mesmos números sorteados, independentemente da ordem em que foram extraídos**. O sistema varre todo o histórico de sorteios armazenado no banco de dados, agrupa as combinações de dezenas (ordenadas) e identifica pares de concursos distintos com sequências idênticas — revelando padrões históricos raros onde a mesma combinação foi sorteada mais de uma vez. As partes do código-fonte que implementam esse épico são:

- **Backend:**
  - `SenaPro.API/Controllers/SorteiosController.cs` — endpoint `/api/sorteios/repetidos`.
  - `SenaPro.Application/Services/AnaliseEstatisticaService.cs` — método `AnalisarSorteiosRepetidosAsync()` que agrupa sorteios por conjunto de dezenas ordenadas e encontra pares duplicados.
  - `SenaPro.Domain/Results/SorteioRepetidoResultado.cs` — DTO com lista de pares repetidos (concurso1, data1, concurso2, data2, dezenas).
- **Frontend:**
  - `sena-pro-frontend/src/app/pages/sorteios/sorteios.html` / `[sorteios].ts` — card "Sorteios Históricos Idênticos" com listagem dos pares repetidos encontrados.
