using SenaPro.Aplicacao.Interfaces;
using SenaPro.Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace SenaPro.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CdbController : ControllerBase
	{
		private readonly ISenaProAppService _SenaProAppService;

		private readonly ILogger<CdbController> _logger;

		public CdbController(ILogger<CdbController> logger, ISenaProAppService SenaProAppService)
		{
			_logger = logger;
			_SenaProAppService = SenaProAppService;
		}

		[HttpGet("Calcular")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoCdb))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult Post(decimal valor, int meses)
		{
			_logger.LogInformation($@"Calcular o CDB para o Valor {valor} e para {meses} Meses.");

			if (valor <= 0)
				return BadRequest("Valor deve ser positivo");

			if(meses <= 1)
				return BadRequest("Quantidade Meses deve ser maior que 1");

			var result = _SenaProAppService.Calcular(valor, meses);
			return Ok(result);
		}
	}
}