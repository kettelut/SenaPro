using SenaPro.Domain.Entities;
using SenaPro.Domain.Repositories;
using SenaPro.Domain.Services.Interfaces;

namespace SenaPro.Domain.Services
{
    /// <summary>
    /// Serviço responsável por fornecer operações relacionadas aos sorteios da Mega-Sena.
    /// </summary>
    public class SenaProService : ISenaProService
	{
        #region Parâmetros
        /// <summary>
        /// Interface para acesso aos dados da Mega-Sena.
        /// </summary>
        private readonly IMegaSenaResourceAccess _megaSenaResourceAccess;

        /// <summary>
        /// Lista de todos os sorteios realizados.
        /// </summary>
        private readonly List<Sorteio> _sorteios;
        #endregion

        #region Construtor
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="SenaProService"/>.
        /// </summary>
        /// <param name="megaSena">Interface para acesso aos dados da Mega-Sena.</param>
        public SenaProService(IMegaSenaResourceAccess megaSenaResourceAccess)
        {
            _megaSenaResourceAccess = megaSenaResourceAccess;
            _sorteios = _megaSenaResourceAccess.ObterSorteios();
        }
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Obtém os números sorteados do último sorteio realizado.
        /// </summary>
        /// <returns>Uma lista de inteiros representando os números sorteados no último sorteio. Retorna uma lista vazia se não houver sorteios.</returns>
        public List<int> ObterUltimoSorteio()
        {
            return _sorteios.LastOrDefault()?.NumerosSorteados ?? new List<int>();
        }

        /// <summary>
        /// Obtém a lista de todos os sorteios realizados.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Sorteio"/> contendo todos os sorteios realizados.</returns>
        public List<Sorteio> ObterTodosSorteios()
        {
            return new List<Sorteio>(_sorteios);
        }

        /// <summary>
        /// Verifica se existem sorteios repetidos na lista de sorteios.
        /// </summary>
        /// <returns>Retorna true se houver sorteios repetidos, caso contrário, retorna false.</returns>
        public bool ExisteSorteiosRepetidos()
        {
            var conjuntosSorteados = new HashSet<string>();

            foreach (var sorteio in _sorteios)
            {
                var representacao = string.Join("-", sorteio.NumerosSorteados.OrderBy(n => n));

                if (!conjuntosSorteados.Add(representacao))
                {
                    return true; // Encontramos um sorteio repetido
                }
            }

            return false; // Não foram encontrados sorteios repetidos
        }

        /// <summary>
        /// Obtém os cinco números mais sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números mais sorteados.</returns>
        public List<FrequenciaNumeral> ObterCincoNumerosMaisSorteados()
        {
            var frequenciaNumeros = ObterFrequenciaNumeros();
            return frequenciaNumeros.Take(5).ToList();
        }

        /// <summary>
        /// Obtém os cinco números menos sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo os cinco números menos sorteados.</returns>
        public List<FrequenciaNumeral> ObterCincoNumerosMenosSorteados()
        {
            var frequenciaNumeros = ObterFrequenciaNumeros();
            return frequenciaNumeros.TakeLast(5).ToList();
        }

        /// <summary>
        /// Obtém os 5 pares de números que mais frequentemente aparecem juntos nos sorteios.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="ParesNumerais"/> contendo os pares de números mais frequentes e suas respectivas frequências.</returns>
        public List<ParesNumerais> ParesDeNumerosQueMaisAparecemJuntos()
        {
            var paresFrequentes = new Dictionary<string, int>();

            foreach (var sorteio in _sorteios)
            {
                var combinacoes = GetCombinacoes(sorteio.NumerosSorteados, 2); // Gera todos os pares possíveis no sorteio

                foreach (var par in combinacoes)
                {
                    string parChave = string.Join("-", par.OrderBy(n => n));
                    if (paresFrequentes.ContainsKey(parChave))
                    {
                        paresFrequentes[parChave]++;
                    }
                    else
                    {
                        paresFrequentes[parChave] = 1;
                    }
                }
            }

            var paresMaisFrequentes = paresFrequentes
                .OrderByDescending(p => p.Value)
                .Take(5)
                .Select(p => new ParesNumerais { ParNumerico = p.Key, Frequencia = p.Value })
                .ToList();

            return paresMaisFrequentes;
        }

