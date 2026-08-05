# Gerador Inteligente de Jogos

**ID:** EPIC-003

## Descrição

Este épico abrange a funcionalidade que gera combinações de dezenas inteligentes, aplicando filtros estatísticos configuráveis para priorizar apostas com base em padrões históricos. O usuário seleciona quais análises devem ser respeitadas (ex.: evitar repetidos), define a quantidade de jogos e o número de dezenas por jogo (6–15). O serviço então gera combinações que atendam aos critérios, com fallback para tentativas até atingir o limite. As partes do código-fonte que implementam esse épico são:

- **Backend:**
  - `SenaPro.API/Controllers/GeradorController.cs` — endpoints `/api/gerador/analises` (lista de filtros disponíveis) e `/api/gerador/gerar`.
  - `SenaPro.Application/Services/GeradorJogosService.cs` — motor de geração com validação de configuração, aplicação sequencial dos filtros estatísticos e cache interno de análises para evitar consultas repetidas ao repositório.
  - `SenaPro.Domain/Results/JogoSugerido.cs`, `GeracaoJogosResultado.cs`, `ConfiguracaoGeracaoJogos.cs` — modelos de entrada/saída do gerador.
- **Frontend:**
  - `sena-pro-frontend/src/app/pages/gerador/gerador.html` / `[gerador].ts` — formulário com seletor de quantidade de jogos, quantidade de dezenas (6–15) e checkboxes para ativar filtros; exibição animada dos jogos gerados com botão de cópia individual e "copiar todos".
