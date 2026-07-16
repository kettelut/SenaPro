using Microsoft.EntityFrameworkCore;
using SenaPro.Application.Services;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;
using SenaPro.Infrastructure.Data;
using SenaPro.Infrastructure.Repositories;
using System.Net;
using System.Text.Json.Nodes;
using Xunit;

namespace SenaPro.Tests.Services;

/// <summary>
/// Testes para o serviço de consulta à API da loteria.
/// Seguindo TDD: Red -> Green -> Refactor
/// </summary>
public class ApiLoteriaServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ISorteioRepository _sorteioRepository;
    private readonly MockHttpMessageHandler _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiLoteriaService _apiLoteriaService;

    public ApiLoteriaServiceTests()
    {
        // Configuração do banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sorteioRepository = new SorteioRepository(_context);

        // Configura HttpClient mockável
        _httpMessageHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler);

        _apiLoteriaService = new ApiLoteriaService(_httpClient, _sorteioRepository);
    }

    #region Testes de Consulta

    [Fact]
    public async Task ConsultarUltimoSorteioAsync_DeveRetornarSorteioValido()
    {
        // Arrange - Configura resposta mock da API
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse());

        // Act
        var resultado = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado!.Concurso > 0, "concurso deve ser positivo");
        Assert.Equal(6, resultado.Dezenas.Length);
        Assert.All(resultado.Dezenas, d => Assert.True(d >= 1 && d <= 60));
    }

    [Fact]
    public async Task ConsultarSorteioAsync_DeveRetornarSorteioEspecifico()
    {
        // Arrange
        var concurso = 2500;
        _httpMessageHandler.SetResponse(Mocks.ApiSorteioResponse(concurso));

        // Act
        var resultado = await _apiLoteriaService.ConsultarSorteioAsync(concurso);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(concurso, resultado!.Concurso);
    }

    [Fact]
    public async Task ConsultarSorteioAsync_DeveRetornarNuloParaConcursoInexistente()
    {
        // Arrange
        var concursoInexistente = 9999999;
        _httpMessageHandler.SetResponse(null, HttpStatusCode.NotFound);

        // Act
        var resultado = await _apiLoteriaService.ConsultarSorteioAsync(concursoInexistente);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region Testes de Verificação de Atualizações

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarUltimoConcursoDaApi()
    {
        // Arrange
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse());

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.True(resultado.UltimoConcursoApi > 0);
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarUltimoConcursoDoBanco()
    {
        // Arrange - Adiciona um sorteio no banco
        _context.Sorteios.Add(new Sorteio
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

        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2510));

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        Assert.Equal(2500, resultado.UltimoConcursoBanco);
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveIdentificarGapQuandoExistir()
    {
        // Arrange - Banco com sorteio antigo
        _context.Sorteios.Add(new Sorteio
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

        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2020));

        // Act
        var resultado = await _apiLoteriaService.VerificarAtualizacoesAsync();

        // Assert
        Assert.True(resultado.HaGap);
        Assert.True(resultado.QuantidadeGap > 0);
    }

    [Fact]
    public async Task VerificarAtualizacoesAsync_DeveRetornarSemGapQuandoBancoAtualizado()
    {
        // Arrange - Simula banco atualizado com o último concurso
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2500));
        var ultimoSorteio = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        _context.Sorteios.Add(new Sorteio
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
        Assert.False(resultado.HaGap);
        Assert.Equal(0, resultado.QuantidadeGap);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public async Task AtualizarAsync_DeveInserirNovoSorteio()
    {
        // Arrange - Banco vazio, API com último sorteio
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2500));

        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.True(resultado.NovosSorteios > 0);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAlertarUsuarioQuandoHouverGap()
    {
        // Arrange - Banco com sorteio antigo (gap grande)
        _context.Sorteios.Add(new Sorteio
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

        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2500));

        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        Assert.True(resultado.HaGap);
        Assert.Contains("importe", resultado.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AtualizarAsync_NaoDeveInserirDuplicados()
    {
        // Arrange - Obtém último sorteio da API
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2500));
        var ultimoSorteio = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        // Insere no banco
        _context.Sorteios.Add(new Sorteio
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
        Assert.Equal(0, resultado.NovosSorteios);
    }

    #endregion

    #region Testes de Tratamento de Erros

    [Fact]
    public async Task ConsultarUltimoSorteioAsync_DeveRetornarMensagemErroQuandoApiIndisponivel()
    {
        // Arrange - Simular API indisponível
        _httpMessageHandler.SetResponse(null, HttpStatusCode.ServiceUnavailable);

        // Act
        var resultado = await _apiLoteriaService.ConsultarUltimoSorteioAsync();

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarMensagemInformativa()
    {
        // Arrange
        _httpMessageHandler.SetResponse(Mocks.ApiUltimoSorteioResponse(2500));

        // Act
        var resultado = await _apiLoteriaService.AtualizarAsync();

        // Assert
        Assert.False(string.IsNullOrEmpty(resultado.Mensagem));
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
        _httpClient.Dispose();
    }
}

/// <summary>
/// Mock de HttpMessageHandler para simular respostas da API.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private string? _responseContent;
    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    public void SetResponse(string? content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = content;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_responseContent == null || _statusCode != HttpStatusCode.OK)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}

/// <summary>
/// Mocks de respostas da API.
/// </summary>
public static class Mocks
{
    public static string ApiUltimoSorteioResponse(int concurso = 2500)
    {
        var json = new JsonObject
        {
            ["numero"] = concurso,
            ["dataApuracao"] = "15/03/2024",
            ["dezenas"] = new JsonArray { 5, 12, 23, 34, 45, 56 },
            ["acumulado"] = "false",
            ["ganhadores6Numeros"] = "1",
            ["premio6Numeros"] = "150000000,00"
        };

        return json.ToString();
    }

    public static string ApiSorteioResponse(int concurso)
    {
        var json = new JsonObject
        {
            ["numero"] = concurso,
            ["dataApuracao"] = "01/01/2024",
            ["dezenas"] = new JsonArray { 1, 10, 20, 30, 40, 50 },
            ["acumulado"] = "true",
            ["ganhadores6Numeros"] = "0",
            ["premio6Numeros"] = "50000000,00"
        };

        return json.ToString();
    }
}