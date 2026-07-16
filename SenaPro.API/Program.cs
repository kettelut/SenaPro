using Microsoft.EntityFrameworkCore;
using SenaPro.Infrastructure.Data;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Repositories;
using SenaPro.Application.Services;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories and Services
builder.Services.AddScoped<ISorteioRepository, SorteioRepository>();
builder.Services.AddScoped<IAnaliseEstatisticaService, AnaliseEstatisticaService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddScoped<IGeradorJogosService, GeradorJogosService>();

// Configure typed HttpClient for ApiLoteriaService
builder.Services.AddHttpClient<IApiLoteriaService, ApiLoteriaService>();

// Configure Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.UseHangfireDashboard(); // Exposes Hangfire Dashboard at /hangfire

app.MapControllers();

// Configure Hangfire recurring job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IApiLoteriaService>(
        "verificar-atualizacoes-megasena",
        service => service.AtualizarAsync(CancellationToken.None),
        Cron.Hourly);
}

app.Run();