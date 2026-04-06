# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão Geral

SenaPro é um sistema de análise estatística da Mega-Sena com backend .NET 8 e frontend Angular.

## Arquitetura

Clean Architecture com as seguintes camadas:

- **SenaPro.API** — Controllers, Program.cs, configuração da aplicação
- **SenaPro.Domain** — Entidades, enums, interfaces de domínio
- **SenaPro.Application** — Services, DTOs, casos de uso
- **SenaPro.Infrastructure** — EF Core (Npgsql), repositórios, clientes HTTP externos
- **SenaPro.Tests** — Testes unitários com xUnit

## Metodologia

Desenvolvimento com **TDD (Test-Driven Development)**:
1. Red — escrever teste que falha
2. Green — implementar mínimo para passar
3. Refactor — melhorar código mantendo testes passando

## Comandos

```bash
# Backend
cd SenaPro.API && dotnet restore && dotnet run

# Migrations
dotnet ef migrations add <Nome> --project SenaPro.Infrastructure --startup-project SenaPro.API
dotnet ef database update --project SenaPro.Infrastructure --startup-project SenaPro.API

# Testes
dotnet test

# Frontend
cd sena-pro-frontend && npm install && ng serve
```

## Convenções

- Commits em português, no imperativo: "Adiciona endpoint de análise estatística"
- C#: PascalCase para membros públicos, _camelCase para privados
- Angular: standalone components, signal-based state management quando apropriado

## Organização de Arquivos (Clean Code)

- **Uma classe por arquivo**: cada classe/interface deve estar em seu próprio arquivo
- **Namespaces por pasta**: o namespace deve refletir a estrutura de pastas
  - `SenaPro.Domain.Entities` → entidades de domínio
  - `SenaPro.Domain.Interfaces` → interfaces de serviços
  - `SenaPro.Domain.Results` → objetos de resultado/dto do domínio
  - `SenaPro.Application.Services` → implementações de serviços
  - `SenaPro.Infrastructure.Data` → contexto e repositórios

## Funcionalidades

### 1. Atualização da Base de Dados

#### 1.1 Importação de Excel com Base Histórica
- Importa arquivo Excel oficial da Caixa Econômica Federal
- Valida se informações do Excel correspondem à base existente
- Adiciona apenas novos registros não existentes

#### 1.2 Atualização via API
- Consulta API oficial da loteria
- Compara resultado com último sorteio armazenado
- Se gap identificado, alerta usuário para importar histórico

### 2. Análise Estatística

- **2.1 Sorteios Repetidos** — identifica sorteios com mesmos números, independente da ordem

### 3. Gerador de Sugestões de Jogos

- Seleção de análises estatísticas a respeitar
- Quantidade de números por jogo configurável
- Quantidade de jogos a gerar configurável
- Respeita restrições das análises selecionadas

---

## Fontes de Dados Externos

- Excel histórico: https://loterias.caixa.gov.br/Paginas/Download-Resultados.aspx
- API Caixa: `https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena`