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

        public List<FrequenciaNumeral> ObterDezSomaNumerosMaisSorteados()
        {
            var frequenciaNumeros = ObterFrequenciaSomaNumeros();
            return frequenciaNumeros.Take(10).ToList();
        }

        public List<FrequenciaNumeral> ObterTopSomaNumerosSorteiosAteMetade()
        {
            var frequenciaNumeros = ObterFrequenciaSomaNumeros();
            var response = new List<FrequenciaNumeral>();
            int contador = 0;
            while (contador <= (_sorteios.Count * 0.45))
            {
                contador += frequenciaNumeros[0].Frequencia;
                response.Add(frequenciaNumeros[0]);
                frequenciaNumeros.Remove(frequenciaNumeros[0]);
            }    
            return response;
        }
      
        public List<FrequenciaNumeral> ObterDezSomaNumerosMenosSorteados()
        {
            var frequenciaNumeros = ObterFrequenciaSomaNumeros();
            return frequenciaNumeros.TakeLast(10).ToList();
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
        /// Calcula a frequencia de sorteios anteriores necessários para localizar uma quantidade específica de números.
        /// </summary>
        /// <param name="qntNumerosAnalisar">A quantidade de números a serem analisados.</param>
        /// <returns>A frequencia de sorteios anteriores necessários para localizar a quantidade específica de números.</returns>
        public List<FrequenciaNumeral> CalcularFrequenciaDeSorteiosAnterioresParaLocalizarQntNumeros(int qntNumerosAnalisar)
        {
            List<int> resultados = CalcularSorteiosNecessarios(qntNumerosAnalisar);
            List<int> resultadosFiltrados = resultados.Where(r => r > 0).ToList();

            return resultadosFiltrados
                .GroupBy(n => n)
                .Select(g => new FrequenciaNumeral { Numero = g.Key, Frequencia = g.Count() })
                .OrderByDescending(fn => fn.Frequencia)
                .ToList();
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

            return response.OrderBy(lista => lista.Sum()).ToList();
        }
        #endregion

        #region Métodos Privados
        private List<int> ObtemUltimosSorteios(int qntNumeros)
        {
            var frequenciaNumeros = CalcularFrequenciaDeSorteiosAnterioresParaLocalizarQntNumeros(qntNumeros);
            var response = new List<FrequenciaNumeral>();
            int contador = 0;
            while (contador <= (_sorteios.Count * 0.45))
            {
                contador += frequenciaNumeros[0].Frequencia;
                response.Add(frequenciaNumeros[0]);
                frequenciaNumeros.Remove(frequenciaNumeros[0]);
            }
            
            int qntSorteios = response.Select(x => x.Numero).Max();
            return _sorteios.OrderByDescending(s => s.NumeroConcurso)
                .Take(qntSorteios)
                .Select(s => s.NumerosSorteados)
                .SelectMany(s => s)
                .Distinct()
                .ToList();
        }

        // Método para calcular a porcentagem de sorteios com X números pares
        private Dictionary<int, double> CalcularPorcentagemNumerosPares()
        {
            var contagemPares = new Dictionary<int, int>();

            // Inicializa o dicionário com as chaves de 0 a 6 (quantidade de pares possíveis)
            for (int i = 0; i <= 6; i++)
            {
                contagemPares[i] = 0;
            }

            // Itera sobre cada sorteio e conta a quantidade de números pares sorteados
            foreach (var sorteio in _sorteios)
            {
                int qtdPares = sorteio.NumerosSorteados.Count(n => n % 2 == 0);
                contagemPares[qtdPares]++;
            }

            // Calcula a porcentagem de sorteios com 0 a 6 números pares
            var porcentagensPares = new Dictionary<int, double>();
            int totalSorteios = _sorteios.Count;

            foreach (var item in contagemPares)
            {
                porcentagensPares[item.Key] = (item.Value / (double)totalSorteios) * 100;
            }

            return porcentagensPares;
        }

        // Método para calcular a porcentagem de sorteios com X números ímpares
        private Dictionary<int, double> CalcularPorcentagemNumerosImpares()
        {
            var contagemImpares = new Dictionary<int, int>();

            // Inicializa o dicionário com as chaves de 0 a 6 (quantidade de ímpares possíveis)
            for (int i = 0; i <= 6; i++)
            {
                contagemImpares[i] = 0;
            }

            // Itera sobre cada sorteio e conta a quantidade de números ímpares sorteados
            foreach (var sorteio in _sorteios)
            {
                int qtdImpares = sorteio.NumerosSorteados.Count(n => n % 2 != 0);
                contagemImpares[qtdImpares]++;
            }

            // Calcula a porcentagem de sorteios com 0 a 6 números ímpares
            var porcentagensImpares = new Dictionary<int, double>();
            int totalSorteios = _sorteios.Count;

            foreach (var item in contagemImpares)
            {
                porcentagensImpares[item.Key] = (item.Value / (double)totalSorteios) * 100;
            }

            return porcentagensImpares;
        }

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

            List<int> somasMedidas = ObterTopSomaNumerosSorteiosAteMetade().Select(x => x.Numero).ToList();

            var ultimosSorteios6Numeros = ObtemUltimosSorteios(6);
            var ultimosSorteios5Numeros = ObtemUltimosSorteios(5);
            var ultimosSorteios4Numeros = ObtemUltimosSorteios(4);
            var ultimosSorteios3Numeros = ObtemUltimosSorteios(3);
            var ultimosSorteios2Numeros = ObtemUltimosSorteios(2);
            var ultimosSorteios1Numeros = ObtemUltimosSorteios(1);

            int numeroDeSimulacoes = 1000000; // Número de simulações de Monte Carlo
            Random random = new Random();
            bool numerosSugeridosValidos = false;
            List<int> numerosSugeridos = new List<int>();

            while (!numerosSugeridosValidos)
            {
                var resultadosSimulacao = SimularSorteios(numeroDeSimulacoes);
                var frequenciaNumeros = CalcularFrequencia(resultadosSimulacao);

                numerosSugeridos = frequenciaNumeros
                    .OrderByDescending(kv => kv.Value)
                    .Take(qntNumerosPrevisao)
                    .Select(kv => kv.Key)
                    .OrderBy(n => n)
                    .ToList();

                List<bool> validacoesObtrigatorias = new List<bool>();
                List<bool> validacoesOpcionais = new List<bool>();

                var combinacoes = new Combinacoes();
                var resultado = combinacoes.CalculaCombinacoes(numerosSugeridos, 6);

                foreach (List<int> combinacao in resultado)
                {
                    // Sorteios Repetido
                    validacoesObtrigatorias.Add(!conjuntosSorteados.Contains(string.Join("-", combinacao.OrderBy(n => n))));

                    // Sorteios Anteriores
                    validacoesOpcionais.Add(combinacao.Count(num => ultimosSorteios6Numeros.Contains(num)) == 6);
                    validacoesOpcionais.Add(combinacao.Count(num => ultimosSorteios5Numeros.Contains(num)) == 5);
                    validacoesOpcionais.Add(combinacao.Count(num => ultimosSorteios4Numeros.Contains(num)) == 4);
                    validacoesOpcionais.Add(combinacao.Count(num => ultimosSorteios3Numeros.Contains(num)) == 3);
                    validacoesOpcionais.Add(combinacao.Count(num => ultimosSorteios2Numeros.Contains(num)) == 2);

                    // Somatórias 
                    validacoesObtrigatorias.Add(somasMedidas.Contains(combinacao.Sum()));

                    // Balanço entre par e impar
                    var qntPares = combinacao.Count(num => num % 2 == 0);
                    var qntImpares = combinacao.Count(num => num % 2 != 0);
                    validacoesObtrigatorias.Add(qntPares >= 2 && qntPares <= 4);
                    validacoesObtrigatorias.Add(qntImpares >= 2 && qntImpares <= 4);
                }

                validacoesObtrigatorias.Add(numerosSugeridos.Count(num => ultimosSorteios6Numeros.Contains(num)) == qntNumerosPrevisao);
                validacoesObtrigatorias.Add(numerosSugeridos.Count(num => ultimosSorteios1Numeros.Contains(num)) == 1);

                // Valida
                if(qntNumerosPrevisao == 6)
                {
                    numerosSugeridosValidos = validacoesObtrigatorias.TrueForAll(x => x) && validacoesOpcionais.TrueForAll(x => x);
                }
                else 
                {
                    numerosSugeridosValidos = validacoesObtrigatorias.TrueForAll(x => x) && validacoesOpcionais.Where(x => x == true).Count() >= 27;
                }
                
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
        /// Obtém a frequência da soma de todos os números sorteados.
        /// </summary>
        /// <returns>Uma lista de FrequenciaNumeral contendo todas as somas dos números sorteados e suas frequências.</returns>
        private List<FrequenciaNumeral> ObterFrequenciaSomaNumeros()
        {
            return _sorteios
                .Select(s => s.SomaNumeros)
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
