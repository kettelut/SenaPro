namespace SenaPro.Domain.Results;

/// <summary>
/// Dados de um sorteio retornados pela API da loteria.
/// </summary>
public class SorteioApiResultado
{
    /// <summary>
    /// Número do concurso.
    /// </summary>
    public int Concurso { get; set; }

    /// <summary>
    /// Data do sorteio.
    /// </summary>
    public DateOnly Data { get; set; }

    /// <summary>
    /// Dezenas sorteadas (ordenadas).
    /// </summary>
    public byte[] Dezenas { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Indica se acumulou.
    /// </summary>
    public bool Acumulado { get; set; }

    /// <summary>
    /// Valor do prêmio da Sena.
    /// </summary>
    public decimal? PremioSena { get; set; }

    /// <summary>
    /// Quantidade de ganhadores da Sena.
    /// </summary>
    public int GanhadoresSena { get; set; }
}