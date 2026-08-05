using Microsoft.AspNetCore.Mvc;
using SenaPro.Application.Services;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SorteiosController : ControllerBase
{
    private readonly IExcelImportService _excelImportService;
    private readonly IAnaliseEstatisticaService _analiseEstatisticaService;

    public SorteiosController(
        IExcelImportService excelImportService,
        IAnaliseEstatisticaService analiseEstatisticaService)
    {
        _excelImportService = excelImportService;
        _analiseEstatisticaService = analiseEstatisticaService;
    }

    // ── EPIC-001: Importação de Excel ────────────────────────────────

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

    // ── EPIC-002: Sorteios Repetidos ────────────────────────────────

    [HttpGet("repetidos")]
    public async Task<IActionResult> GetSorteiosRepetidos(CancellationToken cancellationToken)
    {
        var resultado = await _analiseEstatisticaService.AnalisarSorteosRepetidosAsync(cancellationToken);
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
