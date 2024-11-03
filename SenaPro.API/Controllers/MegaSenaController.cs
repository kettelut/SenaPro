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
			_logger.LogInformation($@"Obtém todos os resultados da Mega-Sena.");
			var result = _SenaProAppService.ObterTodosSorteios();
			return Ok(result);
		}

        [HttpGet("ObterUltimoResultado")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObterUltimoResultado()
        {
            _logger.LogInformation($@"Obtém o último resultado da Mega-Sena.");
            var result = _SenaProAppService.ObterUltimoSorteio();
            return Ok(result);
        }

        [HttpGet("ObterCincoNumerosMaisSorteados")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FrequenciaNumeral>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObterCincoNumerosMaisSorteados()
        {
            _logger.LogInformation($@"Obtém os 5 números mais sorteados da Mega-Sena.");
            var result = _SenaProAppService.ObterCincoNumerosMaisSorteados();
            return Ok(result);
        }

        [HttpGet("ObterCincoNumerosMenosSorteados")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FrequenciaNumeral>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObterCincoNumerosMenosSorteados()
        {
            _logger.LogInformation($@"Obtém os 5 números menos sorteados da Mega-Sena.");
            var result = _SenaProAppService.ObterCincoNumerosMenosSorteados();
            return Ok(result);
        }

        [HttpGet("ObterSomasMaisSorteados")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FrequenciaNumeral>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObterSomasMaisSorteados()
        {
            _logger.LogInformation($@"Obtém os top soma números mais sorteados da Mega-Sena.");
            var result = _SenaProAppService.ObterTopSomaNumerosSorteiosAteMetade();
            return Ok(result);
        }

        [HttpGet("ParesDeNumerosQueMaisAparecemJuntos")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ParesNumerais>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ParesDeNumerosQueMaisAparecemJuntos()
        {
            _logger.LogInformation($@"Obtém os pares de números que mais aparecem juntos da Mega-Sena.");
            var result = _SenaProAppService.ParesDeNumerosQueMaisAparecemJuntos();
            return Ok(result);
        }

        [HttpGet("CalcularQntDeSorteiosAnterioresParaLocalizarNumeros")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CalculoSorteiosAnteriores>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CalcularQntDeSorteiosAnterioresParaLocalizarNumeros()
        {
            _logger.LogInformation($@"Obtém a quantidade de sorteios anteriores para localizar uma quantiade de números.");
            var result = _SenaProAppService.CalcularQntDeSorteiosAnterioresParaLocalizarNumeros();
            return Ok(result);
        }

        [HttpGet("ObterSugetaoParaProximoSorteio")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<List<int>>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObterSugetaoParaProximoSorteio([FromQuery] int qntNumerosPorJogo, int qntDeJogos)
        {
            _logger.LogInformation($@"Obtém a quantidade de sorteios anteriores para localizar uma quantiade de números.");
            var result = _SenaProAppService.ObterSugetaoParaProximoSorteio(qntNumerosPorJogo, qntDeJogos);
            return Ok(result);
        }
    }
}