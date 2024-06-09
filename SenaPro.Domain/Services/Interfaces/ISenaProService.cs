using SenaPro.Domain.Entities;

namespace SenaPro.Domain.Services.Interfaces
{
    public interface ISenaProService
    {
        /// <summary>
        /// Obtém os números sorteados do último sorteio realizado.
        /// </summary>
        /// <returns>Uma lista de inteiros representando os números sorteados no último sorteio. Retorna uma lista vazia se não houver sorteios.</returns>
        List<int> ObterUltimoSorteio();

        /// <summary>
        /// Obtém a lista de todos os sorteios realizados.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Sorteio"/> contendo todos os sorteios realizados.</returns>
        List<Sorteio> ObterTodosSorteios();

        /// <summary>
        /// Verifica se existem sorteios repetidos na lista de sorteios.
        /// </summary>
        /// <returns>Retorna true se houver sorteios repetidos, caso contrário, retorna false.</returns>
        bool ExisteSorteiosRepetidos();

        /// <summary>
        /// Obtém os cinco números mais sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números mais sorteados.</returns>
        List<FrequenciaNumeral> ObterCincoNumerosMaisSorteados();

        /// <summary>
        /// Obtém os cinco números menos sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números menos sorteados.</returns>
        List<FrequenciaNumeral> ObterCincoNumerosMenosSorteados();

        /// <summary>
        /// Obtém os 5 pares de números que mais frequentemente aparecem juntos nos sorteios.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="ParesNumerais"/> contendo os pares de números mais frequentes e suas respectivas frequências.</returns>
        List<ParesNumerais> ParesDeNumerosQueMaisAparecemJuntos();

        /// <summary>
        /// Calcula a quantidade máxima de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade máxima de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        int CalcularQntMaximaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar);

        /// <summary>
        /// Calcula a quantidade mínima de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade mínima de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        int CalcularQntMinimaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar);

        /// <summary>
        /// Calcula a quantidade média de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade média de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        double CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar);
    }
}
