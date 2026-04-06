using SenaPro.Domain.Results;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para serviço de consulta à API da loteria.
/// </summary>
public interface IApiLoteriaService
{
    /// <summary>
    /// Consulta o último sorteio disponível na API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do último sorteio ou null se não disponível.</returns>
    Task<SorteioApiResultado?> ConsultarUltimoSorteioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta um sorteio específico pelo número do concurso.
    /// </summary>
    /// <param name="concurso">Número do concurso.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do sorteio ou null se não encontrado.</returns>
    Task<SorteioApiResultado?> ConsultarSorteioAsync(int concurso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o banco de dados com o último sorteio da API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado da atualização.</returns>
    Task<AtualizacaoApiResultado> AtualizarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se há novos sorteios disponíveis na API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado com informações sobre gap de sorteios.</returns>
    Task<AtualizacaoApiResultado> VerificarAtualizacoesAsync(CancellationToken cancellationToken = default);
}