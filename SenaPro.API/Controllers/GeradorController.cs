using Microsoft.AspNetCore.Mvc;
using SenaPro.Domain.Interfaces;
using SenaPro.Domain.Results;

namespace SenaPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeradorController : ControllerBase
{
    private readonly IGeradorJogosService _geradorJogosService;

    public GeradorController(IGeradorJogosService geradorJogosService)
    {
        _geradorJogosService = geradorJogosService;
    }

    [HttpGet("analises")]
    public async Task<IActionResult> GetAnalisesDisponiveis()
    {
        var analises = await _geradorJogosService.ObterAnalisesDisponiveisAsync();
        return Ok(analises);
    }

    [HttpPost("gerar")]
    public async Task<IActionResult> GerarJogos([FromBody] ConfiguracaoGeracaoJogos configuracao, CancellationToken cancellationToken)
    {
        var resultado = await _geradorJogosService.GerarJogosAsync(configuracao, cancellationToken);
        if (!resultado.Sucesso)
        {
            return BadRequest(resultado);
        }
        return Ok(resultado);
    }
}
