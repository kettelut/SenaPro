using SenaPro.Aplicacao.Interfaces;
using SenaPro.Dominio.Entidades;
using SenaPro.Dominio.Interfaces;

namespace SenaPro.Aplicacao.Servicos
{
	public class SenaProAppService : ISenaProAppService
	{
		private readonly ISenaProService _SenaProService;

		public SenaProAppService(ISenaProService SenaProService)
		{
			_SenaProService = SenaProService;
		}

		public ResultadoCdb Calcular(decimal valor, int meses)
		{
			Cdb cdb = new Cdb { 
				Valor = valor, 
				Meses = meses 
			};

			return _SenaProService.Calcular(cdb);
		}
	}
}
