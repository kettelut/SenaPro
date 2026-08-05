# Stack Tecnológico — SenaPro

Documento que define todas as tecnologias, frameworks, bibliotecas e ferramentas empregadas no projeto **SenaPro** — plataforma de análise estatística e geração inteligente de jogos para loterias.

---

## Relação com Outros Documentos de Tech

| Documento | Foco | Quando consultar |
|-----------|------|------------------|
| **[architecture.md](architecture.md)** | Arquitetura (camadas, padrões de design, dependências) | Quer entender **estrutura** e **decisões arquiteturais** |
| **[development-practices.md](development-practices.md)** | Práticas de desenvolvimento (TDD, workflow, testes) | Quer entender **como** o código é escrito e testado |
| **[ui-ux.md](ui-ux.md)** | Sistema de design, layout e padrões de interface | Quer entender **visual** e **interações** da aplicação |

---

## Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                      Docker Compose                         │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │   PostgreSQL │    │  .NET 8 API  │    │  Angular 21  │  │
│  │    16 Alpine │    │  (Kestrel)   │    │  + Nginx     │  │
│  └──────────────┘    └──────────────┘    └──────────────┘  │
│       :5432               :5000              :4200          │
└─────────────────────────────────────────────────────────────┘
```

---

## Backend (.NET)

### Runtime e Framework

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **.NET SDK** | 8.0 | SDK oficial para build e desenvolvimento |
| **ASP.NET Core** | 8.0 (net8.0) | Framework web para API REST |
| **Kestrel** | 8.0 | Server web cross-platform integrado ao ASP.NET Core |

### Camadas do Projeto

| Projeto | Responsabilidade |
|---------|-----------------|
| `SenaPro.API` | Host da API + Swagger + Dockerfile de produção |
| `SenaPro.Application` | Use-cases, services, DTOs (camada de aplicação) |
| `SenaPro.Domain` | Entidades, repositórios (interface), regras de negócio |
| `SenaPro.Infrastructure` | EF Core, repositórios concretos, EPPlus, integrações externas |

### ORM e Banco de Dados

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Entity Framework Core** | 8.0.15 | ORM principal para acesso a dados |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 8.0.11 | Provider EF Core para PostgreSQL |
| **PostgreSQL** | 16 (Alpine) | Banco de dados relacional em produção e dev |

### Bibliotecas de Negócio

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **EPPlus** | 7.5.2 | Leitura e escrita de arquivos Excel (`.xlsx`/`.xls`) |

---

## Frontend (Angular)

### Runtime e Linguagem

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Node.js** | 20 (alpine) | Runtime JavaScript para build e dev |
| **npm** | 11.12.1 | Gerenciador de pacotes |
| **TypeScript** | ~5.9.2 | Superset tipado do JavaScript |

### Framework e Bibliotecas

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Angular** | 21.2.x (LTS) | Framework SPA principal |
| **ngx-charts / ng2-charts** | ^5.0.4 | Visualização de dados e gráficos |
| **Chart.js** | ^4.5.1 | Biblioteca base para renderização de gráficos |

### Ferramentas de Desenvolvimento

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Angular CLI** | ^21.2.6 | Ferramenta oficial de build e dev |
| **Prettier** | ^3.8.1 | Formatador de código |
| **Vitest** | ^4.0.8 | Framework de testes unitários frontend |
| **JSDOM** | ^28.0.0 | Implementação DOM para testes no Node.js |

---

## Infraestrutura e DevOps

### Contêineres e Orquestração

| Tecnologia | Versão/Imagem | Descrição |
|------------|---------------|-----------|
| **Docker Compose** | 3.8 | Orquestração de múltiplos contêineres |
| **PostgreSQL** | `postgres:16-alpine` | Banco de dados (contêiner) |
| **.NET ASP Runtime** | `mcr.microsoft.com/dotnet/aspnet:8.0` | Runtime de execução da API |
| **.NET SDK** | `mcr.microsoft.com/dotnet/sdk:8.0` | Build da aplicação .NET |
| **Node.js** | `node:20-alpine` | Build do frontend Angular |
| **Nginx** | `nginx:alpine` | Servidor web estático para servir o frontend buildado |

### Arquivos de Configuração

| Arquivo | Responsabilidade |
|---------|-----------------|
| `docker-compose.yml` | Definição dos serviços, volumes e dependências |
| `SenaPro.API/Dockerfile` | Build multi-stage da API (.NET) |
| `sena-pro-frontend/Dockerfile` | Build multi-stage do frontend (Node → Nginx) |

---

## Testes

### Backend (SenaPro.Tests)

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **xUnit** | 2.5.3 | Framework de testes unitários |
| **Moq** | 4.20.72 | Biblioteca de mocking |
| **FluentAssertions** | 8.2.0 | Asserções fluentes |
| **Microsoft.EntityFrameworkCore.InMemory** | 8.0.15 | Provider EF Core em memória para testes |
| **Testcontainers.PostgreSql** | 4.13.0 | Contêineres PostgreSQL efêmeros para testes de integração |
| **Microsoft.AspNetCore.Mvc.Testing** | 8.0.15 | Testes de integração comWebHostFactory |
| **coverlet.collector** | 6.0.0 | Coleta de cobertura de código |

### Frontend

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Vitest** | ^4.0.8 | Framework de testes unitários |
| **JSDOM** | ^28.0.0 | Ambiente DOM para testes no Node.js |

---

## Documentação e API

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **Swagger / Swashbuckle.AspNetCore** | 6.6.2 | Geração automática de documentação OpenAPI + UI interativa |
| **URL do Swagger UI** | `http://localhost:5000/swagger` | Disponível apenas em ambiente **Development** |

---

## Convenções e Padrões

### Nomeção de Projetos

| Camada | Padrão | Exemplo |
|--------|--------|---------|
| API Host | `{NomeProjeto}.API` | `SenaPro.API` |
| Aplicação | `{NomeProjeto}.Application` | `SenaPro.Application` |
| Domínio | `{NomeProjeto}.Domain` | `SenaPro.Domain` |
| Infraestrutura | `{NomeProjeto}.Infrastructure` | `SenaPro.Infrastructure` |
| Testes | `{NomeProjeto}.Tests` | `SenaPro.Tests` |

### Target Framework

- **Todos os projetos .NET:** `net8.0` (confiar nos `.csproj`)
- **Node.js frontend:** `node:20-alpine` (LTS)

### Nomenclatura de Banco de Dados

| Ambiente | Host | Port | Database | User | Password |
|----------|------|------|----------|------|----------|
| Docker Compose | `db` | 5432 | `senapro` | `senapro` | `senapro` |
| Variável de ambiente | — | — | `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=senapro;Username=senapro;Password=senapro` |

---

## Resumo Visual

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER          │  TECHNOLOGIES                              │
├─────────────────┼───────────────────────────────────────────┤
│  Runtime        │  .NET 8, Node.js 20                       │
│  Framework      │  ASP.NET Core 8, Angular 21 (LTS)         │
│  ORM            │  Entity Framework Core 8.0                │
│  Database       │  PostgreSQL 16                            │
│  Excel          │  EPPlus 7.5                               │
│  Charts         │  Chart.js 4, ng2-charts 5                 │
│  Tests (BE)     │  xUnit, Moq, FluentAssertions             │
│  Tests (FE)     │  Vitest                                   │
│  Containers     │  Docker Compose, Nginx Alpine             │
│  Docs           │  Swagger / Swashbuckle 6.6                │
└─────────────────────────────────────────────────────────────┘
```

---

## Histórico de Alterações

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-08-05 | v1.0 | Criação do documento com stack atual do projeto |
