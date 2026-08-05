# US-006: Persistência com Logging de Auditoria

**As a** operador ou analista de suporte
**I want** que cada registro rejeitado pelo pipeline tenha um log de auditoria com motivo detalhado
**So that** posso investigar falhas na fonte e corrigir planilhas defeituosas.

**Description:**
Cada etapa do pipeline (validação estrutural, validação semântica, deduplicação) deve registrar eventos de auditoria para linhas rejeitadas, contendo: número da linha no arquivo, motivo da rejeição e timestamp. Registros persistidos com sucesso também podem gerar evento de auditoria opcional.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Linha rejeitada — validação estrutural | Linha 42 sem coluna `Dezena5` | Pipeline executa e rejeita a linha | Evento de auditoria é gerado: `{linha: 42, motivo: "Coluna 'Dezena5' ausente", timestamp: ISO8601}` |
| 2 | Linha rejeitada — validação semântica | Linha 15 com `Dezena3 = 75` (fora da faixa) | Pipeline executa e rejeita a linha | Evento de auditoria é gerado: `{linha: 15, motivo: "Dezena 'Dezena3' fora da faixa: 75", timestamp: ISO8601}` |
| 3 | Linha rejeitada — validação de datas | Linha 28 com `Data Sorteio = 2030-01-01` (futuro) | Pipeline executa e rejeita a linha | Evento de auditoria é gerado: `{linha: 28, motivo: "Data no futuro: 2030-01-01", timestamp: ISO8601}` |
| 4 | Persistência bem-sucedida — sem erros | Lote de 500 linhas todas válidas e novas | Pipeline executa com sucesso | Evento de auditoria opcional registrado: `{acao: "persistencia_sucesso", total_inseridos: 500, timestamp: ISO8601}` |
| 5 | Resumo ao final do pipeline | Pipeline processou 1000 linhas (950 inseridas, 30 rejeitadas, 20 duplicatas) | Pipeline termina | Log de resumo é gerado com contadores por etapa: `{validacao_estrutural_rejeitados: 10, validacao_semantica_rejeitados: 20, dedup_arquivo_descartados: 5, dedup_banco_ignores: 15, persistidos: 950}` |

**Business Value:**
Fornece visibilidade operacional sobre a saúde da ingestão — suporte pode identificar rapidamente se erros são pontuais ou sistêmicos na fonte.

**Justification:**
Sem logs de auditoria, não há como diagnosticar por que dados não estão no banco. O logging é essencial para operações em produção e para debugging durante desenvolvimento. `[Assumption: Logs usam o ILogger padrão do .NET; formato JSON estruturado.]`

**Success Criteria:**
- Cada linha rejeitada gera exatamente um evento de auditoria com motivo detalhado.
- Resumo final do pipeline inclui contadores por etapa de processamento.
