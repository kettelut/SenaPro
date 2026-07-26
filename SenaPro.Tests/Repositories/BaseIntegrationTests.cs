using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SenaPro.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace SenaPro.Tests.Repositories;

public abstract class BaseIntegrationTests : IAsyncLifetime
{
    protected static readonly bool DockerAvailable = IsDockerSocketReachable();

    public PostgreSqlContainer? Container { get; private set; }
    public AppDbContext? Context { get; private set; }

    public async Task InitializeAsync()
    {
        if (!DockerAvailable) return;
        await CreateIntegrationContextAsync();
    }

    protected async Task CreateIntegrationContextAsync()
    {
        Container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await Container.StartAsync();

        Context = CreateContext(Container.GetConnectionString());
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    private static bool IsDockerSocketReachable()
    {
        try
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo("docker", "info --format=")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            var output = proc.StandardOutput.ReadLine();
            proc.WaitForExit(8000);

            return !proc.HasExited && (output == string.Empty || output is null);
        }
        catch
        {
            return false;
        }
    }

    protected static AppDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options);
}
