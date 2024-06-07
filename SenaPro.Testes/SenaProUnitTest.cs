using SenaPro.Dominio.Entidades;
using SenaPro.Dominio.Services;

namespace SenaPro.Testes
{
	public class SenaProUnitTest
	{
		private readonly SenaProService _SenaProService;

		public SenaProUnitTest()
		{
			_SenaProService = new SenaProService();
		}

		[Fact]
		public void Calcular6MesesInvestimentoCdb()
		{
			// Arrange
			var cdb = new Cdb
			{
				Valor = 12000,
				Meses = 6
			};

			// Act
			var result = _SenaProService.Calcular(cdb);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(12717.07m, result.ValorBruto, 4);
			Assert.Equal(12555.73m, result.ValorLiquido, 4);
		}

		[Fact]
		public void Calcular12MesesInvestimentoCdb()
		{
			// Arrange
			var cdb = new Cdb
			{
				Valor = 1000,
				Meses = 12
			};

			// Act
			var result = _SenaProService.Calcular(cdb);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(1123.08m, result.ValorBruto, 4);
			Assert.Equal(1098.47m, result.ValorLiquido, 4);
		}

		[Fact]
		public void Calcular24MesesInvestimentoCdb()
		{
			// Arrange
			var cdb = new Cdb
			{
				Valor = 1000,
				Meses = 24
			};

			// Act
			var result = _SenaProService.Calcular(cdb);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(1261.31m, result.ValorBruto, 4);
			Assert.Equal(1215.58m, result.ValorLiquido, 4);
		}

		[Theory]
		[InlineData(6, 500, 112.5)]
		[InlineData(12, 500, 100)]
		[InlineData(24, 500, 87.5)]
		[InlineData(36, 500, 75)]
		public void CalcularImposto(int meses, decimal rendimento, decimal expectedImposto)
		{
			// Arrange e Act
			var imposto = _SenaProService.CalcularImposto(meses, rendimento);

			// Assert
			Assert.Equal(expectedImposto, imposto);
		}
	}
}