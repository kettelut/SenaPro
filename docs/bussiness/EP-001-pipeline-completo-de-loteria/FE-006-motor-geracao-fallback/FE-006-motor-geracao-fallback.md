# FE-006: Motor de Geração com Fallback

**Description:**
Algoritmo central que gera combinações de dezenas respeitando os filtros estatísticos selecionados (FE-003 / FE-004), com mecanismo de fallback para tentar novas combinações quando as tentativas iniciais não atendem aos critérios. Quando o limite de tentativas é atingido sem sucesso, informa ao usuário com opção de relaxamento automático dos filtros mais restritivos.

**Business Value:**
- Produz jogos que realmente respeitam as preferências do usuário, não apenas combinações aleatórias.
- Fallback evita frustração quando filtros são excessivamente restritivos para o histórico disponível.

**Justification:**
Geração combinatória com restrições é um problema de satisfação de constraints — abordagens ingênuas (gerar aleatoriamente e filtrar) podem falhar rapidamente quando os filtros são restritos. O fallback controlado garante que o sistema sempre retorne algo útil ao usuário. `[Assumption: Limite máximo de tentativas por combinação antes do fallback é configurável; padrão: 1000 tentativas.]`

**Success Criteria:**
- Cada combinação gerada respeita todos os filtros estatísticos selecionados (repetidos evitados, distribuição par/ímpar dentro do range escolhido).
- Geração completa de 10 jogos com 2–3 filtros ativos ocorre em até 5 segundos.
- Quando o limite de tentativas é atingido sem sucesso, o sistema informa ao usuário e oferece relaxamento automático dos filtros mais restritivos.
- Combinações geradas são únicas entre si (sem duplicatas dentro do lote).
- Output estruturado: lista de jogos com dezenas ordenadas + metadados de qual filtro foi aplicado em cada um.

---

## Hierarchy

```
EP-001: Pipeline Completo de Dados e Inteligência para Loterias
└── FE-006: Motor de Geração com Fallback
    └── [US futuras — nível 3]
```
