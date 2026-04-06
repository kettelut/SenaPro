using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Data;

namespace SenaPro.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de sorteios.
/// </summary>
public class SorteioRepository : ISorteioRepository
{
    private readonly AppDbContext _context;

    public SorteioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sorteio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sorteios
            .OrderBy(s => s.Concurso)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sorteio?> ObterPorConcursoAsync(int concurso, CancellationToken cancellationToken = default)
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

    public async Task AdicionarAsync(Sorteio sorteio, CancellationToken cancellationToken = default)
    {
        await _context.Sorteios.AddAsync(sorteio, cancellationToken);
    }

    public async Task AdicionarVariosAsync(IEnumerable<Sorteio> sorteios, CancellationToken cancellationToken = default)
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