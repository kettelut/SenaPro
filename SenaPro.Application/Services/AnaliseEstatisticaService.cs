using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.Application.Services;

/// <summary>
/// Serviço de análise estatística de sorteios da Mega-Sena.
/// </summary>
public class AnaliseEstatisticaService : IAnaliseEstatisticaService
{
    private readonly ISorteioRepository _sorteioRepository;

    public AnaliseEstatisticaService(ISorteioRepository sorteioRepository)
    {
        _sorteioRepository = sorteioRepository;
    }

    public async Task<SorteiosRepetidosResultado> AnalisarSorteiosRepetidosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new SorteiosRepetidosResultado();

        try
        {
            var sorteios = await _sorteioRepository.ObterTodosAsync(cancellationToken);

            if (sorteios.Count == 0)
            {
                resultado.Sucesso = true;
                resultado.ExistemRepetidos = false;
                resultado.QuantidadePares = 0;
                resultado.Mensagem = "Não há sorteios cadastrados.";
                return resultado;
            }

            // Agrupa sorteios por conjunto de dezenas (ordenadas)
            var gruposPorDezenas = new Dictionary<string, List<Domain.Entities.Sorteio>>();

            foreach (var sorteio in sorteios)
            {
                var dezenasOrdenadas = new byte[]
                {
                    sorteio.Dezena1, sorteio.Dezena2, sorteio.Dezena3,
                    sorteio.Dezena4, sorteio.Dezena5, sorteio.Dezena6
                };
                Array.Sort(dezenasOrdenadas);
                var chave = string.Join(",", dezenasOrdenadas);

                if (!gruposPorDezenas.ContainsKey(chave))
                {
                    gruposPorDezenas[chave] = new List<Domain.Entities.Sorteio>();
                }
                gruposPorDezenas[chave].Add(sorteio);
            }

            // Encontra pares de sorteios repetidos
            var pares = new List<SorteioRepetidoInfo>();

            foreach (var grupo in gruposPorDezenas.Values)
            {
                if (grupo.Count > 1)
                {
                    // Ordena por concurso para criar pares ordenados
                    var ordenados = grupo.OrderBy(s => s.Concurso).ToList();

                    // Gera todas as combinações de pares
                    for (int i = 0; i < ordenados.Count; i++)
                    {
                        for (int j = i + 1; j < ordenados.Count; j++)
                        {
                            var primeiro = ordenados[i];
                            var segundo = ordenados[j];

                            pares.Add(new SorteioRepetidoInfo
                            {
                                Concurso1 = primeiro.Concurso,
                                Data1 = primeiro.Data,
                                Concurso2 = segundo.Concurso,
                                Data2 = segundo.Data,
                                Dezenas = new byte[]
                                {
                                    primeiro.Dezena1, primeiro.Dezena2, primeiro.Dezena3,
                                    primeiro.Dezena4, primeiro.Dezena5, primeiro.Dezena6
                                }.OrderBy(d => d).ToArray()
                            });
                        }
                    }
                }
            }

            resultado.Sucesso = true;
            resultado.ExistemRepetidos = pares.Count > 0;
            resultado.QuantidadePares = pares.Count;
            resultado.Pares = pares;

            if (pares.Count == 0)
            {
                resultado.Mensagem = "Não foram encontrados sorteios repetidos no histórico.";
            }
            else
            {
                resultado.Mensagem = $"Foram encontrados {pares.Count} par(es) de sorteios com as mesmas dezenas.";
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro ao analisar sorteios: {ex.Message}");
            resultado.Mensagem = "Erro ao realizar análise.";
            return resultado;
        }
    }

    public async Task<bool> VerificarDezenasJaSorteadasAsync(byte[] dezenas, CancellationToken cancellationToken = default)
    {
        // Validações
        if (dezenas == null || dezenas.Length != 6)
        {
            throw new ArgumentException("Devem ser informadas exatamente 6 dezenas.", nameof(dezenas));
        }

        // Verifica se todas as dezenas estão no intervalo válido
        foreach (var dezena in dezenas)
        {
            if (dezena < 1 || dezena > 60)
            {
                throw new ArgumentException("As dezenas devem estar entre 1 e 60.", nameof(dezenas));
            }
        }

        // Ordena as dezenas para comparação
        var dezenasOrdenadas = dezenas.OrderBy(d => d).ToArray();
        var chaveDezenas = string.Join(",", dezenasOrdenadas);

        var sorteios = await _sorteioRepository.ObterTodosAsync(cancellationToken);

        foreach (var sorteio in sorteios)
        {
            var dezenasSorteio = new byte[]
            {
                sorteio.Dezena1, sorteio.Dezena2, sorteio.Dezena3,
                sorteio.Dezena4, sorteio.Dezena5, sorteio.Dezena6
            };
            Array.Sort(dezenasSorteio);
            var chaveSorteio = string.Join(",", dezenasSorteio);

            if (chaveDezenas == chaveSorteio)
            {
                return true;
            }
        }

        return false;
    }
}