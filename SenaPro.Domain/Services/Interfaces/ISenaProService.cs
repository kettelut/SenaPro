using SenaPro.Domain.Entities;

namespace SenaPro.Domain.Services.Interfaces
{
    public interface ISenaProService
    {
        List<int> ObterUltimoSorteio();

        List<Sorteio> ObterTodosSorteios();
    }
}
