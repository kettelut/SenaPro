namespace SenaPro.Domain.Entities;

/// <summary>
/// Representa um sorteio da Mega-Sena.
/// </summary>
public class Sorteio
{
    public int Id { get; set; }

    /// <summary>
    /// Número do concurso.
    /// </summary>
    public int Concurso { get; set; }

    /// <summary>
    /// Data do sorteio.
    /// </summary>
    public DateOnly Data { get; set; }

    /// <summary>
    /// Data do próximo concurso.
    /// </summary>
    public DateOnly? DataProximoConcurso { get; set; }

    /// <summary>
    /// Local onde foi realizado o sorteio.
    /// </summary>
    public string? LocalSorteio { get; set; }

    /// <summary>
    /// Município e UF onde foi realizado o sorteio.
    /// </summary>
    public string? MunicipioUFSorteio { get; set; }

    /// <summary>
    /// Primeira dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena1 { get; set; }

    /// <summary>
    /// Segunda dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena2 { get; set; }

    /// <summary>
    /// Terceira dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena3 { get; set; }

    /// <summary>
    /// Quarta dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena4 { get; set; }

    /// <summary>
    /// Quinta dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena5 { get; set; }

    /// <summary>
    /// Sexta dezena sorteada (ordenada).
    /// </summary>
    public byte Dezena6 { get; set; }

    /// <summary>
    /// Dezenas na ordem em que foram sorteadas.
    /// </summary>
    public byte[]? DezenasOrdemSorteio { get; set; }

    /// <summary>
    /// Indica se o prêmio acumulou.
    /// </summary>
    public bool Acumulado { get; set; }

    /// <summary>
    /// Valor arrecadado no concurso.
    /// </summary>
    public decimal? ValorArrecadado { get; set; }

    /// <summary>
    /// Valor acumulado para o próximo concurso.
    /// </summary>
    public decimal? ValorAcumuladoProximoConcurso { get; set; }

    /// <summary>
    /// Valor estimado para o próximo concurso.
    /// </summary>
    public decimal? ValorEstimadoProximoConcurso { get; set; }

    /// <summary>
    /// Valor do prêmio da Sena (6 acertos).
    /// </summary>
    public decimal? PremioSena { get; set; }

    /// <summary>
    /// Quantidade de ganhadores da Sena.
    /// </summary>
    public int GanhadoresSena { get; set; }

    /// <summary>
    /// Valor do prêmio da Quina (5 acertos).
    /// </summary>
    public decimal? PremioQuina { get; set; }

    /// <summary>
    /// Quantidade de ganhadores da Quina.
    /// </summary>
    public int GanhadoresQuina { get; set; }

    /// <summary>
    /// Valor do prêmio da Quadra (4 acertos).
    /// </summary>
    public decimal? PremioQuadra { get; set; }

    /// <summary>
    /// Quantidade de ganhadores da Quadra.
    /// </summary>
    public int GanhadoresQuadra { get; set; }

    /// <summary>
    /// Indica se o sorteio já foi conferido.
    /// </summary>
    public bool Conferido { get; set; }

    /// <summary>
    /// Retorna as dezenas ordenadas como array.
    /// </summary>
    public byte[] GetDezenas()
    {
        return new byte[] { Dezena1, Dezena2, Dezena3, Dezena4, Dezena5, Dezena6 }
            .OrderBy(d => d)
            .ToArray();
    }
}