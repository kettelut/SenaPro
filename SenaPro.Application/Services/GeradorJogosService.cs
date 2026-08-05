using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.Application.Services;

/// <summary>
/// Serviço de geração de sugestões de jogos da Mega-Sena.
/// </summary>
public class GeradorJogosService : IGeradorJogosService
{
    private readonly ISorteioRepository _sorteioRepository;
    private readonly IAnaliseEstatisticaService _analiseService;
    private readonly Random _random = new();

    private const int MinNumeros = 6;
    private const int MaxNumeros = 15;
    private const int MaxNumeroMega = 60;
    private const int MinNumeroMega = 1;

    private static readonly List<string> AnalisesDisponiveis = new()
    {
        "SorteiosRepetidos"
    };

    public GeradorJogosService(ISorteioRepository sorteioRepository, IAnaliseEstatisticaService analiseService)
    {
        _sorteioRepository = sorteioRepository;
        _analiseService = analiseService;
    }

    public async Task<GeracaoJogosResultado> GerarJogosAsync(ConfiguracaoGeracaoJogos configuracao, CancellationToken cancellationToken = default)
    {
        var resultado = new GeracaoJogosResultado
        {
            Configuracao = configuracao
        };

        // Validações
        var erros = ValidarConfiguracao(configuracao);
        if (erros.Count > 0)
        {
            resultado.Sucesso = false;
            resultado.Erros = erros;
            resultado.Mensagem = "Configuração inválida.";
            return resultado;
        }

        try
        {
            // Carrega sorteios históricos para análise se necessário
            var sorteiosHistoricos = new List<Domain.Entities.Sorteio>();
            if (configuracao.AnalisesRespeitadas.Any())
            {
                sorteiosHistoricos = await _sorteioRepository.ObterTodosAsync(cancellationToken);
            }

            var jogos = new List<JogoSugerido>();
            var jogosGerados = new HashSet<string>();
            var id = 1;
            int tentativasFalhasConsecutivas = 0;
            int maxTentativasSemSucesso = configuracao.QuantidadeJogos * 50;

            while (jogos.Count < configuracao.QuantidadeJogos)
            {
                var dezenas = GerarDezenasAleatorias(configuracao.QuantidadeNumeros);
                var chave = string.Join(",", dezenas);

                // Verifica se já foi gerado
                if (jogosGerados.Contains(chave))
                {
                    tentativasFalhasConsecutivas++;
                    if (tentativasFalhasConsecutivas > maxTentativasSemSucesso)
                        break;
                    continue;
                }

                tentativasFalhasConsecutivas = 0;

                // Aplica filtros das análises selecionadas
                var aprovado = true;

                if (configuracao.AnalisesRespeitadas.Contains("SorteiosRepetidos"))
                {
                    // Verifica se o jogo já foi sorteado no histórico
                    if (await EhJogoSorteado(dezenas, sorteiosHistoricos))
                        aprovado = false;
                }

                if (!aprovado)
                    continue;

                jogosGerados.Add(chave);
                jogos.Add(new JogoSugerido
                {
                    Id = id++,
                    Dezenas = dezenas,
                    DataGeracao = DateTime.UtcNow
                });
            }

            resultado.Sucesso = true;
            resultado.Jogos = jogos;
            resultado.QuantidadeGerada = jogos.Count;
            resultado.Mensagem = jogos.Count > 0
                ? $"Foram gerados {jogos.Count} jogo(s) com sucesso."
                : "Não foi possível gerar jogos dentro das restrições selecionadas. Tente reduzir o número de análises.";

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro ao gerar jogos: {ex.Message}");
            resultado.Mensagem = "Erro ao gerar jogos.";
            return resultado;
        }
    }

    public Task<List<string>> ObterAnalisesDisponiveisAsync()
    {
        return Task.FromResult(AnalisesDisponiveis);
    }

    private List<string> ValidarConfiguracao(ConfiguracaoGeracaoJogos config)
    {
        var erros = new List<string>();

        if (config.QuantidadeJogos <= 0)
            erros.Add("A quantidade de jogos deve ser maior que zero.");

        if (config.QuantidadeNumeros < MinNumeros)
            erros.Add($"A quantidade de números deve ser no mínimo {MinNumeros}.");

        if (config.QuantidadeNumeros > MaxNumeros)
            erros.Add($"A quantidade de números deve ser no máximo {MaxNumeros}.");

        return erros;
    }

    private byte[] GerarDezenasAleatorias(int quantidade)
    {
        var dezenas = new HashSet<byte>();

        while (dezenas.Count < quantidade)
        {
            var numero = (byte)_random.Next(MinNumeroMega, MaxNumeroMega + 1);
            dezenas.Add(numero);
        }

        var resultado = dezenas.ToArray();
        Array.Sort(resultado);
        return resultado;
    }

    private async Task<bool> EhJogoSorteado(byte[] dezenas, List<Domain.Entities.Sorteio> sorteiosHistoricos)
    {
        var dezenasOrdenadas = dezenas.OrderBy(d => d).ToArray();
        var chaveDezenas = string.Join(",", dezenasOrdenadas);

        foreach (var sorteio in sorteiosHistoricos)
        {
            var dezenasSorteio = new byte[]
            {
                sorteio.Dezena1, sorteio.Dezena2, sorteio.Dezena3,
                sorteio.Dezena4, sorteio.Dezena5, sorteio.Dezena6
            };
            Array.Sort(dezenasSorteio);
            var chaveSorteio = string.Join(",", dezenasSorteio);

            if (chaveDezenas == chaveSorteio)
                return true;
        }

        return false;
    }
}
