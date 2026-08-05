using Microsoft.EntityFrameworkCore;
using SenaPro.Application.Services;
using SenaPro.Domain.Interfaces;
using SenaPro.Infrastructure.Data;
using SenaPro.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── MVC Application Parts ─────────────────────────────────────────────
// Registra explicitamente os assemblies que contêm controllers como
// Application Parts. Isso garante que o routing engine descubra e casem
// as rotas em runtime, não apenas o Swashbuckle via reflexão.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(SenaPro.API.Controllers.SorteiosController).Assembly)
    .AddApplicationPart(typeof(SenaPro.API.Controllers.GeradorController).Assembly);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
