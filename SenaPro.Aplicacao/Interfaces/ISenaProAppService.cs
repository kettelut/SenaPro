using SenaPro.Dominio.Entidades;

namespace SenaPro.Aplicacao.Interfaces
{
    public interface ISenaProAppService
	{
		ResultadoCdb Calcular(decimal valor, int meses);
	}
}
