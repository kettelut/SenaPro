using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;
using SenaPro.Infrastructure.Data;
using Xunit;

namespace SenaPro.Tests.Services;

/// <summary>
/// Testes para o serviço de análise estatística.
/// Seguindo TDD: Red -> Green -> Refactor
/// </summary>
public class AnaliseEstatisticaServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IAnaliseEstatisticaService _analiseService;

    public AnaliseEstatisticaServiceTests()
    {
        // Configuração do banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // TODO: Implementar AnaliseEstatisticaService e injetar aqui
        // _analiseService = new AnaliseEstatisticaService(_context);
        _analiseService = null!; // Falha proposital - Red phase
    }

    #region Testes de Análise de Sorteios Repetidos

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveRetornarSucesso()
    {
        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue("a análise deve ser bem-sucedida");
        resultado.Erros.Should().BeEmpty("não deve haver erros");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveRetornarNaoExistemQuandoBancoVazio()
    {
        // Arrange - Banco vazio

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.ExistemRepetidos.Should().BeFalse("banco vazio não tem sorteios repetidos");
        resultado.QuantidadePares.Should().Be(0, "quantidade de pares deve ser zero");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveIdentificarSorteiosRepetidos()
    {
        // Arrange - Dois sorteios com as mesmas dezenas
        var dezenas = new byte[] { 4, 15, 23, 33, 42, 51 };

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 4,
            Dezena2 = 15,
            Dezena3 = 23,
            Dezena4 = 33,
            Dezena5 = 42,
            Dezena6 = 51
        });

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2022, 1, 1),
            Dezena1 = 4,
            Dezena2 = 15,
            Dezena3 = 23,
            Dezena4 = 33,
            Dezena5 = 42,
            Dezena6 = 51
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.ExistemRepetidos.Should().BeTrue("existem sorteios com as mesmas dezenas");
        resultado.QuantidadePares.Should().Be(1, "deve encontrar 1 par de sorteios repetidos");
        resultado.Pares.Should().HaveCount(1, "deve retornar 1 par");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveIdentificarSorteiosRepetidosIndependenteDaOrdem()
    {
        // Arrange - Mesmas dezenas em ordens diferentes
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 6
        });

        // Mesmas dezenas, mas armazenadas em ordem diferente (o que não deveria acontecer,
        // mas testamos se a análise ordena antes de comparar)
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2022, 1, 1),
            Dezena1 = 6,
            Dezena2 = 5,
            Dezena3 = 4,
            Dezena4 = 3,
            Dezena5 = 2,
            Dezena6 = 1
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.ExistemRepetidos.Should().BeTrue("as dezenas são as mesmas independente da ordem");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveRetornarInformacoesDosPares()
    {
        // Arrange
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

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2022, 6, 15),
            Dezena1 = 10,
            Dezena2 = 20,
            Dezena3 = 30,
            Dezena4 = 40,
            Dezena5 = 50,
            Dezena6 = 60
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        var par = resultado.Pares.First();
        par.Concurso1.Should().Be(1000);
        par.Concurso2.Should().Be(2000);
        par.Data1.Should().Be(new DateOnly(2020, 1, 1));
        par.Data2.Should().Be(new DateOnly(2022, 6, 15));
        par.Dezenas.Should().Equal(10, 20, 30, 40, 50, 60);
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveRetornarMensagemInformativa()
    {
        // Arrange
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 6
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.Mensagem.Should().NotBeNullOrEmpty("deve conter mensagem informativa");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveEncontrarMultiplosPares()
    {
        // Arrange - Três sorteios com as mesmas dezenas (formam 3 pares únicos)
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

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2021, 1, 1),
            Dezena1 = 5,
            Dezena2 = 10,
            Dezena3 = 15,
            Dezena4 = 20,
            Dezena5 = 25,
            Dezena6 = 30
        });

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 3000,
            Data = new DateOnly(2022, 1, 1),
            Dezena1 = 5,
            Dezena2 = 10,
            Dezena3 = 15,
            Dezena4 = 20,
            Dezena5 = 25,
            Dezena6 = 30
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.QuantidadePares.Should().Be(3, "3 sorteios iguais formam 3 pares (C(3,2) = 3)");
    }

    #endregion

    #region Testes de Verificação de Dezenas

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveRetornarFalseParaBancoVazio()
    {
        // Arrange
        var dezenas = new byte[] { 1, 2, 3, 4, 5, 6 };

        // Act
        var resultado = await _analiseService.VerificarDezenasJaSorteadasAsync(dezenas);

        // Assert
        resultado.Should().BeFalse("banco vazio não tem sorteios");
    }

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveRetornarTrueQuandoExistem()
    {
        // Arrange
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 7,
            Dezena2 = 14,
            Dezena3 = 21,
            Dezena4 = 28,
            Dezena5 = 35,
            Dezena6 = 42
        });
        await _context.SaveChangesAsync();

        var dezenas = new byte[] { 7, 14, 21, 28, 35, 42 };

        // Act
        var resultado = await _analiseService.VerificarDezenasJaSorteadasAsync(dezenas);

        // Assert
        resultado.Should().BeTrue("as dezenas já foram sorteadas");
    }

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveRetornarFalseQuandoNaoExistem()
    {
        // Arrange
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 6
        });
        await _context.SaveChangesAsync();

        var dezenasNaoSorteadas = new byte[] { 10, 20, 30, 40, 50, 60 };

        // Act
        var resultado = await _analiseService.VerificarDezenasJaSorteadasAsync(dezenasNaoSorteadas);

        // Assert
        resultado.Should().BeFalse("essas dezenas não foram sorteadas");
    }

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveFuncionarIndependenteDaOrdem()
    {
        // Arrange
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 6
        });
        await _context.SaveChangesAsync();

        // Mesmas dezenas em ordem diferente
        var dezenasOrdemDiferente = new byte[] { 6, 5, 4, 3, 2, 1 };

        // Act
        var resultado = await _analiseService.VerificarDezenasJaSorteadasAsync(dezenasOrdemDiferente);

        // Assert
        resultado.Should().BeTrue("deve encontrar independente da ordem");
    }

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveValidarQuantidadeDeDezenas()
    {
        // Arrange
        var dezenasInvalidas = new byte[] { 1, 2, 3 }; // Apenas 3 dezenas

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _analiseService.VerificarDezenasJaSorteadasAsync(dezenasInvalidas));
    }

    [Fact]
    public async Task VerificarDezenasJaSorteadasAsync_DeveValidarDezenasNoIntervalo()
    {
        // Arrange
        var dezenasInvalidas = new byte[] { 0, 2, 3, 4, 5, 70 }; // 0 e 70 fora do intervalo

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _analiseService.VerificarDezenasJaSorteadasAsync(dezenasInvalidas));
    }

    #endregion

    #region Testes de Casos de Borda

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_NaoDeveConsiderarSorteiosParciais()
    {
        // Arrange - Sorteios com apenas algumas dezenas iguais
        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 6
        });

        _context.Sorteios.Add(new Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2022, 1, 1),
            Dezena1 = 1,
            Dezena2 = 2,
            Dezena3 = 3,
            Dezena4 = 4,
            Dezena5 = 5,
            Dezena6 = 10 // Última dezena diferente
        });

        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.ExistemRepetidos.Should().BeFalse("sorteios com dezenas diferentes não são repetidos");
    }

    [Fact]
    public async Task AnalisarSorteiosRepetidosAsync_DeveProcessarGrandeQuantidade()
    {
        // Arrange - Simula base com muitos sorteios (sem repetidos)
        for (int i = 1; i <= 1000; i++)
        {
            _context.Sorteios.Add(new Sorteio
            {
                Concurso = i,
                Data = new DateOnly(2020, 1, 1).AddDays(i),
                Dezena1 = (byte)(i % 60 + 1),
                Dezena2 = (byte)((i + 10) % 60 + 1),
                Dezena3 = (byte)((i + 20) % 60 + 1),
                Dezena4 = (byte)((i + 30) % 60 + 1),
                Dezena5 = (byte)((i + 40) % 60 + 1),
                Dezena6 = (byte)((i + 50) % 60 + 1)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _analiseService.AnalisarSorteiosRepetidosAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue("deve processar grande quantidade de sorteios");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}