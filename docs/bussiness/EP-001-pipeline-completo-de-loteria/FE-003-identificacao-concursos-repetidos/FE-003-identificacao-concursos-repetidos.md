# FE-003: Identificação de Concursos Repetidos

**Description:**
Algoritmo que normaliza combinações de dezenas (ordenando os números), agrupa concursos com base nesse identificador canônico e reporta todos os grupos com mais de uma ocorrência — revelando combinações que foram sorteadas múltiplas vezes.

**Business Value:**
- Identifica padrões raros no histórico que seriam imperceptíveis manualmente.
- Fornece dado estatístico direto para o usuário final (ex.: "a combinação X saiu 3 vezes").

**Justification:**
A normalização é essencial porque a ordem de extração não importa — `{01, 23, 45}` é o mesmo jogo que `{45, 01, 23}`. O identificador canônico permite agrupamento correto sem dependência da ordem de extração. `[Assumption: Combinações com apenas 2 ocorrências são reportadas; grupos maiores (trios, etc.) também são detectados.]`

**Success Criteria:**
- Cada combinação é normalizada ordenando as 6 dezenas em ordem crescente e gerando chave canônica (`"01|05|12|23|34|45"`).
- Grupos com exatamente 2 ocorrências, 3 ocorrências, etc. são todos detectados (não apenas duplicatas exatas de 2).
- Output inclui: combinação repetida, número de vezes que se repetiu, e lista de números dos concursos envolvidos.
- Performance: varredura completa do histórico em menos de 5 segundos.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-003: Identificação de Concursos Repetidos
    └── [US futuras — nível 3]
```
