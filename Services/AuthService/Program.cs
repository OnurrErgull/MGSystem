using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// EF Core - PostgreSQL baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreCon")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
