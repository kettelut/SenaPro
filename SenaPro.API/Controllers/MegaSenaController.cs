using SenaPro.Application.Interfaces;
using SenaPro.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SenaPro.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MegaSenaController : ControllerBase
	{
		private readonly ISenaProAppService _SenaProAppService;

		private readonly ILogger<MegaSenaController> _logger;

		public MegaSenaController(ILogger<MegaSenaController> logger, ISenaProAppService SenaProAppService)
		{
			_logger = logger;
			_SenaProAppService = SenaProAppService;
		}

		[HttpGet("ObterResultados")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Sorteio>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult ObterResultados()
		{
			_logger.LogInformation($@"Obt�m todos os resultados da Mega-Sena.");

			var result = _SenaProAppService.ObterSorteios();
			return Ok(result);
		}
	}
}