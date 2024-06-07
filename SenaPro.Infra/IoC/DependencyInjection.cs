using SenaPro.Application.Interfaces;
using SenaPro.Application.Services;
using SenaPro.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SenaPro.Domain.Services.Interfaces;
using SenaPro.Domain.Repositories;
using SenaPro.Infra.Repositories;

namespace SenaPro.Infra
{
    public static class DependencyInjection
	{
		public static void AddApplicationDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
		{
			AddApplicationDependencies(serviceCollection);
		}

		public static void AddApplicationDependencies(this IServiceCollection serviceCollection)
		{
			// Application
			serviceCollection.AddScoped<ISenaProAppService, SenaProAppService>();

			// Domain
			serviceCollection.AddScoped<ISenaProService, SenaProService>();

            // Infra
            serviceCollection.AddScoped<IMegaSena, MegaSena>();
        }
	}
}
