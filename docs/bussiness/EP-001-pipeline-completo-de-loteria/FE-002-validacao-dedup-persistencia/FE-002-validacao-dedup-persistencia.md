# FE-002: Validação, Deduplicação e Persistência

**Description:**
Pipeline sequencial que aplica validações estruturais e semânticas nos DTOs produzidos pelo parsing, remove duplicatas (dentro do arquivo e contra o banco), e persiste registros válidos no repositório com logging de auditoria.

**Business Value:**
- Garante que apenas dados corretos e únicos cheguem ao banco de dados.
- Fornece visibilidade sobre linhas ignoradas para suporte e debugging.

**Justification:**
Validação, deduplicação e persistência são etapas sequenciais com dependências diretas. A validação estrutural (colunas obrigatórias, tipos) e semântica (faixas de valores, datas válidas) ocorre antes da detecção de duplicatas para evitar trabalho desnecessário. `[Assumption: Chave primária de duplicidade é o número do Concurso.]`

**Success Criteria:**
- Colunas obrigatórias (`Concurso`, `Data Sorteio`, `Dezena1`–`Dezena6`) são verificadas; linhas com colunas faltantes são ignoradas.
- Valores numéricos estão dentro dos limites válidos (dezenas: 01–60, concurso: > 0).
- Datas de sorteio são válidas e não futurísticas.
- Duplicatas internas ao arquivo são removidas antes da persistência.
- Duplicatas já existentes no banco são detectadas e ignoradas silenciosamente.
- Performance total (validação + dedup + insert): 10.000 linhas em menos de 3 segundos.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-002: Validação, Deduplicação e Persistência
    ├── US-001: Validação Estrutural dos DTOs
│   ├── TS-001: Definir interface de validador estrutural
│   └── TC-001: Validar colunas obrigatórias presentes e ausentes
├── US-002: Validação Semântica de Faixas de Valores
│   ├── TS-002: Implementar validação de faixa para Dezena (01–60)
│   └── TC-002: Validar dezenas fora da faixa (00 e 61+)
├── US-003: Validação Semântica de Datas
│   ├── TS-003: Implementar validação de formato e não-futurismo de Data Sorteio
│   └── TC-003: Validar datas no futuro e formatos inválidos
├── US-004: Deduplicação Intra-Arquivo
│   ├── TS-004: Implementar dedup por chave Concurso dentro do lote
│   └── TC-004: Validar remoção de duplicatas exatas e parciais
├── US-005: Deduplicação Contra Banco de Dados
│   ├── TS-005: Consultar banco por concursos existentes antes do insert
│   └── TC-005: Validar idempotência em re-execução do pipeline
├── US-006: Persistência com Logging de Auditoria
│   ├── TS-006: Registrar eventos de auditoria para cada rejeição e persistência
│   └── TC-006: Validar geração de evento de auditoria por linha rejeitada
├── US-007: Orquestração do Pipeline (Validação → Dedup → Persistência)
│   ├── TS-007: Encadear etapas sequencialmente com tratamento granular de erros
│   └── TC-007: Validar ordem das etapas e status final do pipeline
└── US-008: Desempenho do Pipeline Completo
    ├── TS-008: Medir tempo total (entrada → persistência) via benchmark
    └── TC-008: Validar 10.000 linhas em < 3 segundos
```
