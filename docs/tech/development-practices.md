# Práticas de Desenvolvimento — SenaPro

Documento que define as práticas de desenvolvimento, metodologia e workflow aplicados no projeto **SenaPro**. Focado em **como** o código é escrito, testado e evoluído.

---

## Relação com Outros Documentos de Tech

| Documento | Foco | Quando consultar |
|-----------|------|------------------|
| **[architecture.md](architecture.md)** | Arquitetura (camadas, padrões de design, dependências) | Quer entender **estrutura** e **decisões arquiteturais** |
| **[tech.md](tech.md)** | Stack tecnológico (tecnologias, versões, ferramentas) | Quer saber **quais** tecnologias são usadas |
| **[ui-ux.md](ui-ux.md)** | Sistema de design, layout e padrões de interface | Quer entender **visual** e **interações** da aplicação |

---

## Metodologia Principal: **TDD (Test-Driven Development)**

O projeto segue rigorosamente o ciclo **Red → Green → Refactor** como prática padrão de desenvolvimento. Cada feature começa com um teste falhando, seguido pela implementação mínima para fazê-lo passar, e então refatoração com segurança.

### Ciclo TDD

```
┌─────────────────────────────────────────────────────────────┐
│                    CICLO TDD                                │
│                                                             │
│   ┌──────────┐    ┌──────────┐    ┌──────────────────┐     │
│   │  RED     │───►│  GREEN   │───►│   REFACTOR       │     │
│   │ (Failing)│    │ (Passing)│    │  (Clean Code)    │     │
│   └──────────┘    └──────────┘    └──────────────────┘     │
│        ▲                                 │                 │
│        │                                 ▼                 │
│        └──────────── Teste passa ✓ ──────────┘             │
└─────────────────────────────────────────────────────────────┘
```

#### 1. **Red** — Escrever teste falhando

- Antes de qualquer implementação, escreva um teste que **deve falhar**.
- O teste deve ser pequeno, focado e expressar uma expectativa clara.
- Se o teste passa sem código novo, ele não está testando nada útil.

```csharp
// Exemplo: Teste antes da implementação
[Fact]
public async Task ImportarAsync_ComArquivoValido_DeveRetornarSucesso()
{
    // Arrange
    var repository = new Mock<ISorteioRepository>();
    var service = new ExcelImportService(repository.Object);
    
    // Act
    var resultado = await service.ImportarAsync("Mega-Sena-Example.xlsx");
    
    // Assert
    resultado.Sucesso.Should().BeTrue();
    resultado.RegistrosInseridos.Should().BeGreaterThan(0);
}
```

#### 2. **Green** — Implementação mínima para passar

- Escreva o código **mínimo necessário** para o teste passar.
- Não adie refatoração: se funciona e passa no teste, está pronto.
- Evite over-engineering nesta fase.

```csharp
// Implementação mínima
public async Task<ImportacaoResultado> ImportarAsync(string caminhoArquivo)
{
    var resultado = new ImportacaoResultado();
    
    if (!File.Exists(caminhoArquivo))
    {
        resultado.Sucesso = false;
        resultado.Erros.Add("Arquivo não encontrado");
        return resultado;
    }
    
    // TODO: implementar parsing completo
    
    resultado.Sucesso = true;
    return resultado;
}
```

#### 3. **Refactor** — Melhorar código com segurança

- Com testes passando, refatore livremente.
- Mantenha a cobertura de testes intacta.
- Remova duplicação, melhora nomes, simplifica lógica.

---

## Workflow de Desenvolvimento

### Sequência Típica de Implementação

1. **Entender o requisito** — Ler user story / feature documentada
2. **Escrever teste falhando** — Definir comportamento esperado via teste
3. **Implementar mínimo** — Código suficiente para passar no teste
4. **Rodar testes** — Garantir que tudo passa (`dotnet test`)
5. **Refatorar** — Melhorar código com segurança dos testes
6. **Commit** — Mensagem clara referenciando a feature (ex.: `feat: adicionar importação Excel [FE-001]`)

### Regra de Ouro

> **"Não escreva uma linha de produção sem um teste falhando primeiro."**

Exceções (raras):
- Configuração de infraestrutura (Docker, migrations)
- Scripts de build/deploy
- Documentação

---

## Estrutura de Testes

### Camada de Testes: `SenaPro.Tests`

```
SenaPro.Tests/
├── Services/              ← Testes unitários de services
│   ├── ExcelImportServiceTests.cs
│   ├── AnaliseEstatisticaServiceTests.cs
│   └── GeradorJogosServiceTests.cs
└── Repositories/          ← Testes de integração de repositórios
    ├── BaseIntegrationTests.cs
    └── SorteioRepositoryTests.cs
```

### Tipos de Teste

| Tipo | Localização | Objetivo | Exemplo |
|------|-------------|----------|---------|
| **Unitário** | `SenaPro.Tests/Services/` | Testar lógica isolada sem dependências externas | `ExcelImportServiceTests` |
| **Integração** | `SenaPro.Tests/Repositories/` | Testar interação com banco real (Testcontainers) | `SorteioRepositoryTests` |

---

### Testes Unitários

**Framework:** xUnit + Moq + FluentAssertions

**Características:**
- Isolam a unidade de código (service, classe de domínio)
- Mock de dependências externas (`ISorteioRepository`, `IExcelImportService`)
- Rápido execução (segundos)
- Não acessam banco de dados, sistema de arquivos, APIs externas

**Exemplo — Estrutura de Teste Unitário:**

```csharp
public class ExcelImportServiceTests
{
    private readonly Mock<ISorteioRepository> _mockRepository;
    private readonly ExcelImportService _service;

    public ExcelImportServiceTests()
    {
        _mockRepository = new Mock<ISorteioRepository>();
        _service = new ExcelImportService(_mockRepository.Object);
    }

    [Fact]
    public async Task ImportarAsync_ComArquivoInexistente_DeveRetornarErro()
    {
        // Arrange
        var caminhoInexistente = Path.Combine(Path.GetTempPath(), "inexistente.xlsx");

        // Act
        var resultado = await _service.ImportarAsync(caminhoInexistente);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Erros.Should().Contain(e => e.Contains("não encontrado"));
    }
}
```

---

### Testes de Integração com Testcontainers

**Framework:** Testcontainers.PostgreSql + EF Core InMemory (alternativo)

**Características:**
- Usam PostgreSQL real em contêiner efêmero
- Testam interação real com banco de dados
- Lento execução (segundos a minutos)
- Valida migrations, queries complexas, constraints

**Exemplo — Base para Testes de Integração:**

```csharp
public class BaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new("postgres:16-alpine");
    protected AppDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        
        Context = new AppDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgres.StopAsync();
    }
}
```

---

## Como TDD Influencia a Arquitetura

A escolha de seguir TDD **molda diretamente** a arquitetura do projeto:

### 1. **Interfaces Definidas Antes das Implementações**

Em vez de escrever a implementação e depois pensar em como testar, escrevemos os testes primeiro — o que nos força a definir **contratos limpos** (interfaces) antes da lógica concreta.

```csharp
// Interface definida PRIMEIRO (para teste)
public interface IExcelImportService
{
    Task<ImportacaoResultado> ImportarAsync(string caminhoArquivo, CancellationToken ct = default);
}

// Implementação escrita DEPOIS (para passar no teste)
public class ExcelImportService : IExcelImportService { ... }
```

### 2. **Dependência de Abstrações, Não de Concretos**

Testes unitários exigem isolamento — não podemos depender de PostgreSQL, sistema de arquivos, etc. Isso nos leva naturalmente ao **Dependency Inversion**:

- `SenaPro.Domain` define interfaces (`ISorteioRepository`)
- `SenaPro.Infrastructure` implementa concretos (`SorteioRepository`)
- Testes mockam interfaces, não implementações

### 3. **Classes Pequenas e Focadas**

Testar classes grandes é difícil → escrevemos classes pequenas com uma responsabilidade clara:

| Camada | Responsabilidade Única |
|--------|----------------------|
| `SenaPro.Domain.Entities` | Modelos de domínio puros |
| `SenaPro.Application.Services` | Orquestração de casos de uso |
| `SenaPro.Infrastructure.Repositories` | Acesso a dados concreto |

### 4. **Result Objects vs. Exceções**

Para operações que podem falhar de forma esperada (importação, geração), usamos objetos de resultado (`ImportacaoResultado`) em vez de exceções — mais testável e expressivo:

```csharp
// Anti-pattern: exceção para fluxo normal
throw new InvalidOperationException("Arquivo inválido");

// Pattern TDD-friendly: resultado explícito
return new ImportacaoResultado { Sucesso = false, Erros = ["Arquivo inválido"] };
```

---

## Estratégias de Teste por Camada

### Domain Layer (90% unitários)

- **O que testar:** Regras de negócio puras, validações, cálculos
- **Como:** Instanciar classes diretamente, mockar apenas dependências externas
- **Exemplo:** Testar que `Sorteio.GetDezenas()` retorna ordenado

### Application Layer (70% unitários, 30% integração)

- **O que testar:** Orquestração de casos de uso, validações de negócio
- **Como:** Mockar repositórios e services externos
- **Exemplo:** Testar que `ExcelImportService.ImportarAsync` chama repository corretamente

### Infrastructure Layer (100% integração)

- **O que testar:** Interação com banco, leitura de Excel, integrações externas
- **Como:** Testcontainers para PostgreSQL, arquivos reais no sistema de testes
- **Exemplo:** Testar que `SorteioRepository.AdicionarVariosAsync` persiste no DB real

### API Layer (testes de integração/end-to-end)

- **O que testar:** Rotas HTTP, serialização, autenticação (se aplicável)
- **Como:** `Microsoft.AspNetCore.Mvc.Testing` + HttpClient fake
- **Exemplo:** Testar que `POST /api/sorteios/importar-excel` retorna 200 OK

---

## Cobertura e Qualidade

### Meta de Cobertura

| Camada | Meta de Coverage (%) | Justificativa |
|--------|---------------------|---------------|
| Domain | ≥ 95% | Regras críticas, alto risco de bugs |
| Application | ≥ 85% | Orquestração complexa, múltiplos caminhos |
| Infrastructure | ≥ 70% | Interação com externo, mais lento para testar |
| API | ≥ 60% | Rotas simples, foco em integração |

### Ferramentas de Medição

- **coverlet.collector** — Coleta automática de cobertura durante `dotnet test`
- **Relatório HTML** — Gerado em `SenaPro.Tests/TestResults/` após execução

---

## Convenções de Nomenclatura de Testes

### Classe de Teste

```csharp
// Nome: {ClasseSobTeste}Tests
public class ExcelImportServiceTests { ... }
public class SorteioRepositoryTests { ... }
```

### Método de Teste

```csharp
// Formato: [NomeDoCenario]_Quando[Condição]_Deve[ResultadoEsperado]
[Fact]
public async Task ImportarAsync_ComArquivoValido_DeveRetornarSucesso() { ... }

[Fact]
public async Task ImportarAsync_ComColunasInvalidas_DeveRetornarErro() { ... }

[Fact]
public async Task ObterTodosAsync_ComRegistros_DeveRetornarOrdenadoPorConcurso() { ... }
```

---

## Fluxo de CI/CD com Testes

```
┌─────────────────────────────────────────────────────────────┐
│                    PIPELINE DE CONTÍNUA INTEGRATION         │
│                                                             │
│  Commit → Build → Testes Unitários → Testes Integração →    │
│       ↓                                                       │
│  Reporte de Cobertura → Deploy (se tudo passar)             │
└─────────────────────────────────────────────────────────────┘
```

### Comando Local para Rodar Todos os Testes

```bash
# Todos os testes (unitários + integração)
dotnet test SenaPro.Tests

# Apenas unitários (mais rápido)
dotnet test SenaPro.Tests --filter "Category=Unit"

# Apenas integração
dotnet test SenaPro.Tests --filter "Category=Integration"
```

---

## Referências Cruzadas

| Documento | Relação |
|-----------|---------|
| [architecture.md](architecture.md) | Define **O QUE** o sistema é (camadas, padrões, dependências) |
| [tech.md](tech.md) | Lista **COMO** está implementado (tecnologias, versões) |

---

## Resumo Visual das Práticas

```
┌─────────────────────────────────────────────────────────────┐
│  PRÁTICA              │  FERRAMENTAS / TECNOLOGIAS          │
├───────────────────────┼─────────────────────────────────────┤
│  Metodologia          │  TDD (Red → Green → Refactor)       │
│  Framework de Teste   │  xUnit 2.5.3                        │
│  Mocking              │  Moq 4.20.72                        │
│  Asserções            │  FluentAssertions 8.2.0             │
│  Cobertura            │  coverlet.collector 6.0.0           │
│  Testcontainers       │  Testcontainers.PostgreSql 4.13.0   │
│  Testes Integração    │  Microsoft.AspNetCore.Mvc.Testing   │
└─────────────────────────────────────────────────────────────┘
```

---

## Histórico de Alterações

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-08-05 | v1.0 | Criação do documento com práticas de desenvolvimento e TDD |