        /// <summary>
        /// Calcula a quantidade máxima de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade máxima de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        public int CalcularQntMaximaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar)
        {
            List<int> resultados = CalcularSorteiosNecessarios(qntNumerosAnalisar);
            var resultadosValidos = resultados.Where(r => r > 0).ToList();

            if (resultadosValidos.Any())
            {
                return resultadosValidos.Max();
            }
            return 0;
        }

        /// <summary>
        /// Calcula a quantidade mínima de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade mínima de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        public int CalcularQntMinimaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar)
        {
            List<int> resultados = CalcularSorteiosNecessarios(qntNumerosAnalisar);
            var resultadosValidos = resultados.Where(r => r > 0).ToList();

            if (resultadosValidos.Any())
            {
                return resultadosValidos.Min();
            }
            return 0;
        }

        /// <summary>
        /// Calcula a quantidade média de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A quantidade média de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        public double CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar)
        {
            List<int> resultados = CalcularSorteiosNecessarios(qntNumerosAnalisar);
            var resultadosValidos = resultados.Where(r => r > 0).ToList();

            if (resultadosValidos.Any())
            {
                return resultadosValidos.Average();
            }
            return 0;
        }
        #endregion

        #region Métodos Privados
        /// <summary>
        /// Calcula a quantidade de sorteios necessários para encontrar os números que faltam para formar um conjunto de números específico.
        /// </summary>
        /// <param name="qntDeNumeros">A quantidade de números necessários para formar o conjunto.</param>
        /// <returns>Uma lista contendo a quantidade de sorteios necessários para cada conjunto de números.</returns>
        private List<int> CalcularSorteiosNecessarios(int qntDeNumeros)
        {
            List<int> resultados = new List<int>();
            _sorteios.Reverse();

            for (int i = 0; i < _sorteios.Count; i++)
            {
                HashSet<int> bolasNecessarias = new HashSet<int>(_sorteios[i].NumerosSorteados);
                int sorteiosNecessarios = 0;

                for (int j = i + 1; j < _sorteios.Count; j++)
                {
                    foreach (int bola in _sorteios[j].NumerosSorteados)
                    {
                        bolasNecessarias.Remove(bola);
                    }

                    sorteiosNecessarios++;

                    if (bolasNecessarias.Count <= (6 - qntDeNumeros))
                    {
                        break;
                    }
                }

                if (bolasNecessarias.Count > (6 - qntDeNumeros))
                    sorteiosNecessarios = 0;

                resultados.Add(sorteiosNecessarios);
            }

            resultados.Reverse();
            _sorteios.Reverse();
            return resultados;
        }

        /// <summary>
        /// Obtém a frequência de todos os números sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo todos os números sorteados e suas frequências.</returns>
        private List<FrequenciaNumeral> ObterFrequenciaNumeros()
        {
            return _sorteios
                .SelectMany(s => s.NumerosSorteados)
                .GroupBy(n => n)
                .Select(g => new FrequenciaNumeral { Numero = g.Key, Frequencia = g.Count() })
                .OrderByDescending(fn => fn.Frequencia)
                .ToList();
        }

        /// <summary>
        /// Gera todas as combinações possíveis de um determinado tamanho a partir de uma lista fornecida.
        /// </summary>
        /// <typeparam name="T">O tipo de elementos na lista.</typeparam>
        /// <param name="list">A lista de elementos a partir da qual as combinações serão geradas.</param>
        /// <param name="length">O tamanho das combinações a serem geradas.</param>
        /// <returns>Uma coleção de combinações, onde cada combinação é uma coleção de elementos do tipo T.</returns>
        /// <remarks>
        /// Este método utiliza recursão para gerar as combinações. Se o tamanho solicitado for 1, retorna uma coleção de elementos individuais.
        /// Para tamanhos maiores, gera todas as combinações menores e as expande adicionando cada um dos elementos não presentes na combinação menor.
        /// Este método não verifica por duplicatas na lista de entrada e assume que todos os elementos são únicos.
        /// </remarks>
        private static IEnumerable<IEnumerable<T>> GetCombinacoes<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetCombinacoes(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                            (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        #endregion
    }
}
