using SenaPro.Domain.Entities;
using SenaPro.Domain.Repositories;
using SenaPro.Domain.Services.Interfaces;
using System.Linq;

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

        /// <summary>
        /// Gera uma lista de sugestões de números para o próximo sorteio, com base nos sorteios anteriores.
        /// </summary>
        /// <param name="qntNumerosPorJogo">A quantidade de números a serem sugeridos para cada jogo. Deve ser maior ou igual a 6.</param>
        /// <param name="qntDeJogos">A quantidade de jogos para os quais serão geradas sugestões. Deve ser maior ou igual a 1.</param>
        /// <returns>Uma lista de listas de inteiros, onde cada lista interna representa um conjunto sugerido de números para um jogo.</returns>
        /// <exception cref="ArgumentException">Lançada quando a quantidade de números por jogo é menor que 6 ou a quantidade de jogos é menor que 1.</exception>
        public List<List<int>> ObterSugetaoParaProximoSorteio(int qntNumerosPorJogo, int qntDeJogos)
        {
            // Validação dos parâmetros
            if (qntNumerosPorJogo < 6)
            {
                throw new ArgumentException("A quantidade de números por jogo não pode ser menor que 6.");
            }

            if (qntDeJogos < 1)
            {
                throw new ArgumentException("A quantidade de jogos deve ser igual ou maior que 1.");
            }

            var response = new List<List<int>>();
            var random = new Random();
            var numerosSorteados = _sorteios.Select(s => s.NumerosSorteados).ToList();

            for (int i = 0; i < qntDeJogos; i++)
            {
                var numerosSugeridos = ObterNumeroSugeridos(numerosSorteados, qntNumerosPorJogo);
                response.Add(numerosSugeridos);
            }

            return response;
        }
        #endregion

        #region Métodos Privados
        /// <summary>
        /// Gera uma lista de números sugeridos com base em simulações de sorteios anteriores e na frequência de ocorrência
        /// dos números sorteados, utilizando o método de Monte Carlo.
        /// </summary>
        /// <param name="numerosSorteados">Uma lista de listas de inteiros, onde cada lista interna representa um conjunto de números sorteados.</param>
        /// <param name="qntNumerosPrevisao">O número de previsões de números a serem gerados.</param>
        /// <returns>Uma lista de inteiros representando os números sugeridos, ordenados em ordem crescente.</returns>
        private List<int> ObterNumeroSugeridos(List<List<int>> numerosSorteados, int qntNumerosPrevisao)
        {
            var conjuntosSorteados = new HashSet<string>(numerosSorteados.Select(s => string.Join("-", s.OrderBy(n => n))));

            var ultimosSorteiosTodosNumeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularQntMaximaDeSorteiosAnterioresParaLocalizarQntNumeros(6)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios6Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(6)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios5Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(5)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios4Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(4)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios3Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(3)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios2Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(2)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            var ultimosSorteios1Numeros = _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(Convert.ToInt32(CalcularMediaDeSorteiosAnterioresParaLocalizarQntNumeros(1)))
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .ToList();

            int numeroDeSimulacoes = 1000000; // Número de simulações de Monte Carlo
            List<int> numerosSugeridos = new List<int>();
            Random random = new Random();
            bool numerosSugeridosValidos = false;

            while (!numerosSugeridosValidos)
            {
                var resultadosSimulacao = SimularSorteios(numeroDeSimulacoes);
                var frequenciaNumeros = CalcularFrequencia(resultadosSimulacao);

                numerosSugeridos = frequenciaNumeros
                    .OrderByDescending(kv => kv.Value)
                    .ThenBy(kv => random.Next()) // Adiciona aleatoriedade na seleção dos números
                    .Take(qntNumerosPrevisao)
                    .Select(kv => kv.Key)
                    .OrderBy(n => n)
                    .ToList();

                var combinacoes = new Combinacoes();
                var resultado = combinacoes.CalculaCombinacoes(numerosSugeridos, 6);

                List<bool> validacoes = new List<bool>();
                foreach(List<int> combinacao in resultado)
                {
                    bool validaRepeticao = !conjuntosSorteados.Contains(string.Join("-", combinacao.OrderBy(n => n)));
                    bool valida6Numeros = combinacao.Count(num => ultimosSorteios6Numeros.Contains(num)) == 6;
                    bool valida5Numeros = combinacao.Count(num => ultimosSorteios5Numeros.Contains(num)) == 5;
                    bool valida4Numeros = combinacao.Count(num => ultimosSorteios4Numeros.Contains(num)) == 4;
                    bool valida3Numeros = combinacao.Count(num => ultimosSorteios3Numeros.Contains(num)) == 3;
                    bool valida2Numeros = combinacao.Count(num => ultimosSorteios2Numeros.Contains(num)) == 2;
                    bool valida1Numeros = combinacao.Count(num => ultimosSorteios1Numeros.Contains(num)) == 1;
                    validacoes.Add(validaRepeticao && valida6Numeros && valida5Numeros && valida4Numeros && valida3Numeros && valida2Numeros && valida1Numeros);
                }

                numerosSugeridosValidos = numerosSugeridos.Count(num => ultimosSorteiosTodosNumeros.Contains(num)) == qntNumerosPrevisao && (!validacoes.Exists(x => x == false));
 
            }

            return numerosSugeridos;
        }

        /// <summary>
        /// Calcula a frequência de ocorrência de cada número em uma lista de sorteios simulados.
        /// </summary>
        /// <param name="resultadosSimulacao">Uma lista de arrays, onde cada array representa um sorteio simulado contendo 6 números inteiros.</param>
        /// <returns>Um dicionário onde a chave é um número inteiro e o valor é a quantidade de vezes que o número apareceu em todos os sorteios simulados.</returns>
        private Dictionary<int, int> CalcularFrequencia(List<int[]> resultadosSimulacao)
        {
            var frequencia = new Dictionary<int, int>();

            foreach (var sorteio in resultadosSimulacao)
            {
                foreach (var numero in sorteio)
                {
                    if (frequencia.ContainsKey(numero))
                    {
                        frequencia[numero]++;
                    }
                    else
                    {
                        frequencia[numero] = 1;
                    }
                }
            }

            return frequencia;
        }

        /// <summary>
        /// Simula múltiplos sorteios da Mega-Sena gerando números aleatórios.
        /// Cada sorteio consiste em 6 números únicos entre 1 e 60.
        /// </summary>
        /// <param name="numeroDeSimulacoes">O número de sorteios a serem simulados.</param>
        /// <returns>Uma lista de arrays, onde cada array contém 6 números inteiros representando um sorteio simulado.</returns>
        private List<int[]> SimularSorteios(int numeroDeSimulacoes)
        {
            var resultados = new List<int[]>();
            var random = new Random();

            for (int i = 0; i < numeroDeSimulacoes; i++)
            {
                var sorteioSimulado = new int[6];
                var numerosSorteados = new HashSet<int>();

                while (numerosSorteados.Count < 6)
                {
                    int numeroSorteado = random.Next(1, 61); // Números de 1 a 60
                    numerosSorteados.Add(numeroSorteado);
                }

                numerosSorteados.CopyTo(sorteioSimulado);
                resultados.Add(sorteioSimulado);
            }

            return resultados;
        }

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
        private IEnumerable<List<int>> GetCombinacoes(List<int> lista, int tamanho)
        {
            int[] resultado = new int[tamanho];
            Stack<int> stack = new Stack<int>(tamanho);
            stack.Push(0);

            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();

                while (value < lista.Count)
                {
                    resultado[index++] = lista[value++];
                    stack.Push(value);

                    if (index == tamanho)
                    {
                        yield return new List<int>(resultado);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
