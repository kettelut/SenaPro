using SenaPro.Domain.Results;

namespace SenaPro.Domain.Interfaces;

/// <summary>
/// Interface para serviço de importação de arquivo Excel.
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Importa sorteios de um arquivo Excel.
    /// </summary>
    /// <param name="caminhoArquivo">Caminho do arquivo Excel.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado da importação.</returns>
    Task<ImportacaoResultado> ImportarAsync(string caminhoArquivo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se o arquivo Excel possui o formato esperado.
    /// </summary>
    /// <param name="caminhoArquivo">Caminho do arquivo Excel.</param>
    /// <returns>True se o formato é válido, false caso contrário.</returns>
    Task<bool> ValidarFormatoAsync(string caminhoArquivo);
}