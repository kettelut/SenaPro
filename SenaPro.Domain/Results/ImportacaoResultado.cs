namespace SenaPro.Domain.Results;

/// <summary>
/// Resultado da importação de sorteios.
/// </summary>
public class ImportacaoResultado
{
    /// <summary>
    /// Quantidade de registros novos inseridos.
    /// </summary>
    public int RegistrosInseridos { get; set; }

    /// <summary>
    /// Quantidade de registros já existentes (ignorados).
    /// </summary>
    public int RegistrosIgnorados { get; set; }

    /// <summary>
    /// Indica se a importação foi bem-sucedida.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Lista de erros encontrados durante a importação.
    /// </summary>
    public List<string> Erros { get; set; } = new();
}