# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão Geral

SenaPro é um sistema de análise estatística da Mega-Sena com backend .NET 9 e frontend Angular.

## Arquitetura

Clean Architecture com as seguintes camadas:

- **SenaPro.API** — Controllers, Program.cs, configuração da aplicação
- **SenaPro.Domain** — Entidades, enums, interfaces de domínio
- **SenaPro.Application** — Services, DTOs, casos de uso
- **SenaPro.Infrastructure** — EF Core (Npgsql), repositórios, clientes HTTP externos
- **SenaPro.Tests** — Testes unitários com xUnit

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

## Fontes de Dados Externos

- Excel histórico: https://loterias.caixa.gov.br/Paginas/Download-Resultados.aspx
- API Caixa: `https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena`