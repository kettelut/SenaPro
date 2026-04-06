using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;
using SenaPro.Infrastructure.Data;
using Xunit;

namespace SenaPro.Tests.Services;

/// <summary>
/// Testes para o serviço de geração de sugestões de jogos.
/// Seguindo TDD: Red -> Green -> Refactor
/// </summary>
public class GeradorJogosServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IGeradorJogosService _geradorService;

    public GeradorJogosServiceTests()
    {
        // Configuração do banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // TODO: Implementar GeradorJogosService e injetar aqui
        // _geradorService = new GeradorJogosService(_context, _analiseService);
        _geradorService = null!; // Falha proposital - Red phase
    }

    #region Testes de Geração Básica

    [Fact]
    public async Task GerarJogosAsync_DeveRetornarSucesso()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Sucesso.Should().BeTrue("a geração deve ser bem-sucedida");
        resultado.Erros.Should().BeEmpty("não deve haver erros");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarQuantidadeSolicitada()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 5,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Jogos.Should().HaveCount(5, "deve gerar a quantidade solicitada");
        resultado.QuantidadeGerada.Should().Be(5);
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarJogosComQuantidadeCorretaDeNumeros()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 3,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        foreach (var jogo in resultado.Jogos)
        {
            jogo.Dezenas.Should().HaveCount(6, "cada jogo deve ter 6 números");
        }
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarNumerosNoIntervaloValido()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 10,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        foreach (var jogo in resultado.Jogos)
        {
            jogo.Dezenas.All(d => d >= 1 && d <= 60).Should().BeTrue("números devem estar entre 1 e 60");
        }
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarNumerosSemRepeticao()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 10,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        foreach (var jogo in resultado.Jogos)
        {
            jogo.Dezenas.Distinct().Should().HaveCount(6, "não deve haver números repetidos no mesmo jogo");
        }
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarNumerosOrdenados()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 10,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        foreach (var jogo in resultado.Jogos)
        {
            jogo.Dezenas.Should().BeInAscendingOrder("dezenas devem estar ordenadas");
        }
    }

    #endregion

    #region Testes de Configuração

    [Fact]
    public async Task GerarJogosAsync_DeveValidarQuantidadeMinimaNumeros()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1,
            QuantidadeNumeros = 5 // Inválido - mínimo 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Sucesso.Should().BeFalse("configuração inválida deve falhar");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveValidarQuantidadeMaximaNumeros()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1,
            QuantidadeNumeros = 16 // Inválido - máximo 15
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Sucesso.Should().BeFalse("configuração inválida deve falhar");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveValidarQuantidadeJogos()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 0, // Inválido
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Sucesso.Should().BeFalse("configuração inválida deve falhar");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveRetornarConfiguracaoUtilizada()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 3,
            QuantidadeNumeros = 6,
            AnalisesRespeitadas = new List<string> { "SorteiosRepetidos" }
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Configuracao.Should().NotBeNull();
        resultado.Configuracao.QuantidadeJogos.Should().Be(3);
        resultado.Configuracao.QuantidadeNumeros.Should().Be(6);
    }

    #endregion

    #region Testes de Análise - Sorteios Repetidos

    [Fact]
    public async Task GerarJogosAsync_ComAnaliseSorteiosRepetidos_NaoDeveGerarJogosIguaisAoHistorico()
    {
        // Arrange - Adiciona sorteio histórico
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 10,
            Dezena2 = 20,
            Dezena3 = 30,
            Dezena4 = 40,
            Dezena5 = 50,
            Dezena6 = 60
        });
        await _context.SaveChangesAsync();

        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 100,
            QuantidadeNumeros = 6,
            AnalisesRespeitadas = new List<string> { "SorteiosRepetidos" }
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        var jogoProibido = new byte[] { 10, 20, 30, 40, 50, 60 };
        resultado.Jogos.Should().NotContain(j => j.Dezenas.SequenceEqual(jogoProibido),
            "não deve gerar jogo igual ao histórico quando análise SorteiosRepetidos está ativa");
    }

    [Fact]
    public async Task GerarJogosAsync_SemAnaliseSorteiosRepetidos_PodeGerarJogosIguaisAoHistorico()
    {
        // Arrange - Adiciona sorteio histórico
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 5,
            Dezena2 = 10,
            Dezena3 = 15,
            Dezena4 = 20,
            Dezena5 = 25,
            Dezena6 = 30
        });
        await _context.SaveChangesAsync();

        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 100,
            QuantidadeNumeros = 6,
            AnalisesRespeitadas = new List<string>() // Sem análises
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        // Sem a análise, pode gerar qualquer combinação (inclusive igual ao histórico)
        resultado.Sucesso.Should().BeTrue("sem restrições, deve gerar normalmente");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarJogosDistintos()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 50,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        var jogosDistintos = resultado.Jogos
            .Select(j => string.Join(",", j.Dezenas.OrderBy(d => d)))
            .Distinct()
            .Count();

        jogosDistintos.Should().Be(50, "todos os jogos devem ser únicos");
    }

    #endregion

    #region Testes de Obter Análises Disponíveis

    [Fact]
    public async Task ObterAnalisesDisponiveisAsync_DeveRetornarLista()
    {
        // Act
        var analises = await _geradorService.ObterAnalisesDisponiveisAsync();

        // Assert
        analises.Should().NotBeEmpty("deve haver análises disponíveis");
    }

    [Fact]
    public async Task ObterAnalisesDisponiveisAsync_DeveConterAnaliseSorteiosRepetidos()
    {
        // Act
        var analises = await _geradorService.ObterAnalisesDisponiveisAsync();

        // Assert
        analises.Should().Contain("SorteiosRepetidos", "análise de sorteios repetidos deve estar disponível");
    }

    #endregion

    #region Testes de Casos de Borda

    [Fact]
    public async Task GerarJogosAsync_DeveGerarJogosComDataGeracao()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1,
            QuantidadeNumeros = 6
        };

        var antes = DateTime.UtcNow;

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        var depois = DateTime.UtcNow;
        foreach (var jogo in resultado.Jogos)
        {
            jogo.DataGeracao.Should().BeOnOrAfter(antes);
            jogo.DataGeracao.Should().BeOnOrBefore(depois);
        }
    }

    [Fact]
    public async Task GerarJogosAsync_DeveGerarJogosComIdsUnicos()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 100,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        var ids = resultado.Jogos.Select(j => j.Id).ToList();
        ids.Distinct().Should().HaveCount(100, "todos os IDs devem ser únicos");
    }

    [Fact]
    public async Task GerarJogosAsync_DeveRetornarMensagemInformativa()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1,
            QuantidadeNumeros = 6
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Mensagem.Should().NotBeNullOrEmpty("deve conter mensagem informativa");
    }

    [Fact]
    public async Task GerarJogosAsync_ComMuitosJogos_DeveGerarEficientemente()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 1000,
            QuantidadeNumeros = 6
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var resultado = await _geradorService.GerarJogosAsync(config);
        stopwatch.Stop();

        // Assert
        resultado.Sucesso.Should().BeTrue("deve processar muitos jogos");
        resultado.Jogos.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "deve gerar em tempo razoável");
    }

    [Fact]
    public async Task GerarJogosAsync_ComQuantidadeNumerosMaiorQueSeis_DeveGerarCorretamente()
    {
        // Arrange
        var config = new ConfiguracaoGeracaoJogos
        {
            QuantidadeJogos = 5,
            QuantidadeNumeros = 10 // Para Surpresinha com mais números
        };

        // Act
        var resultado = await _geradorService.GerarJogosAsync(config);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        foreach (var jogo in resultado.Jogos)
        {
            jogo.Dezenas.Should().HaveCount(10);
            jogo.Dezenas.Distinct().Should().HaveCount(10, "não deve ter repetidos");
            jogo.Dezenas.Should().BeInAscendingOrder();
        }
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}