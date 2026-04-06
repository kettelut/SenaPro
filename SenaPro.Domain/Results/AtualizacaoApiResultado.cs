namespace SenaPro.Domain.Results;

/// <summary>
/// Resultado da atualização via API da loteria.
/// </summary>
public class AtualizacaoApiResultado
{
    /// <summary>
    /// Indica se a atualização foi bem-sucedida.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de novos sorteios inseridos.
    /// </summary>
    public int NovosSorteios { get; set; }

    /// <summary>
    /// Número do último concurso disponível na API.
    /// </summary>
    public int? UltimoConcursoApi { get; set; }

    /// <summary>
    /// Número do último concurso armazenado no banco.
    /// </summary>
    public int? UltimoConcursoBanco { get; set; }

    /// <summary>
    /// Indica se há gap (sorteios faltantes) entre o banco e a API.
    /// </summary>
    public bool HaGap { get; set; }

    /// <summary>
    /// Quantidade de sorteios faltantes (gap).
    /// </summary>
    public int QuantidadeGap { get; set; }

    /// <summary>
    /// Lista de erros encontrados.
    /// </summary>
    public List<string> Erros { get; set; } = new();
}