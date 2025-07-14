using AuthService.Data;
using AuthService.Services;
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
builder.Services.AddSingleton<PasswordHasherService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddControllers();

// EF Core - PostgreSQL baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreCon")));

builder.Services.AddControllers();

var app = builder.Build();

//kullanýcý seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasherService>();
    await DbSeeder.SeedUsersAsync(db, hasher);
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
