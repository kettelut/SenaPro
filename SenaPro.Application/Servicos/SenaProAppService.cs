using SenaPro.Application.Interfaces;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Services.Interfaces;

namespace SenaPro.Application.Services
{
    public class SenaProAppService : ISenaProAppService
	{
		private readonly ISenaProService _SenaProService;

		public SenaProAppService(ISenaProService SenaProService)
		{
			_SenaProService = SenaProService;
		}

		public List<Sorteio> ObterSorteios()
		{
			return _SenaProService.ObterTodosSorteios();
		}
	}
}
