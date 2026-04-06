namespace SenaPro.Domain.Results;

/// <summary>
/// Representa um jogo sugerido.
/// </summary>
public class JogoSugerido
{
    /// <summary>
    /// Identificador único do jogo sugerido.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Dezenas do jogo (ordenadas).
    /// </summary>
    public byte[] Dezenas { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Data/hora de geração do jogo.
    /// </summary>
    public DateTime DataGeracao { get; set; }
}

/// <summary>
/// Configurações para geração de jogos.
/// </summary>
public class ConfiguracaoGeracaoJogos
{
    /// <summary>
    /// Quantidade de números por jogo (padrão: 6).
    /// </summary>
    public int QuantidadeNumeros { get; set; } = 6;

    /// <summary>
    /// Quantidade de jogos a gerar.
    /// </summary>
    public int QuantidadeJogos { get; set; } = 1;

    /// <summary>
    /// Analises estatísticas a serem respeitadas.
    /// </summary>
    public List<string> AnalisesRespeitadas { get; set; } = new();
}

/// <summary>
/// Resultado da geração de sugestões de jogos.
/// </summary>
public class GeracaoJogosResultado
{
    /// <summary>
    /// Indica se a geração foi bem-sucedida.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Lista de jogos sugeridos.
    /// </summary>
    public List<JogoSugerido> Jogos { get; set; } = new();

    /// <summary>
    /// Quantidade de jogos gerados.
    /// </summary>
    public int QuantidadeGerada { get; set; }

    /// <summary>
    /// Configurações utilizadas na geração.
    /// </summary>
    public ConfiguracaoGeracaoJogos Configuracao { get; set; } = new();

    /// <summary>
    /// Lista de erros encontrados.
    /// </summary>
    public List<string> Erros { get; set; } = new();
}