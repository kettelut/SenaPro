using SenaPro.Domain.Results;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para serviço de análise estatística de sorteios.
/// </summary>
public interface IAnaliseEstatisticaService
{
    /// <summary>
    /// Analisa se existem sorteios repetidos no histórico.
    /// Sorteios repetidos são aqueles que possuem as mesmas 6 dezenas,
    /// independente da ordem em que foram sorteadas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado da análise com lista de pares repetidos.</returns>
    Task<SorteiosRepetidosResultado> AnalisarSorteiosRepetidosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um conjunto de dezenas já foi sorteado em algum concurso.
    /// </summary>
    /// <param name="dezenas">Array de 6 dezenas ordenadas.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se já existe um sorteio com essas dezenas, false caso contrário.</returns>
    Task<bool> VerificarDezenasJaSorteadasAsync(byte[] dezenas, CancellationToken cancellationToken = default);
}