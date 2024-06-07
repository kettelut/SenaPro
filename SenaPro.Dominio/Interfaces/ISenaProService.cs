using SenaPro.Dominio.Entidades;

namespace SenaPro.Dominio.Interfaces
{
    public interface ISenaProService
	{
		ResultadoCdb Calcular(Cdb cdb);
	}
}
