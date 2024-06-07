using SenaPro.Dominio.Interfaces;
using SenaPro.Dominio.Entidades;

namespace SenaPro.Dominio.Services
{
    public class SenaProService : ISenaProService
	{
		private const decimal TaxaBanco = 1.08m;
		private const decimal TaxaCdi = 0.009m;

		public ResultadoCdb Calcular(Cdb cdb)
		{
			decimal valorBruto = CalcularValorBruto(cdb.Valor, cdb.Meses);
			decimal rendimento = valorBruto - cdb.Valor;
			decimal imposto = CalcularImposto(cdb.Meses, rendimento);

			var result = new ResultadoCdb
			{
				ValorBruto = Math.Round(valorBruto, 2),
				ValorLiquido = Math.Round(valorBruto - imposto, 2)
			};

			return result;
		}

		private decimal CalcularValorBruto(decimal valorInicial, int numMeses)
		{
			decimal taxaCdiBanco = TaxaCdi * TaxaBanco;
			decimal valorFinal = valorInicial;

			for (int i = 0; i < numMeses; i++)
				valorFinal *= (1 + taxaCdiBanco);
			
			return valorFinal;
		}

		public decimal CalcularImposto(int meses, decimal rendimento)
			=> meses switch
			{
				<= 6 => (rendimento * 0.225m),
				<= 12 => (rendimento * 0.20m),
				<= 24 => (rendimento * 0.175m),
				_ => (rendimento * 0.15m)
			};
	}
}
