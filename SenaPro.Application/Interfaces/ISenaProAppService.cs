﻿using SenaPro.Domain.Entities;

namespace SenaPro.Application.Interfaces
{
    public interface ISenaProAppService
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

        List<FrequenciaNumeral> ObterDezSomaNumerosMaisSorteados();

        List<FrequenciaNumeral> ObterDezSomaNumerosMenosSorteados();

        List<FrequenciaNumeral> ObterTopSomaNumerosSorteiosAteMetade();

        /// <summary>
        /// Obtém os 5 pares de números que mais frequentemente aparecem juntos nos sorteios.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="ParesNumerais"/> contendo os pares de números mais frequentes e suas respectivas frequências.</returns>
        List<ParesNumerais> ParesDeNumerosQueMaisAparecemJuntos();

        /// <summary>
        /// Calcula a quantidade de sorteios anteriores necessários para localizar de 1 a 6 números.
        /// </summary>
        /// <returns>A quantidade de sorteios anteriores necessários para localizar de 1 a 6 números.</returns>
        List<CalculoSorteiosAnteriores> CalcularQntDeSorteiosAnterioresParaLocalizarNumeros();

        /// <summary>
        /// Gera uma lista de sugestões de números para o próximo sorteio, com base nos sorteios anteriores.
        /// </summary>
        /// <param name="qntNumerosPorJogo">A quantidade de números a serem sugeridos para cada jogo. Deve ser maior ou igual a 6.</param>
        /// <param name="qntDeJogos">A quantidade de jogos para os quais serão geradas sugestões. Deve ser maior ou igual a 1.</param>
        /// <returns>Uma lista de listas de inteiros, onde cada lista interna representa um conjunto sugerido de números para um jogo.</returns>
        /// <exception cref="ArgumentException">Lançada quando a quantidade de números por jogo é menor que 6 ou a quantidade de jogos é menor que 1.</exception>
        List<List<int>> ObterSugetaoParaProximoSorteio(int qntNumerosPorJogo, int qntDeJogos);
    }
}
