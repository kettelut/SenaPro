# US-007: Orquestração do Pipeline (Validação → Dedup → Persistência)

**As a** desenvolvedor de integração
**I want** que as etapas de validação, deduplicação e persistência sejam executadas sequencialmente como um pipeline orquestrado
**So that** o fluxo de dados é consistente, com cada etapa filtrando e transformando os resultados da anterior.

**Description:**
Implementar a orquestração do pipeline que encadeia as etapas: (1) Validação Estrutural → (2) Validação Semântica de Valores → (3) Validação Semântica de Datas → (4) Deduplicação Intra-Arquivo → (5) Deduplicação Contra Banco → (6) Persistência com Auditoria. Cada etapa recebe os resultados da anterior e pode filtrar registros, propagando rejeições sem interromper o pipeline inteiro.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Pipeline completo com sucesso | Lote de 500 DTOs válidos e novos | Pipeline é executado sequencialmente | Todos os 500 registros são persistidos; pipeline termina com status "sucesso" |
| 2 | Pipeline com rejeições intermediárias | Lote: 100 entradas → 10 rejeitadas na validação estrutural, 5 na semântica de valores, 3 duplicatas intra-arquivo, 7 duplicatas contra banco | Pipeline é executado sequencialmente | Apenas 75 registros são persistidos; pipeline termina com status "sucesso_com_rejeicoes" e resumo de auditoria |
| 3 | Pipeline aborta em erro fatal | Fonte inválida (arquivo corrompido, exceção não tratável) na etapa de leitura | Pipeline é executado | Pipeline aborta com status "erro"; nenhuma persistência ocorre; evento de auditoria registra o erro fatal |
| 4 | Pipeline vazio — arquivo sem dados | Arquivo Excel com apenas cabeçalho e zero linhas de dados | Pipeline é executado | Pipeline termina com status "sucesso_vazio" (nenhum registro processado); zero efeitos colaterais no banco |
| 5 | Ordem das etapas é preservada | Lote contém linhas que falhariam em múltiplas etapas | Pipeline executa etapas na ordem: estrutura → valores → datas → dedup_arquivo → dedup_banco → persistência | Cada etapa filtra o output da anterior; nenhuma etapa é pulada ou executada fora de ordem |

**Business Value:**
Garante consistência e confiabilidade do pipeline como um todo — cada etapa depende da anterior, e a orquestração assegura que falhas em etapas iniciais não propaguem dados inválidos.

**Justification:**
Sem orquestração explícita, há risco de etapas serem executadas fora de ordem (ex.: dedup antes de validação = trabalho desnecessário). A orquestração também permite tratamento granular de erros por etapa e geração de métricas intermediárias. `[Assumption: Pipeline é síncrono para lote único; assíncrono/paralelo pode ser adicionado no futuro.]`

**Success Criteria:**
- As 6 etapas são executadas na ordem correta, sem exceções.
- Pipeline termina com status claro (sucesso / sucesso_com_rejeicoes / erro / sucesso_vazio).
- Etapas posteriores não processam registros rejeitados por etapas anteriores.
