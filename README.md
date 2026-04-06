# SenaPro 🎰

Sistema de análise estatística de jogos da Mega-Sena com gerador de jogos baseado em padrões históricos.

---

## Visão Geral

O SenaPro importa o histórico completo de sorteios da Mega-Sena, analisa padrões estatísticos e gera sugestões de jogos com base nesses dados. O sistema é composto por uma API REST em .NET 9 e um frontend em Angular.

---

## Funcionalidades

### 1. Atualização da Base de Dados

O sistema oferece duas formas de manter o histórico de sorteios atualizado:

#### 1.1 Importação de Excel com Base Histórica
- Importa o arquivo Excel oficial da Caixa Econômica Federal
- Valida se as informações do Excel correspondem às já existentes no banco
- Adiciona apenas os novos registros não existentes na base

#### 1.2 Atualização via API
- Consulta automaticamente a API oficial da loteria
- Compara o resultado retornado com o último sorteio armazenado
- Se identificado um gap (sorteios faltantes entre o último armazenado e o atual), o sistema alerta o usuário para importar o histórico completo

---

### 2. Análise Estatística

Análise de padrões e frequências dos números sorteados ao longo do tempo:

- **2.1 Sorteios Repetidos** — identifica se existem sorteios com os mesmos números sorteados, independentemente da ordem

---

### 3. Gerador de Sugestões de Jogos

Gera sugestões de jogos com base em parâmetros configuráveis:

- Seleção de quais análises estatísticas devem ser respeitadas
- Definição da quantidade de números por jogo
- Definição da quantidade de jogos a gerar
- Exemplo: se selecionada a análise "Sorteios Repetidos" e a base não contém sorteios repetidos, os jogos sugeridos não repetirão padrões históricos

---

## Metodologia de Desenvolvimento

Este projeto é desenvolvido utilizando **TDD (Test-Driven Development)**:

1. **Red** — Escrever um teste que falha
2. **Green** — Implementar o mínimo para fazer o teste passar
3. **Refactor** — Melhorar o código mantendo os testes passando

---

## Stack Tecnológico

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core (.NET 8) |
| Frontend | Angular (LTS) |
| Banco de dados | PostgreSQL |
| ORM | Entity Framework Core + Npgsql |
| Agendamento | Hangfire |
| Documentação | Swagger (Swashbuckle) |
| Análise estatística | MathNet.Numerics |
| Importação de dados | EPPlus |
| Infraestrutura local | Docker Desktop |

---

## Estrutura do Projeto

```
SenaPro/
├── SenaPro.API/              # ASP.NET Core Web API — controllers e configuração
├── SenaPro.Domain/           # Entidades, interfaces e regras de negócio
├── SenaPro.Application/      # Casos de uso, services e DTOs
├── SenaPro.Infrastructure/   # EF Core, repositórios e integrações externas
├── SenaPro.Tests/            # Testes unitários
├── sena-pro-frontend/        # Projeto Angular
├── docker-compose.yml       # Orquestração dos containers
└── .dockerignore             # Arquivos ignorados no build Docker
```

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js LTS](https://nodejs.org) + Angular CLI (`npm install -g @angular/cli`)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Como Executar

### Opção 1: Docker Compose (Recomendado)

Executa todos os serviços com um único comando:

```bash
docker-compose up -d
```

Serviços disponíveis:
- **Frontend**: http://localhost:4200
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432

Para parar os serviços:
```bash
docker-compose down
```

### Opção 2: Execução Local

#### 1. Banco de dados

```bash
docker run --name senapro-db \
  -e POSTGRES_USER=senapro \
  -e POSTGRES_PASSWORD=senapro \
  -e POSTGRES_DB=senapro \
  -p 5432:5432 \
  -d postgres
```

#### 2. API

```bash
cd SenaPro.API
dotnet restore
dotnet ef database update
dotnet run
```

A API estará disponível em `http://localhost:5000`.
Documentação Swagger em `http://localhost:5000/swagger`.

#### 3. Frontend

```bash
cd sena-pro-frontend
npm install
ng serve
```

O frontend estará disponível em `http://localhost:4200`.

---

## Fonte dos Dados

- **Histórico completo:** arquivo Excel disponibilizado pela [Caixa Econômica Federal](https://loterias.caixa.gov.br/Paginas/Download-Resultados.aspx)
- **Resultados recentes:** API oficial `https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena`
- **Atualização automática:** job Hangfire configurado para verificar novos sorteios periodicamente

---

## Configuração

As configurações da aplicação ficam em `SenaPro.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=senapro;Username=senapro;Password=senapro"
  },
  "Hangfire": {
    "IntervalMinutes": 60
  }
}
```

---

## Desenvolvimento com Claude Code + Ollama

Este projeto foi desenvolvido com auxílio do [Claude Code](https://docs.anthropic.com/en/docs/claude-code/overview) integrado ao [Ollama](https://ollama.com) com o modelo `glm-4.7-flash` rodando localmente.

Para iniciar o ambiente de desenvolvimento:

```powershell
# Terminal 1 — inicia o Ollama
ollama serve

# Terminal 2 — inicia o Claude Code
ollama launch claude --model glm-4.7-flash
```

---

## Licença

Uso pessoal. Todos os direitos reservados.