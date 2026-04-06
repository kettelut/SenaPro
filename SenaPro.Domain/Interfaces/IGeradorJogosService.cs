using SenaPro.Domain.Results;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para serviço de geração de sugestões de jogos.
/// </summary>
public interface IGeradorJogosService
{
    /// <summary>
    /// Gera sugestões de jogos baseado nas configurações informadas.
    /// </summary>
    /// <param name="configuracao">Configurações de geração.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado com lista de jogos sugeridos.</returns>
    Task<GeracaoJogosResultado> GerarJogosAsync(ConfiguracaoGeracaoJogos configuracao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna lista de análises estatísticas disponíveis para seleção.
    /// </summary>
    /// <returns>Lista de nomes de análises disponíveis.</returns>
    Task<List<string>> ObterAnalisesDisponiveisAsync();
}