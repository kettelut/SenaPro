# FE-005: Configuração de Filtros e Parâmetros de Geração

**Description:**
Camada responsável por receber, validar e normalizar os parâmetros de geração informados pelo usuário. Inclui seleção de quais análises estatísticas (FE-003 / FE-004) devem ser respeitadas, definição da quantidade de jogos e do número de dezenas por jogo (6–15), com validação de consistência entre filtros selecionados e limites compatíveis.

**Business Value:**
- Garante que a geração só ocorra com parâmetros válidos e consistentes.
- Fornece feedback imediato ao usuário sobre incompatibilidades antes de executar o motor de geração (custo computacional).

**Justification:**
Separar configuração de geração permite reutilizar a camada de validação em diferentes contextos (ex.: API, batch futuro) e evita que o motor de geração precise conhecer regras de UI. `[Assumption: Filtros disponíveis são os produzidos pela FE-003 (repetidos) e FE-004 (distribuição par/ímpar).]`

**Success Criteria:**
- Usuário pode selecionar zero ou mais filtros da FE-003 / FE-004 para aplicar na geração.
- Quantidade de jogos: mínimo 1, máximo configurável (padrão: 10).
- Dezenas por jogo: entre 6 e 15 (limites fixos da Mega-Sena estendida).
- Filtros incompatíveis (ex.: "evitar repetidos" + "forçar exatamente 3 pares") são detectados antes da geração com mensagem clara ao usuário.
- Parâmetros normalizados são entregues ao motor de geração (FE-006) em formato estruturado.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-005: Configuração de Filtros e Parâmetros de Geração
    └── [US futuras — nível 3]
```
