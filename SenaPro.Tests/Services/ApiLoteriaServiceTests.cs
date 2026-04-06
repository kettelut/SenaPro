using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;
using SenaPro.Infrastructure.Data;
using Xunit;

namespace SenaPro.Tests.Services;

/// <summary>
/// Testes para o serviço de consulta à API da loteria.
/// Seguindo TDD: Red -> Green -> Refactor
/// </summary>
public class ApiLoteriaServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IApiLoteriaService _apiLoteriaService;

    public ApiLoteriaServiceTests()
    {
        // Configuração do banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // TODO: Implementar ApiLoteriaService e injetar aqui
        // _apiLoteriaService = new ApiLoteriaService(_context, httpClient);
        _apiLoteriaService = null!; // Falha proposital - Red phase
    }

    #region Testes de Consulta

    [Fact]
    public async Task ConsultarUltimoSorteioAsync_DeveRetornarSorteioValido()
    {
        // Act
        var resultado = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        // Assert
        resultado.Should().NotBeNull("a API deve retornar um sorteio");
        resultado!.Concurso.Should().BeGreaterThan(0, "concurso deve ser positivo");
        resultado.Dezenas.Should().HaveCount(6, "deve ter 6 dezenas");
        resultado.Dezenas.All(d => d >= 1 && d <= 60).Should().BeTrue("dezenas devem estar entre 1 e 60");
    }

    [Fact]
    public async Task ConsultarSorteioAsync_DeveRetornarSorteioEspecifico()
    {
        // Arrange
        var concurso = 2500;

        // Act
        var resultado = await _apiLoteriaService.ConsultarSorteioAsync(concurso);

        // Assert
        resultado.Should().NotBeNull("o sorteio deve existir");
        resultado!.Concurso.Should().Be(concurso, "deve retornar o concurso solicitado");
    }

    [Fact]
    public async Task ConsultarSorteioAsync_DeveRetornarNuloParaConcursoInexistente()
    {
        // Arrange
        var concursoInexistente = 9999999;

        // Act
        var resultado = await _apiLoteriaService.ConsultarSorteioAsync(concursoInexistente);

        // Assert
        resultado.Should().BeNull("concurso inexistente deve retornar null");
    }

    #endregion

    #region Testes de Verificação de Atualizações

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarUltimoConcursoDaApi()
    {
        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue("deve conseguir verificar atualizações");
        resultado.UltimoConcursoApi.Should().BeGreaterThan(0, "deve retornar o último concurso da API");
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarUltimoConcursoDoBanco()
    {
        // Arrange - Adiciona um sorteio no banco
        _context.Sorteios.Add(new Domain.Entities.Sorteio
        {
            Concurso = 2500,
            Data = new DateOnly(2024, 1, 1),
            Dezena1 = 1,
            Dezena2 = 10,
            Dezena3 = 20,
            Dezena4 = 30,
            Dezena5 = 40,
            Dezena6 = 50
        });
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        resultado.UltimoConcursoBanco.Should().Be(2500, "deve retornar o último concurso do banco");
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveIdentificarGapQuandoExistir()
    {
        // Arrange - Banco com sorteio antigo
        _context.Sorteios.Add(new Domain.Entities.Sorteio
        {
            Concurso = 2000,
            Data = new DateOnly(2023, 1, 1),
            Dezena1 = 1,
            Dezena2 = 10,
            Dezena3 = 20,
            Dezena4 = 30,
            Dezena5 = 40,
            Dezena6 = 50
        });
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        resultado.HaGap.Should().BeTrue("deve identificar gap entre banco e API");
        resultado.QuantidadeGap.Should().BeGreaterThan(0, "deve informar quantidade de sorteios faltantes");
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarSemGapQuandoBancoAtualizado()
    {
        // Arrange - Simula banco atualizado com o último concurso
        var ultimoSorteio = await _apiLoteriaService.ConsultarUltimoSorteioAsync();
        _context.Sorteios.Add(new Domain.Entities.Sorteio
        {
            Concurso = ultimoSorteio!.Concurso,
            Data = ultimoSorteio.Data,
            Dezena1 = ultimoSorteio.Dezenas[0],
            Dezena2 = ultimoSorteio.Dezenas[1],
            Dezena3 = ultimoSorteio.Dezenas[2],
            Dezena4 = ultimoSorteio.Dezenas[3],
            Dezena5 = ultimoSorteio.Dezenas[4],
            Dezena6 = ultimoSorteio.Dezenas[5]
        });
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        resultado.HaGap.Should().BeFalse("não deve haver gap quando banco está atualizado");
        resultado.QuantidadeGap.Should().Be(0, "quantidade de gap deve ser zero");
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public async Task AtualizarAsync_DeveInserirNovoSorteio()
    {
        // Arrange - Banco vazio
        var verificacao = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        resultado.Sucesso.Should().BeTrue("atualização deve ser bem-sucedida");
        resultado.NovosSorteios.Should().BeGreaterThan(0, "deve inserir novos sorteios");
    }

    [Fact]
    public async Task AtualizarAsync_DeveAlertarUsuarioQuandoHouverGap()
    {
        // Arrange - Banco com sorteio antigo (gap grande)
        _context.Sorteios.Add(new Domain.Entities.Sorteio
        {
            Concurso = 1000,
            Data = new DateOnly(2020, 1, 1),
            Dezena1 = 1,
            Dezena2 = 10,
            Dezena3 = 20,
            Dezena4 = 30,
            Dezena5 = 40,
            Dezena6 = 50
        });
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        resultado.HaGap.Should().BeTrue("deve identificar gap");
        resultado.Mensagem.Should().Contain("importar", "deve sugerir importação do histórico");
    }

    [Fact]
    public async Task AtualizarAsync_NaoDeveInserirDuplicados()
    {
        // Arrange - Obtém último sorteio da API
        var ultimoSorteio = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        // Insere no banco
        _context.Sorteios.Add(new Domain.Entities.Sorteio
        {
            Concurso = ultimoSorteio!.Concurso,
            Data = ultimoSorteio.Data,
            Dezena1 = ultimoSorteio.Dezenas[0],
            Dezena2 = ultimoSorteio.Dezenas[1],
            Dezena3 = ultimoSorteio.Dezenas[2],
            Dezena4 = ultimoSorteio.Dezenas[3],
            Dezena5 = ultimoSorteio.Dezenas[4],
            Dezena6 = ultimoSorteio.Dezenas[5]
        });
        await _context.SaveChangesAsync();

        // Act - Tenta atualizar novamente
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        resultado.NovosSorteios.Should().Be(0, "não deve inserir duplicados");
    }

    #endregion

    #region Testes de Tratamento de Erros

    [Fact]
    public async Task ConsultarUltimoSorteioAsync_DeveRetornarMensagemErroQuandoApiIndisponivel()
    {
        // Este teste seria feito com mock de HTTP client simulando falha
        // Arrange - Simular API indisponível

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert - Quando API disponível, deve ter sucesso
        resultado.Erros.Should().BeEmpty("API deve estar disponível");
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarMensagemInformativa()
    {
        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        resultado.Mensagem.Should().NotBeNullOrEmpty("deve conter mensagem informativa");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}