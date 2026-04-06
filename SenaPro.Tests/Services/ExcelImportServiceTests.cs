using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SenaPro.Application.Services;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Data;
using SenaPro.Tests.Repositories;
using Xunit;

namespace SenaPro.Tests.Services;

/// <summary>
/// Testes para o serviço de importação de Excel.
/// Seguindo TDD: Red -> Green -> Refactor
/// </summary>
public class ExcelImportServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ISorteioRepository _sorteioRepository;
    private readonly IExcelImportService _excelImportService;
    private readonly string _testFilesDirectory;

    public ExcelImportServiceTests()
    {
        // Configuração do banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sorteioRepository = new SorteioRepositoryTests(_context);

        // Configura licença do EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Instância do serviço
        _excelImportService = new ExcelImportService(_sorteioRepository);

        // Diretório para arquivos de teste
        _testFilesDirectory = Path.Combine(Path.GetTempPath(), "SenaProTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testFilesDirectory);

        // Criar arquivos de teste
        CriarArquivosTeste();
    }

    private void CriarArquivosTeste()
    {
        // Arquivo válido principal
        CriarArquivoExcelValido("Mega-Sena.xlsx", 100);

        // Arquivo vazio
        CriarArquivoExcelVazio("arquivo-vazio.xlsx");

        // Arquivo sem colunas obrigatórias
        CriarArquivoExcelSemColunas("arquivo-sem-colunas.xlsx");

        // Arquivo com dezena inválida
        CriarArquivoExcelComDezenaInvalida("arquivo-dezena-invalida.xlsx");

        // Arquivo com concurso duplicado
        CriarArquivoExcelComConcursoDuplicado("arquivo-concurso-duplicado.xlsx");

        // Arquivo com campos opcionais vazios
        CriarArquivoExcelCamposOpcionais("arquivo-campos-opcionais.xlsx");

        // Arquivo grande
        CriarArquivoExcelValido("arquivo-grande.xlsx", 1500);
    }

    private void CriarArquivoExcelValido(string nomeArquivo, int quantidadeLinhas)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Mega-Sena");

        // Cabeçalhos
        worksheet.Cells[1, 1].Value = "Concurso";
        worksheet.Cells[1, 2].Value = "Data Sorteio";
        worksheet.Cells[1, 3].Value = "Dezena1";
        worksheet.Cells[1, 4].Value = "Dezena2";
        worksheet.Cells[1, 5].Value = "Dezena3";
        worksheet.Cells[1, 6].Value = "Dezena4";
        worksheet.Cells[1, 7].Value = "Dezena5";
        worksheet.Cells[1, 8].Value = "Dezena6";
        worksheet.Cells[1, 9].Value = "Ganhadores_Sena";
        worksheet.Cells[1, 10].Value = "Rateio_Sena";

        var random = new Random(42); // Seed fixa para reprodutibilidade
        var dezenasUsadas = new HashSet<string>();

        for (int i = 2; i <= quantidadeLinhas + 1; i++)
        {
            var concurso = i - 1;
            var data = new DateOnly(2020, 1, 1).AddDays(concurso);

            // Gerar dezenas únicas para cada linha
            byte[] dezenas;
            string dezenasKey;
            do
            {
                dezenas = new byte[6];
                for (int d = 0; d < 6; d++)
                {
                    dezenas[d] = (byte)(random.Next(1, 61));
                }
                Array.Sort(dezenas);
                dezenasKey = string.Join(",", dezenas);
            } while (dezenasUsadas.Contains(dezenasKey));

            dezenasUsadas.Add(dezenasKey);

            worksheet.Cells[i, 1].Value = concurso;
            worksheet.Cells[i, 2].Value = data.ToString("dd/MM/yyyy");
            worksheet.Cells[i, 3].Value = dezenas[0];
            worksheet.Cells[i, 4].Value = dezenas[1];
            worksheet.Cells[i, 5].Value = dezenas[2];
            worksheet.Cells[i, 6].Value = dezenas[3];
            worksheet.Cells[i, 7].Value = dezenas[4];
            worksheet.Cells[i, 8].Value = dezenas[5];
            worksheet.Cells[i, 9].Value = random.Next(0, 5);
            worksheet.Cells[i, 10].Value = random.Next(1000000, 50000000);
        }

        package.SaveAs(new FileInfo(caminho));
    }

    private void CriarArquivoExcelVazio(string nomeArquivo)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        // Apenas cabeçalho, sem dados
        worksheet.Cells[1, 1].Value = "Concurso";
        package.SaveAs(new FileInfo(caminho));
    }

    private void CriarArquivoExcelSemColunas(string nomeArquivo)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        // Colunas diferentes das esperadas
        worksheet.Cells[1, 1].Value = "Numero";
        worksheet.Cells[1, 2].Value = "Data";
        worksheet.Cells[1, 3].Value = "Numeros";
        package.SaveAs(new FileInfo(caminho));
    }

    private void CriarArquivoExcelComDezenaInvalida(string nomeArquivo)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Mega-Sena");

        worksheet.Cells[1, 1].Value = "Concurso";
        worksheet.Cells[1, 2].Value = "Data Sorteio";
        worksheet.Cells[1, 3].Value = "Dezena1";
        worksheet.Cells[1, 4].Value = "Dezena2";
        worksheet.Cells[1, 5].Value = "Dezena3";
        worksheet.Cells[1, 6].Value = "Dezena4";
        worksheet.Cells[1, 7].Value = "Dezena5";
        worksheet.Cells[1, 8].Value = "Dezena6";

        // Linha válida
        worksheet.Cells[2, 1].Value = 1000;
        worksheet.Cells[2, 2].Value = "01/01/2024";
        worksheet.Cells[2, 3].Value = 1;
        worksheet.Cells[2, 4].Value = 10;
        worksheet.Cells[2, 5].Value = 20;
        worksheet.Cells[2, 6].Value = 30;
        worksheet.Cells[2, 7].Value = 40;
        worksheet.Cells[2, 8].Value = 50;

        // Linha com dezena inválida (fora do intervalo 1-60)
        worksheet.Cells[3, 1].Value = 1001;
        worksheet.Cells[3, 2].Value = "02/01/2024";
        worksheet.Cells[3, 3].Value = 70; // Inválido
        worksheet.Cells[3, 4].Value = 10;
        worksheet.Cells[3, 5].Value = 20;
        worksheet.Cells[3, 6].Value = 30;
        worksheet.Cells[3, 7].Value = 40;
        worksheet.Cells[3, 8].Value = 50;

        // Linha válida
        worksheet.Cells[4, 1].Value = 1002;
        worksheet.Cells[4, 2].Value = "03/01/2024";
        worksheet.Cells[4, 3].Value = 5;
        worksheet.Cells[4, 4].Value = 15;
        worksheet.Cells[4, 5].Value = 25;
        worksheet.Cells[4, 6].Value = 35;
        worksheet.Cells[4, 7].Value = 45;
        worksheet.Cells[4, 8].Value = 55;

        package.SaveAs(new FileInfo(caminho));
    }

    private void CriarArquivoExcelComConcursoDuplicado(string nomeArquivo)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Mega-Sena");

        worksheet.Cells[1, 1].Value = "Concurso";
        worksheet.Cells[1, 2].Value = "Data Sorteio";
        worksheet.Cells[1, 3].Value = "Dezena1";
        worksheet.Cells[1, 4].Value = "Dezena2";
        worksheet.Cells[1, 5].Value = "Dezena3";
        worksheet.Cells[1, 6].Value = "Dezena4";
        worksheet.Cells[1, 7].Value = "Dezena5";
        worksheet.Cells[1, 8].Value = "Dezena6";

        // Primeira linha
        worksheet.Cells[2, 1].Value = 1000;
        worksheet.Cells[2, 2].Value = "01/01/2024";
        worksheet.Cells[2, 3].Value = 1;
        worksheet.Cells[2, 4].Value = 10;
        worksheet.Cells[2, 5].Value = 20;
        worksheet.Cells[2, 6].Value = 30;
        worksheet.Cells[2, 7].Value = 40;
        worksheet.Cells[2, 8].Value = 50;

        // Linha duplicada (mesmo concurso)
        worksheet.Cells[3, 1].Value = 1000;
        worksheet.Cells[3, 2].Value = "01/01/2024";
        worksheet.Cells[3, 3].Value = 2;
        worksheet.Cells[3, 4].Value = 11;
        worksheet.Cells[3, 5].Value = 21;
        worksheet.Cells[3, 6].Value = 31;
        worksheet.Cells[3, 7].Value = 41;
        worksheet.Cells[3, 8].Value = 51;

        // Terceira linha
        worksheet.Cells[4, 1].Value = 1001;
        worksheet.Cells[4, 2].Value = "02/01/2024";
        worksheet.Cells[4, 3].Value = 3;
        worksheet.Cells[4, 4].Value = 12;
        worksheet.Cells[4, 5].Value = 22;
        worksheet.Cells[4, 6].Value = 32;
        worksheet.Cells[4, 7].Value = 42;
        worksheet.Cells[4, 8].Value = 52;

        package.SaveAs(new FileInfo(caminho));
    }

    private void CriarArquivoExcelCamposOpcionais(string nomeArquivo)
    {
        var caminho = Path.Combine(_testFilesDirectory, nomeArquivo);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Mega-Sena");

        // Apenas colunas obrigatórias
        worksheet.Cells[1, 1].Value = "Concurso";
        worksheet.Cells[1, 2].Value = "Data Sorteio";
        worksheet.Cells[1, 3].Value = "Dezena1";
        worksheet.Cells[1, 4].Value = "Dezena2";
        worksheet.Cells[1, 5].Value = "Dezena3";
        worksheet.Cells[1, 6].Value = "Dezena4";
        worksheet.Cells[1, 7].Value = "Dezena5";
        worksheet.Cells[1, 8].Value = "Dezena6";

        worksheet.Cells[2, 1].Value = 2000;
        worksheet.Cells[2, 2].Value = "01/06/2024";
        worksheet.Cells[2, 3].Value = 5;
        worksheet.Cells[2, 4].Value = 15;
        worksheet.Cells[2, 5].Value = 25;
        worksheet.Cells[2, 6].Value = 35;
        worksheet.Cells[2, 7].Value = 45;
        worksheet.Cells[2, 8].Value = 55;

        package.SaveAs(new FileInfo(caminho));
    }

    private string ObterCaminhoArquivo(string nomeArquivo)
    {
        return Path.Combine(_testFilesDirectory, nomeArquivo);
    }

    #region Testes de Formato e Validação

    [Fact]
    public async Task ValidarFormatoAsync_DeveRetornarTrueParaArquivoValido()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Act
        var resultado = await _excelImportService.ValidarFormatoAsync(caminhoArquivo);

        // Assert
        resultado.Should().BeTrue("o arquivo Excel possui o formato esperado");
    }

    [Fact]
    public async Task ValidarFormatoAsync_DeveRetornarFalseParaArquivoInexistente()
    {
        // Arrange
        var caminhoArquivo = "arquivo-inexistente.xlsx";

        // Act
        var resultado = await _excelImportService.ValidarFormatoAsync(caminhoArquivo);

        // Assert
        resultado.Should().BeFalse("o arquivo não existe");
    }

    [Fact]
    public async Task ValidarFormatoAsync_DeveRetornarFalseParaArquivoNaoExcel()
    {
        // Arrange
        var caminhoArquivo = Path.Combine(_testFilesDirectory, "arquivo.txt");
        File.WriteAllText(caminhoArquivo, "conteúdo de teste");

        // Act
        var resultado = await _excelImportService.ValidarFormatoAsync(caminhoArquivo);

        // Assert
        resultado.Should().BeFalse("o arquivo não é um Excel válido");
    }

    #endregion

    #region Testes de Importação - Cenários de Sucesso

    [Fact]
    public async Task ImportarAsync_DeveInserirNovosSorteios()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("a importação deve ser bem-sucedida");
        resultado.RegistrosInseridos.Should().BeGreaterThan(0, "deve haver sorteios novos para importar");
        resultado.Erros.Should().BeEmpty("não deve haver erros na importação");
    }

    [Fact]
    public async Task ImportarAsync_DeveIgnorarSorteiosJaExistente()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Primeira importação
        await _excelImportService.ImportarAsync(caminhoArquivo);

        // Segunda importação do mesmo arquivo
        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("a importação deve ser bem-sucedida");
        resultado.RegistrosInseridos.Should().Be(0, "não deve inserir duplicatas");
        resultado.RegistrosIgnorados.Should().BeGreaterThan(0, "deve identificar registros já existentes");
    }

    [Fact]
    public async Task ImportarAsync_DeveValidarDadosAntesDeInserir()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Act
        await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert - Verifica se os dados foram corretamente mapeados
        var sorteios = await _sorteioRepository.ObterTodosAsync();

        foreach (var sorteio in sorteios)
        {
            sorteio.Concurso.Should().BeGreaterThan(0, "concurso deve ser positivo");
            sorteio.Data.Should().NotBe(default, "data deve ser válida");

            var dezenas = sorteio.GetDezenas();
            dezenas.Should().HaveCount(6, "sorteio deve ter 6 dezenas");
            dezenas.All(d => d >= 1 && d <= 60).Should().BeTrue("dezenas devem estar entre 1 e 60");
        }
    }

    #endregion

    #region Testes de Tratamento de Erros

    [Fact]
    public async Task ImportarAsync_DeveRetornarErroParaArquivoInexistente()
    {
        // Arrange
        var caminhoArquivo = "arquivo-inexistente.xlsx";

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeFalse("o arquivo não existe");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
        resultado.Erros[0].Should().Contain("não encontrado", "deve indicar que o arquivo não foi encontrado");
    }

    [Fact]
    public async Task ImportarAsync_DeveRetornarErroParaArquivoVazio()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-vazio.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeFalse("o arquivo está vazio");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
    }

    [Fact]
    public async Task ImportarAsync_DeveRetornarErroParaColunasObrigatoriasAusentes()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-sem-colunas.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeFalse("colunas obrigatórias ausentes");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
        resultado.Erros[0].Should().Contain("Coluna", "deve indicar qual coluna está faltando");
    }

    [Fact]
    public async Task ImportarAsync_DeveRetornarErroParaArquivoNaoExcel()
    {
        // Arrange
        var caminhoArquivo = Path.Combine(_testFilesDirectory, "arquivo.txt");
        File.WriteAllText(caminhoArquivo, "conteúdo de teste");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeFalse("o arquivo não é um Excel");
        resultado.Erros.Should().NotBeEmpty("deve conter mensagem de erro");
    }

    #endregion

    #region Testes de Validação de Dados

    [Fact]
    public async Task ImportarAsync_DeveIgnorarLinhaComDezenaInvalida()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-dezena-invalida.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("deve processar linhas válidas");
        resultado.Erros.Should().NotBeEmpty("deve registrar erro para linha inválida");
        resultado.RegistrosInseridos.Should().BeGreaterThan(0, "deve inserir linhas válidas");
    }

    [Fact]
    public async Task ImportarAsync_DeveIgnorarLinhaComConcursoDuplicadoNoArquivo()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-concurso-duplicado.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("deve processar linhas válidas");
        // Deve inserir apenas 2 registros (o primeiro concurso 1000 e o 1001, o segundo 1000 é duplicado)
        resultado.RegistrosInseridos.Should().Be(2, "deve ignorar linhas duplicadas");
    }

    [Fact]
    public async Task ImportarAsync_DeveProcessarLinhaComCamposOpcionaisVazios()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-campos-opcionais.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("campos opcionais podem estar vazios");
        resultado.RegistrosInseridos.Should().BeGreaterThan(0, "deve inserir registros com campos opcionais vazios");
    }

    [Fact]
    public async Task ImportarAsync_DeveOrdenarDezenasAntesDeSalvar()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Act
        await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        var sorteios = await _sorteioRepository.ObterTodosAsync();

        foreach (var sorteio in sorteios)
        {
            var dezenas = new[] { sorteio.Dezena1, sorteio.Dezena2, sorteio.Dezena3, sorteio.Dezena4, sorteio.Dezena5, sorteio.Dezena6 };
            dezenas.Should().BeInAscendingOrder("dezenas devem ser armazenadas ordenadas");
        }
    }

    #endregion

    #region Testes de Casos de Borda

    [Fact]
    public async Task ImportarAsync_DeveProcessarArquivoGrande()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("arquivo-grande.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Sucesso.Should().BeTrue("deve processar arquivo grande");
        resultado.RegistrosInseridos.Should().BeGreaterThan(1000, "deve inserir todos os registros");
    }

    [Fact]
    public async Task ImportarAsync_DeveRetornarMensagemInformativa()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Act
        var resultado = await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        resultado.Mensagem.Should().NotBeNullOrEmpty("deve conter mensagem informativa");
        resultado.Mensagem.Should().Contain("importado", "deve indicar que houve importação");
    }

    [Fact]
    public async Task ImportarAsync_DeveManterDadosExistentes()
    {
        // Arrange
        var caminhoArquivo = ObterCaminhoArquivo("Mega-Sena.xlsx");

        // Primeira importação
        await _excelImportService.ImportarAsync(caminhoArquivo);
        var totalEsperado = await _sorteioRepository.ContarAsync();

        // Act - Segunda importação do mesmo arquivo
        await _excelImportService.ImportarAsync(caminhoArquivo);

        // Assert
        var totalAtual = await _sorteioRepository.ContarAsync();
        totalAtual.Should().Be(totalEsperado, "não deve duplicar registros existentes");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();

        // Limpar arquivos de teste
        if (Directory.Exists(_testFilesDirectory))
        {
            try
            {
                Directory.Delete(_testFilesDirectory, recursive: true);
            }
            catch
            {
                // Ignorar erros na limpeza
            }
        }
    }
}