# EP-001: Pipeline Completo de Dados e Inteligência para Loterias

**Description:**
Plataforma unificada que ingere resultados oficiais de loteria, analisa padrões históricos e gera jogos inteligentes com base em estatísticas. O sistema aceita planilhas Excel oficiais da Caixa Econômica Federal (`.xlsx`/`.xls`) como fonte primária de dados, valida colunas obrigatórias (`Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6`), detecta duplicatas no arquivo e no banco de dados, e persiste registros válidos. Sobre essa base confiável, o sistema varre todo o histórico para identificar concursos repetidos (combinações idênticas independentemente da ordem) e gera combinações inteligentes aplicando filtros estatísticos configuráveis — permitindo ao usuário selecionar quais análises respeitar, definir quantidade de jogos e dezenas por jogo (6–15), com fallback automático até atingir o limite.

**Business Value:**
- Elimina trabalho manual de importação e reconciliação de dados de loteria entre planilhas e banco.
- Revela padrões históricos raros (concursos repetidos) que seriam impossíveis de detectar manualmente em milhares de registros.
- Oferece ao apostador uma ferramenta de apoio à decisão baseada em dados reais, não em intuição.
- Fornece infraestrutura de dados centralizada e auditável como base para futuras extensões (outras loterias, dashboards, relatórios).

**Justification:**
Os três épicos originais (`EPIC-001`, `EPIC-002` e `EPIC-003`) descrevem capacidades interdependentes de um mesmo domínio: ingestão → análise → geração. Manter-se separados criava silos de documentação sem refletir a realidade do produto, onde cada etapa depende da anterior. Unificar sob um único épico permite planejamento integrado, identificação clara de dependências e entrega incremental com valor contínuo em cada feature. `[Assumption: Escopo inicial foca apenas na Mega-Sena; outras loterias podem ser adicionadas como extensão futura.]`

**Success Criteria:**
- Todos os resultados oficiais são importados em lote (via Excel) com zero intervenção manual, detectando e ignorando duplicatas automaticamente.
- O sistema identifica corretamente todos os pares de concursos repetidos no histórico completo armazenado.
- O gerador produz combinações que atendem aos filtros estatísticos selecionados pelo usuário em até 5 segundos por requisição.
- A pipeline completa (ingestão → análise → geração) está disponível como API interna com tempo de resposta < 200ms para consultas de leitura.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
├── FE-001: Parsing e Leitura de Arquivo Excel [FE-001-parsing-e-leitura-excel/]
├── FE-002: Validação, Deduplicação e Persistência [FE-002-validacao-dedup-persistencia/]
├── FE-003: Identificação de Concursos Repetidos [FE-003-identificacao-concursos-repetidos/]
├── FE-004: Distribuição Par/Ímpar [FE-004-distribuicao-par-impar/]
├── FE-005: Configuração de Filtros e Parâmetros de Geração [FE-005-configuracao-filtros/]
└── FE-006: Motor de Geração com Fallback [FE-006-motor-geracao-fallback/]
```

**Nota:** Cada Feature tem seu próprio diretório contendo documentação detalhada e, quando aplicável, arquivos de referência (ex.: `Mega-Sena-Example.xlsx` na FE-001).
