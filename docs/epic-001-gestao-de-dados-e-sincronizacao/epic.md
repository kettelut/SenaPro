# Gestão de Dados e Sincronização

**ID:** EPIC-001

## Descrição

Este épico abrange a funcionalidade de **importação em lote via arquivo Excel oficial da Caixa Econômica Federal**. O sistema aceita planilhas nos formatos `.xlsx` ou `.xls`, valida as colunas obrigatórias (Concurso, Data Sorteio, Dezena1–Dezena6), processa cada linha, ignora duplicatas no arquivo e no banco de dados, e persiste os registros válidos. Um exemplo do arquivo Excel esperado está disponível em `Mega-Sena-Example.xlsx` neste mesmo diretório (`docs/epic-001-gestao-de-dados-e-sincronizacao/Mega-Sena-Example.xlsx`), servindo como referência para usuários e desenvolvedores sobre o formato aceito. As partes do código-fonte que implementam esse épico são:

- **Backend:**
  - `SenaPro.API/Controllers/SorteiosController.cs` — endpoint `/api/sorteios/importar-excel`.
  - `SenaPro.Application/Services/ExcelImportService.cs` — importação de planilha `.xlsx`/`.xls` com validação de colunas, mapeamento dinâmico e deduplicação.
  - `SenaPro.Infrastructure/Data/AppDbContext.cs` e `SenaPro.Infrastructure/Repositories/SorteioRepository.cs` — persistência EF Core via Npgsql.
  - `SenaPro.Domain/Entities/Sorteio.cs` — entidade domínio com dezenas, prêmios e metadados do concurso.
- **Frontend:**
  - `sena-pro-frontend/src/app/pages/home/home.html` / `[home].ts` — card de importação Excel com drag-and-drop e seletor de arquivo.
