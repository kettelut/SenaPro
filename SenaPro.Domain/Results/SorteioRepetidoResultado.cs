namespace SenaPro.Domain.Results;

/// <summary>
/// Informações sobre um par de sorteios repetidos.
/// </summary>
public class SorteioRepetidoInfo
{
    /// <summary>
    /// Número do primeiro concurso.
    /// </summary>
    public int Concurso1 { get; set; }

    /// <summary>
    /// Data do primeiro concurso.
    /// </summary>
    public DateOnly Data1 { get; set; }

    /// <summary>
    /// Número do segundo concurso.
    /// </summary>
    public int Concurso2 { get; set; }

    /// <summary>
    /// Data do segundo concurso.
    /// </summary>
    public DateOnly Data2 { get; set; }

    /// <summary>
    /// Dezenas sorteadas (iguais em ambos os concursos).
    /// </summary>
    public byte[] Dezenas { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Resultado da análise de sorteios repetidos.
/// </summary>
public class SorteiosRepetidosResultado
{
    /// <summary>
    /// Indica se a análise foi bem-sucedida.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Indica se existem sorteios repetidos.
    /// </summary>
    public bool ExistemRepetidos { get; set; }

    /// <summary>
    /// Quantidade total de pares de sorteios repetidos encontrados.
    /// </summary>
    public int QuantidadePares { get; set; }

    /// <summary>
    /// Lista de pares de sorteios repetidos encontrados.
    /// </summary>
    public List<SorteioRepetidoInfo> Pares { get; set; } = new();

    /// <summary>
    /// Lista de erros encontrados.
    /// </summary>
    public List<string> Erros { get; set; } = new();
}