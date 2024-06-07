using SenaPro.Domain.Entities;

namespace SenaPro.Domain.Repositories
{ 
    public interface IMegaSena
    {
        /// <summary>
        /// Obtém a lista de sorteios da Mega-Sena a partir do arquivo XLSX.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Sorteio"/> contendo os resultados dos sorteios.</returns>
        List<Sorteio> ObterSorteios();
    }
}
