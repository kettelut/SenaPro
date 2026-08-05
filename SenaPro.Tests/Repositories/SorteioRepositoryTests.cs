using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Data;

namespace SenaPro.Tests.Repositories;

/// <summary>
/// Fake do repositório de sorteios para testes, usando banco InMemory.
/// Expõe métodos adicionais úteis nos testes além da interface principal.
/// </summary>
public class SorteioRepositoryTests : ISorteioRepository
{
    private readonly AppDbContext _context;

    public SorteioRepositoryTests(AppDbContext context)
    {
        _context = context;
    }

    #region ISorteioRepository

    public async Task<List<Domain.Entities.Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .OrderBy(s => s.Concurso)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteConcursoAsync(int concurso, CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .AnyAsync(s => s.Concurso == concurso, cancellationToken);
    }

    public async Task AdicionarVariosAsync(IEnumerable<Domain.Entities.Sorteio> sorteios, CancellationToken cancellationToken = default)
    {
        await _context.Sorteios.AddRangeAsync(sorteios, cancellationToken);
    }

    public async Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    /// <summary>
    /// Conta o total de sorteios no banco (útil nos testes).
    /// </summary>
    public async Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios.CountAsync(cancellationToken);
    }
}
