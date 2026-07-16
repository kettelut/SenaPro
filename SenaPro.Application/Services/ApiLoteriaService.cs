using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.Application.Services;

/// <summary>
/// Serviço de consulta à API da loteria para atualização de sorteios.
/// </summary>
public class ApiLoteriaService : IApiLoteriaService
{
    private const string UrlBase = "https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena";
    private readonly HttpClient _httpClient;
    private readonly ISorteioRepository _sorteioRepository;

    public ApiLoteriaService(HttpClient httpClient, ISorteioRepository sorteioRepository)
    {
        _httpClient = httpClient;
        _sorteioRepository = sorteioRepository;
    }

    public async Task<SorteioApiResultado?> ConsultarUltimoSorteioAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(UrlBase, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseSorteioDaApi(content);
        }
        catch
        {
            return null;
        }
    }

    public async Task<SorteioApiResultado?> ConsultarSorteioAsync(int concurso, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{UrlBase}/{concurso}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseSorteioDaApi(content);
        }
        catch
        {
            return null;
        }
    }

    public async Task<AtualizacaoApiResultado> VerificarAtualizacoesAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new AtualizacaoApiResultado();

        try
        {
            // Consulta último sorteio da API
            var sorteioApi = await ConsultarUltimoSorteioAsync(cancellationToken);

            if (sorteioApi == null)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("Não foi possível consultar a API da loteria");
                return resultado;
            }

            resultado.UltimoConcursoApi = sorteioApi.Concurso;

            // Consulta último sorteio do banco
            var ultimoConcursoBanco = await _sorteioRepository.ObterUltimoConcursoAsync(cancellationToken);
            resultado.UltimoConcursoBanco = ultimoConcursoBanco;

            // Verifica gap
            if (ultimoConcursoBanco.HasValue)
            {
                var gap = sorteioApi.Concurso - ultimoConcursoBanco.Value;
                resultado.HaGap = gap > 1;
                resultado.QuantidadeGap = Math.Max(0, gap - 1);
            }
            else
            {
                // Banco vazio - não é considerado gap para alerta
                resultado.HaGap = false;
                resultado.QuantidadeGap = 0;
            }

            resultado.Sucesso = true;
            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro ao verificar atualizações: {ex.Message}");
            return resultado;
        }
    }

    public async Task<AtualizacaoApiResultado> AtualizarAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new AtualizacaoApiResultado();

        try
        {
            // Verifica situação atual
            var verificacao = await VerificarAtualizacoesAsync(cancellationToken);

            if (!verificacao.Sucesso)
            {
                resultado.Sucesso = false;
                resultado.Erros.AddRange(verificacao.Erros);
                return resultado;
            }

            resultado.UltimoConcursoApi = verificacao.UltimoConcursoApi;
            resultado.UltimoConcursoBanco = verificacao.UltimoConcursoBanco;
            resultado.HaGap = verificacao.HaGap;
            resultado.QuantidadeGap = verificacao.QuantidadeGap;

            // Se há gap grande, alerta usuário
            if (resultado.HaGap && resultado.QuantidadeGap > 10)
            {
                resultado.Sucesso = true;
                resultado.Mensagem = "Há muitos sorteios faltantes. Por favor, importe o arquivo Excel com o histórico completo.";
                resultado.NovosSorteios = 0;
                return resultado;
            }

            // Se não há novos sorteios
            if (!verificacao.UltimoConcursoApi.HasValue ||
                verificacao.UltimoConcursoBanco.HasValue &&
                verificacao.UltimoConcursoApi.Value <= verificacao.UltimoConcursoBanco.Value)
            {
                resultado.Sucesso = true;
                resultado.Mensagem = "Banco de dados já está atualizado.";
                resultado.NovosSorteios = 0;
                return resultado;
            }

            // Obtém o último sorteio da API
            var sorteioApi = await ConsultarUltimoSorteioAsync(cancellationToken);

            if (sorteioApi == null)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("Não foi possível obter o último sorteio da API");
                return resultado;
            }

            // Verifica se já existe
            var existe = await _sorteioRepository.ExisteConcursoAsync(sorteioApi.Concurso, cancellationToken);

            if (existe)
            {
                resultado.Sucesso = true;
                resultado.Mensagem = "Sorteio já existe no banco de dados.";
                resultado.NovosSorteios = 0;
                return resultado;
            }

            // Insere o novo sorteio
            var novoSorteio = new Sorteio
            {
                Concurso = sorteioApi.Concurso,
                Data = sorteioApi.Data,
                Dezena1 = sorteioApi.Dezenas[0],
                Dezena2 = sorteioApi.Dezenas[1],
                Dezena3 = sorteioApi.Dezenas[2],
                Dezena4 = sorteioApi.Dezenas[3],
                Dezena5 = sorteioApi.Dezenas[4],
                Dezena6 = sorteioApi.Dezenas[5],
                Acumulado = sorteioApi.Acumulado,
                PremioSena = sorteioApi.PremioSena,
                GanhadoresSena = sorteioApi.GanhadoresSena
            };

            await _sorteioRepository.AdicionarAsync(novoSorteio, cancellationToken);
            await _sorteioRepository.SalvarAlteracoesAsync(cancellationToken);

            resultado.Sucesso = true;
            resultado.NovosSorteios = 1;
            resultado.Mensagem = "Atualização concluída com sucesso.";
            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro ao atualizar: {ex.Message}");
            return resultado;
        }
    }

    private SorteioApiResultado? ParseSorteioDaApi(string jsonContent)
    {
        try
        {
            var json = JsonNode.Parse(jsonContent);
            if (json == null)
                return null;

            // Extrai número do concurso
            var concursoStr = json["numero"]?.ToString();
            if (!int.TryParse(concursoStr, out var concurso))
                return null;

            // Extrai data
            var dataStr = json["dataApuracao"]?.ToString();
            DateOnly data = default;
            if (!string.IsNullOrEmpty(dataStr))
            {
                // Formato esperado: "31/12/2024"
                if (DateTime.TryParseExact(dataStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var dataDateTime))
                {
                    data = DateOnly.FromDateTime(dataDateTime);
                }
            }

            // Extrai dezenas
            var dezenasNode = json["dezenas"];
            var dezenas = new List<byte>();
            if (dezenasNode is JsonArray dezenasArray)
            {
                foreach (var dezenaNode in dezenasArray)
                {
                    if (byte.TryParse(dezenaNode?.ToString(), out var dezena))
                    {
                        dezenas.Add(dezena);
                    }
                }
            }

            if (dezenas.Count != 6)
                return null;

            dezenas.Sort();

            // Extrai informações de acumulação
            var acumulado = json["acumulado"]?.ToString()?.ToLowerInvariant() == "true";

            // Extrai ganhadores e prêmio da Sena
            var ganhadoresSena = 0;
            var ganhadoresStr = json["ganhadores6Numeros"]?.ToString();
            int.TryParse(ganhadoresStr, out ganhadoresSena);

            decimal? premioSena = null;
            var premioStr = json["premio6Numeros"]?.ToString();
            if (!string.IsNullOrEmpty(premioStr))
            {
                // Formato pode ser com vírgula como separador decimal
                premioStr = premioStr.Replace(".", "").Replace(",", ".");
                if (decimal.TryParse(premioStr, out var premio))
                {
                    premioSena = premio;
                }
            }

            return new SorteioApiResultado
            {
                Concurso = concurso,
                Data = data,
                Dezenas = dezenas.ToArray(),
                Acumulado = acumulado,
                GanhadoresSena = ganhadoresSena,
                PremioSena = premioSena
            };
        }
        catch
        {
            return null;
        }
    }
}