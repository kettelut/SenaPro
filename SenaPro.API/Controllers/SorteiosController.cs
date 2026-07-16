using Microsoft.AspNetCore.Mvc;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SorteiosController : ControllerBase
{
    private readonly ISorteioRepository _sorteioRepository;
    private readonly IApiLoteriaService _apiLoteriaService;
    private readonly IExcelImportService _excelImportService;
    private readonly IAnaliseEstatisticaService _analiseEstatisticaService;

    public SorteiosController(
        ISorteioRepository sorteioRepository,
        IApiLoteriaService apiLoteriaService,
        IExcelImportService excelImportService,
        IAnaliseEstatisticaService analiseEstatisticaService)
    {
        _sorteioRepository = sorteioRepository;
        _apiLoteriaService = apiLoteriaService;
        _excelImportService = excelImportService;
        _analiseEstatisticaService = analiseEstatisticaService;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var total = await _sorteioRepository.ContarAsync(cancellationToken);
        var apiUpdates = await _apiLoteriaService.VerificarAtualizacoesAsync(cancellationToken);

        Domain.Entities.Sorteio? ultimoSorteio = null;
        if (apiUpdates.UltimoConcursoBanco.HasValue)
        {
            ultimoSorteio = await _sorteioRepository.ObterPorConcursoAsync(apiUpdates.UltimoConcursoBanco.Value, cancellationToken);
        }

        return Ok(new
        {
            TotalSorteiosBanco = total,
            UltimoConcursoBanco = apiUpdates.UltimoConcursoBanco,
            UltimoConcursoApi = apiUpdates.UltimoConcursoApi,
            HaGap = apiUpdates.HaGap,
            QuantidadeGap = apiUpdates.QuantidadeGap,
            UltimoSorteio = ultimoSorteio != null ? new
            {
                ultimoSorteio.Concurso,
                ultimoSorteio.Data,
                Dezenas = ultimoSorteio.GetDezenas(),
                ultimoSorteio.Acumulado,
                ultimoSorteio.PremioSena,
                ultimoSorteio.GanhadoresSena
            } : null
        });
    }

    [HttpPost("atualizar-api")]
    public async Task<IActionResult> AtualizarViaApi(CancellationToken cancellationToken)
    {
        var resultado = await _apiLoteriaService.AtualizarAsync(cancellationToken);
        if (!resultado.Sucesso)
        {
            return BadRequest(resultado);
        }
        return Ok(resultado);
    }

    [HttpPost("importar-excel")]
    public async Task<IActionResult> ImportarExcel(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ImportacaoResultado
            {
                Sucesso = false,
                Erros = { "Nenhum arquivo enviado ou arquivo está vazio." }
            });
        }

        var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extensao != ".xlsx" && extensao != ".xls")
        {
            return BadRequest(new ImportacaoResultado
            {
                Sucesso = false,
                Erros = { "Formato de arquivo inválido. Apenas .xlsx e .xls são suportados." }
            });
        }

        // Cria diretório temporário no workspace
        var tempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        var tempFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}{extensao}");

        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var resultado = await _excelImportService.ImportarAsync(tempFilePath, cancellationToken);
            if (!resultado.Sucesso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
        finally
        {
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
        }
    }

    [HttpGet("repetidos")]
    public async Task<IActionResult> GetSorteiosRepetidos(CancellationToken cancellationToken)
    {
        var resultado = await _analiseEstatisticaService.AnalisarSorteiosRepetidosAsync(cancellationToken);
        if (!resultado.Sucesso)
        {
            return BadRequest(resultado);
        }
        return Ok(resultado);
    }

    [HttpPost("verificar")]
    public async Task<IActionResult> VerificarJogo([FromBody] byte[] dezenas, CancellationToken cancellationToken)
    {
        try
        {
            var jaSorteado = await _analiseEstatisticaService.VerificarDezenasJaSorteadasAsync(dezenas, cancellationToken);
            return Ok(new { JaSorteado = jaSorteado });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }
}
