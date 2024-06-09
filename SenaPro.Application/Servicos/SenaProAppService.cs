using SenaPro.Application.Interfaces;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Services.Interfaces;

namespace SenaPro.Application.Services
{
    public class SenaProAppService : ISenaProAppService
	{
		private readonly ISenaProService _senaProService;

		public SenaProAppService(ISenaProService senaProService)
		{
			_senaProService = senaProService;
		}

        /// <summary>
        /// Obtém os números sorteados do último sorteio realizado.
        /// </summary>
        /// <returns>Uma lista de inteiros representando os números sorteados no último sorteio. Retorna uma lista vazia se não houver sorteios.</returns>
        public List<int> ObterUltimoSorteio()
        {
            return _senaProService.ObterUltimoSorteio();
        }

        /// <summary>
        /// Obtém a lista de todos os sorteios realizados.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Sorteio"/> contendo todos os sorteios realizados.</returns>
        public List<Sorteio> ObterTodosSorteios()
        {
            return _senaProService.ObterTodosSorteios();
        }

        /// <summary>
        /// Verifica se existem sorteios repetidos na lista de sorteios.
        /// </summary>
        /// <returns>Retorna true se houver sorteios repetidos, caso contrário, retorna false.</returns>
        public bool ExisteSorteiosRepetidos()
        {
            return _senaProService.ExisteSorteiosRepetidos();
        }

        /// <summary>
        /// Obtém os cinco números mais sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números mais sorteados.</returns>
        public List<FrequenciaNumeral> ObterCincoNumerosMaisSorteados()
        {
            return _senaProService.ObterCincoNumerosMaisSorteados();
        }

        /// <summary>
        /// Obtém os cinco números menos sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números menos sorteados.</returns>
        public List<FrequenciaNumeral> ObterCincoNumerosMenosSorteados()
        {
            return _senaProService.ObterCincoNumerosMenosSorteados();
        }

        /// <summary>
        /// Obtém os 5 pares de números que mais frequentemente aparecem juntos nos sorteios.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="ParesNumerais"/> contendo os pares de números mais frequentes e suas respectivas frequências.</returns>
        public List<ParesNumerais> ParesDeNumerosQueMaisAparecemJuntos()
        {
            return _senaProService.ParesDeNumerosQueMaisAparecemJuntos();
        }

        /// <summary>
        /// Calcula a quantidade de sorteios anteriores necessários para localizar de 1 a 6 números.
        /// </summary>
        /// <returns>A quantidade de sorteios anteriores necessários para localizar de 1 a 6 números.</returns>
        public List<CalculoSorteiosAnteriores> CalcularQntDeSorteiosAnterioresParaLocalizarNumeros()
        {
            var response = new List<CalculoSorteiosAnteriores>();

            for (int i = 1; i <= 6; i++)
            {
                var calculoSorteiosAnteriores = new CalculoSorteiosAnteriores { QntDeNumeros = i};
                calculoSorteiosAnteriores.QntMaximaDeSorteios = _senaProService.CalcularQntMaximaDeSorteiosAnterioresParaLocalizarQntNumeros(i);
                calculoSorteiosAnteriores.QntMinimaDeSorteios = _senaProService.CalcularQntMinimaDeSorteiosAnterioresParaLocalizarQntNumeros(i);
                calculoSorteiosAnteriores.QntMediaDeSorteios = Convert.ToInt32(_senaProService.CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(i));
                response.Add(calculoSorteiosAnteriores);
            }

            return response;
        }
    }
}
