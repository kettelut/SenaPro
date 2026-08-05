using SenaPro.Domain.Entities;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para repositório de sorteios.
/// </summary>
public interface ISorteioRepository
{
    /// <summary>
    /// Obtém todos os sorteios, ordenados por número do concurso (ascendente).
    /// </summary>
    Task<List<Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um sorteio com o número do concurso informado.
    /// </summary>
    Task<bool> ExisteConcursoAsync(int concurso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona múltiplos sorteios ao repositório (batch insert).
    /// </summary>
    Task AdicionarVariosAsync(IEnumerable<Sorteio> sorteios, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta o total de sorteios no repositório.
    /// </summary>
    Task<int> ContarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações pendentes no banco de dados.
    /// </summary>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
