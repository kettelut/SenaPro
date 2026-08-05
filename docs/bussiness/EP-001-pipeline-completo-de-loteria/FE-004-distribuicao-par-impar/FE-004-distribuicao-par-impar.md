# FE-004: Distribuição Par/Ímpar

**Description:**
Algoritmo que, para cada sorteio no banco, conta quantas dezenas são pares (e implicitamente ímpares, já que o total é 6) e computa a distribuição percentual de jogos com exatamente 0, 1, 2, 3, 4, 5 ou 6 números pares ao longo do histórico completo.

**Business Value:**
- Mostra como se distribuem par/ímpar no histórico real — insumo direto para o gerador inteligente (FE-006).
- Permite ao usuário identificar tendências (ex.: "70% dos sorteios têm 3 pares e 3 ímpares").

**Justification:**
A distribuição par/ímpar é uma análise estatística fundamental em loterias — combinações com equilíbrio entre pares e ímpares são historicamente mais frequentes. Calcular isso sobre o histórico real (e não teoricamente) dá ao usuário dados concretos para decisão. `[Assumption: Mega-Sena sempre tem 6 dezenas sorteadas; a contagem de ímpares é complementar (6 - pares).]`

**Success Criteria:**
- Para cada sorteio, a contagem de números pares (0–6) é calculada corretamente.
- A distribuição percentual sobre o total de sorteios é computada para cada bucket (0, 1, 2, 3, 4, 5, 6 pares).
- Percentuais somam 100% (validação de integridade).
- Output estruturado: lista ordenada de `{pares: N, percentual: X.XX%, quantidade: Y}`.
- Performance: cálculo completo em menos de 3 segundos para o histórico inteiro.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-004: Distribuição Par/Ímpar
    └── [US futuras — nível 3]
```
