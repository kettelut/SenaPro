# FE-001: Parsing e Leitura de Arquivo Excel

**Description:**
Responsável por abrir, detectar o formato e extrair dados brutos do arquivo Excel enviado pelo usuário. Suporta `.xlsx` e `.xls`, navega as sheets esperadas e converte cada linha em um DTO intermediário (`LotteryResultDto`) pronto para validação.

**Business Value:**
- Abstrai a complexidade de leitura de diferentes formatos Excel (BIFF vs. Open XML).
- Fornece dados estruturados para a camada de validação sem acoplamento direto com bibliotecas de parsing.

**Justification:**
O Excel é a fonte primária oficial de resultados da Caixa. Separar parsing de validação permite trocar a fonte (ex.: API direta no futuro) sem reescrever regras de negócio. `[Assumption: O arquivo contém uma única sheet com dados; cabeçalhos na primeira linha.]`

**Success Criteria:**
- Arquivos `.xlsx` e `.xls` são abertos e lidos corretamente.
- Cada linha do arquivo (exceto cabeçalho) é convertida em DTO com campos tipados (`int concurso`, `DateTime dataSorteio`, `int[] dezenas`).
- Arquivo corrompido ou inválido retorna erro claro sem crash da aplicação.
- Performance: leitura de 10.000 linhas em menos de 2 segundos.

---

## Referências

**Arquivo de modelo:** [Mega-Sena-Example.xlsx](Mega-Sena-Example.xlsx) — exemplo oficial da Caixa Econômica Federal com o formato esperado do arquivo de entrada (colunas: Concurso, Data Sorteio, Dezena1–Dezena6).

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-001: Parsing e Leitura de Arquivo Excel
    ├── US-001: Abrir e ler arquivo .xlsx com sucesso
    ├── US-002: Abrir e ler arquivo .xls (BIFF binário) com sucesso
    ├── US-003: Definir interface abstrata e factory de parsing
    ├── US-004: Converter linhas do Excel em DTOs tipados
    ├── US-005: Tratar erros de arquivo corrompido ou inválido sem crash
    ├── US-006: Ler 10.000 linhas em menos de 2 segundos
    └── US-007: Retornar resultado estruturado com estatísticas e erros
```

## Documentos Gerados (Nível 3)

| ID | Arquivo (caminho aninhado) | Descrição |
|----|---------------------------|-----------|
| EP | [EP-001](../EP-001-pipeline-completo-de-loteria.md) | Epic completo com visão geral do pipeline |
| FE | [FE-001](FE-001-parsing-e-leitura-excel.md) | Esta feature — parsing e leitura Excel |
| US-001 | [US-001-parse-arquivo-xlsx/US-001-parse-arquivo-xlsx.md](US-001-parse-arquivo-xlsx/US-001-parse-arquivo-xlsx.md) | Abrir e ler arquivo .xlsx com sucesso |
| US-002 | [US-002-parse-arquivo-xls/US-002-parse-arquivo-xls.md](US-002-parse-arquivo-xls/US-002-parse-arquivo-xls.md) | Abrir e ler arquivo .xls (BIFF binário) com sucesso |
| US-003 | [US-003-interface-abstrata-de-parsing/US-003-interface-abstrata-de-parsing.md](US-003-interface-abstrata-de-parsing/US-003-interface-abstrata-de-parsing.md) | Definir interface abstrata e factory de parsing |
| US-004 | [US-004-converter-linhas-em-dtos/US-004-converter-linhas-em-dtos.md](US-004-converter-linhas-em-dtos/US-004-converter-linhas-em-dtos.md) | Converter linhas do Excel em DTOs tipados |
| US-005 | [US-005-tratamento-de-erros-robusto/US-005-tratamento-de-erros-robusto.md](US-005-tratamento-de-erros-robusto/US-005-tratamento-de-erros-robusto.md) | Tratar erros de arquivo corrompido ou inválido sem crash |
| US-006 | [US-006-desempenho-na-leitura/US-006-desempenho-na-leitura.md](US-006-desempenho-na-leitura/US-006-desempenho-na-leitura.md) | Ler 10.000 linhas em menos de 2 segundos |
| US-007 | [US-007-modelo-de-resultado-do-parsing/US-007-modelo-de-resultado-do-parsing.md](US-007-modelo-de-resultado-do-parsing/US-007-modelo-de-resultado-do-parsing.md) | Retornar resultado estruturado com estatísticas e erros |
