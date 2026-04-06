using SenaPro.Domain.Entities;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para repositório de sorteios.
/// </summary>
public interface ISorteioRepository
{
    /// <summary>
    /// Obtém todos os sorteios.
    /// </summary>
    Task<List<Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um sorteio pelo número do concurso.
    /// </summary>
    Task<Sorteio?> ObterPorConcursoAsync(int concurso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um sorteio com o número do concurso.
    /// </summary>
    Task<bool> ExisteConcursoAsync(int concurso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o número do último concurso.
    /// </summary>
    Task<int?> ObterUltimoConcursoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo sorteio.
    /// </summary>
    Task AdicionarAsync(Sorteio sorteio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona múltiplos sorteios.
    /// </summary>
    Task AdicionarVariosAsync(IEnumerable<Sorteio> sorteios, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva as alterações.
    /// </summary>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta o total de sorteios.
    /// </summary>
    Task<int> ContarAsync(CancellationToken cancellationToken = default);
}