# Arquitetura do Projeto SenaPro

Documento que descreve a arquitetura de software aplicada no projeto **SenaPro** — plataforma de análise estatística e geração inteligente de jogos para loterias.

---

## Visão Geral

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         FRONTEND (Angular 21)                          │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐             │
│  │   Pages      │    │  Services    │    │    Models    │             │
│  │  (Componentes│    │ (HTTP Client)│    │ (Interfaces) │             │
│  └──────────────┘    └──────────────┘    └──────────────┘             │
│         │                    │                    │                   │
│         └────────────────────┼────────────────────┘                   │
│                              │ HTTP/JSON                               │
└──────────────────────────────┼─────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      BACKEND (.NET 8 / ASP.NET Core)                    │
│                                                                         │
│  ┌─────────────────┐    ┌──────────────────┐    ┌───────────────────┐  │
│  │   Controllers   │◄──►│     Services     │◄──►│    Interfaces     │  │
│  │   (API Layer)   │    │ (Application)    │    │   (Domain)        │  │
│  └────────┬────────┘    └────────┬─────────┘    └────────┬──────────┘  │
│           │                     │                        │             │
│           └──────────────┐      │      ┌─────────────────┘            │
│                          ▼      │      ▼                               │
│              ┌──────────────────────┴──────┐                           │
│              │   Repositories (Interfaces)  │                           │
│              │   ISorteioRepository         │                           │
│              └─────────────────┬───────────┘                            │
│                                ▼                                       │
│              ┌──────────────────────────────────────┐                  │
│              │   Infrastructure Layer               │                  │
│              │  • SorteioRepository (EF Core)       │                  │
│              │  • AppDbContext                      │                  │
│              │  • Migrations                        │                  │
│              └─────────────────────┬────────────────┘                   │
│                                    ▼                                   │
│              ┌──────────────────────────────────────┐                  │
│              │        PostgreSQL 16 (Alpine)        │                  │
│              │        Host: db :5432                │                  │
│              └──────────────────────────────────────┘                  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Padrão Arquitetural Principal: **Clean Architecture**

O projeto segue o padrão **Clean Architecture** (também conhecido como **Onion Architecture** ou **Layered Architecture**), com dependências direcionadas para dentro — a camada de Domínio não conhece nenhuma outra camada.

### Princípios Fundamentais

| Princípio | Descrição |
|-----------|-----------|
| **Separação de Responsabilidades** | Cada camada tem uma responsabilidade clara e isolada |
| **Dependency Inversion** | Camadas externas dependem de interfaces definidas nas camadas internas |
| **Testability** | Lógica de negócio isolada em testes unitários sem dependências externas |
| **Framework Independence** | UI, Database, Frameworks são detalhes — não o núcleo do sistema |

---

## Camadas do Projeto (Backend)

### 1. `SenaPro.Domain` — Camada de Domínio (Core)

**Responsabilidade:** Entidades, interfaces e regras de negócio puras. Não depende de nenhuma outra camada.

```
SenaPro.Domain/
├── Entities/              ← Modelos de domínio (Sorteio.cs)
├── Interfaces/            ← Contratos para repositórios e services
└── Results/               ← Objetos de resultado/DTOs de domínio
```

| Elemento | Descrição |
|----------|-----------|
| **Entities** | Classes POCO representando conceitos do domínio (`Sorteio`) |
| **Interfaces** | Contratos definidos pelo domínio que a infraestrutura implementa (`ISorteioRepository`, `IExcelImportService`, etc.) |
| **Results** | Objetos de transferência de dados entre camadas (`ImportacaoResultado`, `JogoSugerido`, `SorteioRepetidoResultado`) |

**Exemplo — Entity:**
```csharp
// SenaPro.Domain/Entities/Sorteio.cs
public class Sorteio
{
    public int Id { get; set; }
    public int Concurso { get; set; }
    public DateOnly Data { get; set; }
    public byte Dezena1 { get; set; }
    // ...
}
```

**Exemplo — Interface (Dependency Inversion):**
```csharp
// SenaPro.Domain/Interfaces/ISorteioRepository.cs
public interface ISorteioRepository
{
    Task<List<Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteConcursoAsync(int concurso, CancellationToken cancellationToken = default);
}
```

---

### 2. `SenaPro.Application` — Camada de Aplicação

**Responsabilidade:** Orquestração de casos de uso, serviços de negócio e lógica de aplicação. Depende apenas do Domínio.

```
SenaPro.Application/
├── Services/              ← Implementações de services (ExcelImportService, etc.)
└── DTOs/                  ← Transfer Objects (se aplicável)
```

| Elemento | Descrição |
|----------|-----------|
| **Services** | Lógica de negócio que orquestra operações (`ExcelImportService`, `AnaliseEstatisticaService`, `GeradorJogosService`) |

**Exemplo — Service:**
```csharp
// SenaPro.Application/Services/ExcelImportService.cs
public class ExcelImportService : IExcelImportService
{
    private readonly ISorteioRepository _sorteioRepository;  // Injeta via interface
    
    public ExcelImportService(ISorteioRepository sorteioRepository)
    {
        _sorteioRepository = sorteioRepository;
    }
    
    public async Task<ImportacaoResultado> ImportarAsync(string caminhoArquivo, ...)
    {
        // Regra de negócio: validação, dedup, persistência
    }
}
```

---

### 3. `SenaPro.Infrastructure` — Camada de Infraestrutura

**Responsabilidade:** Implementações concretas de repositórios, acesso a dados e integrações externas. Depende do Domínio e Application.

```
SenaPro.Infrastructure/
├── Data/                  ← DbContext, Migrations
└── Repositories/          ← Implementações concretas dos repositórios
```

| Elemento | Descrição |
|----------|-----------|
| **Data/AppDbContext** | Contexto EF Core para mapeamento objeto-relacional |
| **Repositories/** | Implementações que satisfazem interfaces do Domínio (`SorteioRepository`) |
| **Migrations/** | Scripts de versionamento de schema do banco |

**Exemplo — Repositório (implementação concreta):**
```csharp
// SenaPro.Infrastructure/Repositories/SorteioRepository.cs
public class SorteioRepository : ISorteioRepository
{
    private readonly AppDbContext _context;  // Injeta DbContext
    
    public async Task<List<Sorteio>> ObterTodosAsync(...)
    {
        return await _context.Sorteios
            .OrderBy(s => s.Concurso)
            .ToListAsync(cancellationToken);
    }
}
```

---

### 4. `SenaPro.API` — Camada de Apresentação (Host)

**Responsabilidade:** Host da API REST, controllers, configuração de DI e Swagger. Depende de todas as outras camadas.

```
SenaPro.API/
├── Controllers/           ← Endpoints HTTP (SorteiosController, GeradorController)
├── Program.cs             ← Configuração de DI, middleware, routing
└── Dockerfile             ← Build multi-stage para produção
```

| Elemento | Descrição |
|----------|-----------|
| **Controllers** | Handlers de rotas HTTP que delegam para services (`SorteiosController`, `GeradorController`) |
| **Program.cs** | Ponto de entrada — registra DI, middleware e configura a aplicação |

**Exemplo — Controller:**
```csharp
// SenaPro.API/Controllers/SorteiosController.cs
[ApiController]
[Route("api/[controller]")]
public class SorteiosController : ControllerBase
{
    private readonly IExcelImportService _excelImportService;  // Injeta via interface
    
    public async Task<IActionResult> ImportarExcel(IFormFile file, ...)
    {
        var resultado = await _excelImportService.ImportarAsync(...);
        return Ok(resultado);
    }
}
```

---

## Camada de Frontend (Angular)

### Estrutura

```
sena-pro-frontend/src/app/
├── models/                ← Interfaces TypeScript (senapro.models.ts)
├── services/              ← Serviço HTTP centralizado (SenaProService)
└── pages/                 ← Páginas/Componentes de UI
    ├── home/              ← Página inicial
    ├── sorteios/          ← Listagem de sorteios
    └── gerador/           ← Gerador inteligente de jogos
```

### Padrões Aplicados

| Padrão | Descrição |
|--------|-----------|
| **Service Layer (HTTP)** | `SenaProService` encapsula toda comunicação com o backend em um único lugar |
| **Typed Models** | Interfaces TypeScript definem contratos com o backend (`ImportacaoResultado`, `ConfiguracaoGeracaoJogos`) |
| **Dependency Injection** | Angular DI injeta `HttpClient` e serviços via `providedIn: 'root'` |
| **Observables (RxJS)** | Comunicação assíncrona reativa com streams do Angular |

---

## Práticas de Desenvolvimento (TDD)

O projeto segue **Test-Driven Development (TDD)** como metodologia principal: ciclo Red → Green → Refactor. Testes são escritos antes da implementação, o que molda a arquitetura (interfaces definidas primeiro, dependências invertidas).

**Documento detalhado:** [development-practices.md](development-practices.md) — workflow TDD, estrutura de testes, estratégias por camada, convenções de nomenclatura e CI/CD.

---

## Padrões de Design Aplicados

### 1. **Repository Pattern**

Abstrai o acesso a dados por trás de uma interface, permitindo troca de implementação sem alterar código de negócio.

```
ISorteioRepository (Domain)
       │
       ▼
SorteioRepository (Infrastructure — EF Core)
```

**Benefícios:**
- Testabilidade: mock do repositório em testes unitários
- Troca de storage: trocar PostgreSQL por outro DB não altera regras de negócio
- Single Responsibility: lógica de query isolada em uma classe

---

### 2. **Dependency Injection (DI)**

Controle de inversão de dependência via container nativo do ASP.NET Core.

**Registro em `Program.cs`:**
```csharp
builder.Services.AddScoped<ISorteioRepository, SorteioRepository>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddScoped<IGeradorJogosService, GeradorJogosService>();
```

**Escopo:** `Scoped` — uma instância por request HTTP (compartilhada entre controllers e services no mesmo request).

---

### 3. **DTO / Result Objects**

Objetos de transferência de dados entre camadas, evitando expor entidades de domínio diretamente na API.

| Classe | Responsabilidade |
|--------|-----------------|
| `ImportacaoResultado` | Retorna status da importação (sucesso, erros, contadores) |
| `JogoSugerido` | Representa um jogo gerado com metadados |
| `SorteioRepetidoResultado` | Agrupa combinações repetidas com contagem |

**Vantagem:** Separação entre modelo de domínio (interno) e contrato de API (externo).

---

### 4. **CQRS Lite (Command-Query Separation)**

Separação conceitual entre operações de escrita (commands) e leitura (queries), embora sem implementação formal de CQRS:

| Tipo | Exemplo |
|------|---------|
| **Command** | `POST /api/sorteios/importar-excel` — modifica estado |
| **Query** | `GET /api/sorteios/repetidos` — apenas lê dados |

---

### 5. **Multi-Stage Docker Build**

Otimização de imagens de contêiner com stages separados:

```dockerfile
# Stage 1: Build (SDK completo)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN dotnet restore && dotnet build -c Release

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime (imagem mínima)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SenaPro.API.dll"]
```

**Benefício:** Imagem final menor (~150MB vs ~3GB com SDK).

---

---

## Fluxo de Dados Típico

### Exemplo: Importar Excel

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐     ┌──────────────┐
│  Frontend   │────►│  Controller  │────►│    Service      │────►│ Repository   │
│ (Angular)   │◄────│ (API Layer)  │◄────│ (Application)   │◄────│ (EF Core)    │
└─────────────┘     └──────────────┘     └─────────────────┘     └──────────────┘
       │                   │                    │                        │
       │  HTTP POST        │                    │                        │
       │  /api/sorteios/   │                    │                        │
       │  importar-excel   │                    │                        │
       │                   │                    │                        │
       └───────────────────┴────────────────────┼────────────────────────┘
                                                ▼
                                     ┌──────────────────┐
                                     │   PostgreSQL     │
                                     │   (contêiner)    │
                                     └──────────────────┘
```

**Passos:**
1. Frontend envia `POST /api/sorteios/importar-excel` com arquivo Excel
2. Controller valida formato, salva temporariamente e delega para `ExcelImportService`
3. Service lê Excel, valida colunas, detecta duplicatas (arquivo + banco), persiste via `ISorteioRepository`
4. Repository usa EF Core para batch insert no PostgreSQL
5. Resultado (`ImportacaoResultado`) retorna ao frontend via JSON

---

## Decisões Arquiteturais

### 1. Por que Clean Architecture?

| Critério | Escolha | Justificativa |
|----------|---------|---------------|
| **Testabilidade** | Camada de Domínio pura | Permite testes unitários sem mocks pesados |
| **Manutenibilidade** | Separação clara de responsabilidades | Novos desenvolvedores entendem onde cada código vive |
| **Evolução** | Dependências direcionadas para dentro | Trocar framework/DB não altera regras de negócio |

### 2. Por que Repository Pattern?

- Abstrai complexidade de query do EF Core
- Permite substituir storage sem alterar services
- Facilita testes com mocks leves

### 3. Por que Service Layer separado?

- Orquestra múltiplos repositórios/operacões
- Centraliza regras de negócio que não pertencem a uma única entidade
- Separa lógica de aplicação de lógica de apresentação (controllers)

---

## Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────┐
│                    Dependências entre Camadas               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   SenaPro.API ─────────────────────────────────► .          │
│       ├─► SenaPro.Application                         │     │
│       ├─► SenaPro.Infrastructure                      │     │
│       └─► SenaPro.Domain (interfaces)                 │     │
│                                                         │
│   SenaPro.Application ───────────────────────────────► .  │
│       └─► SenaPro.Domain                              │     │
│                                                         │
│   SenaPro.Infrastructure ──────────────────────────► .  │
│       ├─► SenaPro.Domain (interfaces + entities)      │     │
│       └─► SenaPro.Application (DTOs, se necessário)   │     │
│                                                         │
│   SenaPro.Domain ◄─────────────────────────────────────┘
│       (não depende de nenhuma outra camada)
│
└─────────────────────────────────────────────────────────────┘
```

---

## Convenções de Nomenclatura

### Projetos (.NET)

| Camada | Padrão | Exemplo |
|--------|--------|---------|
| API Host | `{NomeProjeto}.API` | `SenaPro.API` |
| Aplicação | `{NomeProjeto}.Application` | `SenaPro.Application` |
| Domínio | `{NomeProjeto}.Domain` | `SenaPro.Domain` |
| Infraestrutura | `{NomeProjeto}.Infrastructure` | `SenaPro.Infrastructure` |
| Testes | `{NomeProjeto}.Tests` | `SenaPro.Tests` |

### Nomespaces

```
SenaPro.API.Controllers
SenaPro.Application.Services
SenaPro.Domain.Entities
SenaPro.Domain.Interfaces
SenaPro.Domain.Results
SenaPro.Infrastructure.Data
SenaPro.Infrastructure.Repositories
SenaPro.Tests.Services
SenaPro.Tests.Repositories
```

---

## Resumo Visual da Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER              │  PATTERNS APPLIED                     │
├─────────────────────┼───────────────────────────────────────┤
│  Architecture       │  Clean Architecture (Layered)         │
│  Backend            │  ASP.NET Core Web API                 │
│  Frontend           │  Angular 21 + RxJS                    │
│  Database           │  PostgreSQL 16 + EF Core              │
│  DI Container       │  Microsoft.Extensions.DependencyInjection│
│  ORM                │  Entity Framework Core 8.0            │
│  Repository         │  Custom (ISorteioRepository)          │
│  DTOs               │  Result objects (ImportacaoResultado) │
│  Containers         │  Docker Compose (multi-stage build)   │
│  API Docs           │  Swagger / Swashbuckle                │
└─────────────────────────────────────────────────────────────┘
```

**Nota:** Práticas de teste (TDD, xUnit, Moq, Testcontainers) estão documentadas em [development-practices.md](development-practices.md).

---

## Relação com Outros Documentos de Tech

| Documento | Foco | Quando consultar |
|-----------|------|------------------|
| **[development-practices.md](development-practices.md)** | Práticas de desenvolvimento (TDD, workflow, testes) | Quer entender **como** o código é escrito e testado |
| **[tech.md](tech.md)** | Stack tecnológico (tecnologias, versões, ferramentas) | Quer saber **quais** tecnologias são usadas |
| **[ui-ux.md](ui-ux.md)** | Sistema de design, layout e padrões de interface | Quer entender **visual** e **interações** da aplicação |

---

## Histórico de Alterações

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-08-05 | v1.1 | Separação de práticas de desenvolvimento (TDD) para `development-practices.md` |
| 2026-08-05 | v1.0 | Criação do documento com arquitetura atual do projeto |
