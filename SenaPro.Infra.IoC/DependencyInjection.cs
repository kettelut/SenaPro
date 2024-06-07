using SenaPro.Aplicacao.Interfaces;
using SenaPro.Aplicacao.Servicos;
using SenaPro.Dominio.Interfaces;
using SenaPro.Dominio.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SenaPro.Infra.IoC
{
	public static class DependencyInjection
	{
		public static void AddApplicationDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
		{
			AddApplicationDependencies(serviceCollection);
		}

		public static void AddApplicationDependencies(this IServiceCollection serviceCollection)
		{
			// Aplicação
			serviceCollection.AddScoped<ISenaProAppService, SenaProAppService>();

			// Domínio
			serviceCollection.AddScoped<ISenaProService, SenaProService>();
		}
	}
}
