# US-006: Ler 10.000 linhas em menos de 2 segundos

**As a** usuário do sistema
**I want** que o parsing de um arquivo Excel grande (10.000+ linhas) seja concluído rapidamente
**So that** a experiência de importação seja ágil e não bloqueie a interface do usuário por tempo excessivo

**Description:**
O parser deve processar 10.000 linhas em até 2 segundos, medido em ambiente de desenvolvimento local (Windows, .NET 8). A leitura deve ser feita de forma eficiente: stream-based quando possível, sem carregamento desnecessário de estilos ou metadados da planilha.

**Acceptance Criteria (Gherkin):**

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| 1 | Arquivo grande é lido dentro do SLA | Um `.xlsx` com 10.000 linhas de dados reais da Caixa está disponível | O parsing é executado e cronometrado | A leitura completa (abertura + conversão) leva ≤ 2 segundos em ambiente local |
| 2 | Arquivo muito grande não estoura memória | Um `.xlsx` com 50.000 linhas é enviado | O parsing executa | O uso de memória permanece estável durante o processamento (sem crescimento contínuo); o arquivo é lido sem `OutOfMemoryException` |
| 3 | Performance é testada automaticamente | Um benchmark é configurado no projeto de testes | Os testes são executados em CI/CD | O teste de performance passa; se a latência ultrapassar 2s, o pipeline de CI falha com alerta |

**Business Value:**
Garante que a importação de grandes volumes de dados históricos seja prática e não frustrante para o usuário.

**Justification:**
O SLA de 2 segundos para 10k linhas é razoável para um arquivo Excel típico. Bibliotecas como EPPlus com leitura stream-based (ex.: `OpenXmlReader`) atendem esse requisito sem necessidade de otimizações customizadas. `[Assumption: Ambiente local com SSD e ≥4GB RAM disponível.]`

**Success Criteria:**
- Benchmark automatizado no projeto de testes valida o SLA de 2s para 10k linhas.
- Teste de stress (50k linhas) não causa `OutOfMemoryException`.

---

## Referências

| Field | Value |
|-------|-------|
| **SLA** | ≤ 2 segundos para 10.000 linhas |
| **Feature pai** | FE-001: Parsing e Leitura de Arquivo Excel |
