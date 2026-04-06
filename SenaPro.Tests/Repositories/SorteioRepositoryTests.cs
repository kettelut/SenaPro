using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Data;
using SenaPro.Infrastructure.Repositories;

namespace SenaPro.Tests.Repositories;

/// <summary>
/// Implementação do repositório de sorteios para testes usando InMemory database.
/// </summary>
public class SorteioRepositoryTests : ISorteioRepository
{
    private readonly AppDbContext _context;

    public SorteioRepositoryTests(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Entities.Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .OrderBy(s => s.Concurso)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Sorteio?> ObterPorConcursoAsync(int concurso, CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .FirstOrDefaultAsync(s => s.Concurso == concurso, cancellationToken);
    }

    public async Task<bool> ExisteConcursoAsync(int concurso, CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .AnyAsync(s => s.Concurso == concurso, cancellationToken);
    }

    public async Task<int?> ObterUltimoConcursoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .MaxAsync(s => (int?)s.Concurso, cancellationToken);
    }

    public async Task AdicionarAsync(Domain.Entities.Sorteio sorteio, CancellationToken cancellationToken = default)
    {
        await _context.Sorteios.AddAsync(sorteio, cancellationToken);
    }

    public async Task AdicionarVariosAsync(IEnumerable<Domain.Entities.Sorteio> sorteios, CancellationToken cancellationToken = default)
    {
        await _context.Sorteios.AddRangeAsync(sorteios, cancellationToken);
    }

    public async Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios.CountAsync(cancellationToken);
    }
}