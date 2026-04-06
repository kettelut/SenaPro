using OfficeOpenXml;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.Application.Services;

/// <summary>
/// Serviço de importação de arquivo Excel com histórico de sorteios da Mega-Sena.
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly ISorteioRepository _sorteioRepository;

    // Colunas esperadas no Excel da Caixa
    private static readonly string[] ColunasObrigatorias = new[]
    {
        "Concurso", "Data Sorteio", "Dezena1", "Dezena2", "Dezena3", "Dezena4", "Dezena5", "Dezena6"
    };

    public ExcelImportService(ISorteioRepository sorteioRepository)
    {
        _sorteioRepository = sorteioRepository;
    }

    public async Task<bool> ValidarFormatoAsync(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            return false;

        var extensao = Path.GetExtension(caminhoArquivo).ToLowerInvariant();
        if (extensao != ".xlsx" && extensao != ".xls")
            return false;

        try
        {
            using var package = new ExcelPackage(new FileInfo(caminhoArquivo));
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null || worksheet.Dimension == null)
                return false;

            // Verifica se as colunas obrigatórias existem
            var colunas = ObterNomesColunas(worksheet);
            foreach (var coluna in ColunasObrigatorias)
            {
                if (!colunas.Any(c => c.Equals(coluna, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ImportacaoResultado> ImportarAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        var resultado = new ImportacaoResultado();

        // Validações iniciais
        if (!File.Exists(caminhoArquivo))
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Arquivo não encontrado: {caminhoArquivo}");
            return resultado;
        }

        var extensao = Path.GetExtension(caminhoArquivo).ToLowerInvariant();
        if (extensao != ".xlsx" && extensao != ".xls")
        {
            resultado.Sucesso = false;
            resultado.Erros.Add("Arquivo inválido. Formato esperado: .xlsx ou .xls");
            return resultado;
        }

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(caminhoArquivo));
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null || worksheet.Dimension == null)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("Arquivo vazio ou inválido");
                return resultado;
            }

            // Mapeia índices das colunas
            var indicesColunas = MapearColunas(worksheet);
            foreach (var coluna in ColunasObrigatorias)
            {
                if (!indicesColunas.ContainsKey(coluna))
                {
                    resultado.Sucesso = false;
                    resultado.Erros.Add($"Coluna obrigatória ausente: {coluna}");
                    return resultado;
                }
            }

            // Processa linhas
            var totalLinhas = worksheet.Dimension.End.Row;
            var concursosProcessados = new HashSet<int>();
            var sorteiosParaAdicionar = new List<Sorteio>();

            for (int linha = 2; linha <= totalLinhas; linha++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sorteio = ProcessarLinha(worksheet, linha, indicesColunas, resultado.Erros);
                if (sorteio == null)
                    continue;

                // Verifica duplicata no arquivo
                if (concursosProcessados.Contains(sorteio.Concurso))
                {
                    resultado.RegistrosIgnorados++;
                    continue;
                }

                // Verifica duplicata no banco
                var existeNoBanco = await _sorteioRepository.ExisteConcursoAsync(sorteio.Concurso, cancellationToken);

                if (existeNoBanco)
                {
                    resultado.RegistrosIgnorados++;
                    continue;
                }

                sorteiosParaAdicionar.Add(sorteio);
                resultado.RegistrosInseridos++;
                concursosProcessados.Add(sorteio.Concurso);
            }

            if (sorteiosParaAdicionar.Count > 0)
            {
                await _sorteioRepository.AdicionarVariosAsync(sorteiosParaAdicionar, cancellationToken);
                await _sorteioRepository.SalvarAlteracoesAsync(cancellationToken);
            }

            resultado.Sucesso = resultado.RegistrosInseridos > 0 || resultado.RegistrosIgnorados > 0;
            resultado.Mensagem = resultado.RegistrosInseridos > 0
                ? $"Importação concluída. {resultado.RegistrosInseridos} registros importados."
                : "Nenhum registro novo para importar.";

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro ao processar arquivo: {ex.Message}");
            return resultado;
        }
    }

    private Dictionary<string, int> MapearColunas(ExcelWorksheet worksheet)
    {
        var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalColunas = worksheet.Dimension.End.Column;

        for (int coluna = 1; coluna <= totalColunas; coluna++)
        {
            var nome = worksheet.Cells[1, coluna].Text?.Trim();
            if (!string.IsNullOrEmpty(nome))
            {
                indices[nome] = coluna;
            }
        }

        return indices;
    }

    private List<string> ObterNomesColunas(ExcelWorksheet worksheet)
    {
        var colunas = new List<string>();
        var totalColunas = worksheet.Dimension?.End.Column ?? 0;

        for (int coluna = 1; coluna <= totalColunas; coluna++)
        {
            var nome = worksheet.Cells[1, coluna].Text?.Trim();
            if (!string.IsNullOrEmpty(nome))
            {
                colunas.Add(nome);
            }
        }

        return colunas;
    }

    private Sorteio? ProcessarLinha(ExcelWorksheet worksheet, int linha, Dictionary<string, int> indices, List<string> erros)
    {
        try
        {
            // Lê concurso
            var concursoStr = worksheet.Cells[linha, indices["Concurso"]].Text;
            if (!int.TryParse(concursoStr, out var concurso) || concurso <= 0)
            {
                erros.Add($"Linha {linha}: Concurso inválido");
                return null;
            }

            // Lê data
            var dataStr = worksheet.Cells[linha, indices["Data Sorteio"]].Text;
            if (!DateTime.TryParse(dataStr, out var dataDateTime))
            {
                erros.Add($"Linha {linha}: Data inválida");
                return null;
            }
            var data = DateOnly.FromDateTime(dataDateTime);

            // Lê dezenas
            var dezenas = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                var dezenaStr = worksheet.Cells[linha, indices[$"Dezena{i + 1}"]].Text;
                if (!byte.TryParse(dezenaStr, out var dezena) || dezena < 1 || dezena > 60)
                {
                    erros.Add($"Linha {linha}: Dezena{i + 1} inválida ({dezenaStr})");
                    return null;
                }
                dezenas[i] = dezena;
            }

            // Ordena dezenas
            Array.Sort(dezenas);

            // Campos opcionais
            decimal? premioSena = null;
            var premioSenaStr = ObterValorColuna(worksheet, linha, indices, "Rateio_Sena");
            if (decimal.TryParse(premioSenaStr, out var premio))
                premioSena = premio;

            int ganhadoresSena = 0;
            var ganhadoresStr = ObterValorColuna(worksheet, linha, indices, "Ganhadores_Sena");
            if (int.TryParse(ganhadoresStr, out var ganhadores))
                ganhadoresSena = ganhadores;

            return new Sorteio
            {
                Concurso = concurso,
                Data = data,
                Dezena1 = dezenas[0],
                Dezena2 = dezenas[1],
                Dezena3 = dezenas[2],
                Dezena4 = dezenas[3],
                Dezena5 = dezenas[4],
                Dezena6 = dezenas[5],
                PremioSena = premioSena,
                GanhadoresSena = ganhadoresSena
            };
        }
        catch (Exception ex)
        {
            erros.Add($"Linha {linha}: Erro ao processar - {ex.Message}");
            return null;
        }
    }

    private string? ObterValorColuna(ExcelWorksheet worksheet, int linha, Dictionary<string, int> indices, string nomeColuna)
    {
        if (!indices.TryGetValue(nomeColuna, out var coluna))
            return null;

        return worksheet.Cells[linha, coluna].Text?.Trim();
    }
}