using IdentityService.Data;
using IdentityService.Services; // Yeni servisimiz i�in
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // DbContext i�in
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql; // PostgreSQL i�in

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL i�in DbContext'i ve ba�lant� dizesini ekle
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // UseSqlServer yerine UseNpgsql

// ASP.NET Core Identity'yi yap�land�r
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Kendi servislerimizi DI konteynerine ekle
builder.Services.AddScoped<IAuditService, AuditService>(); // Dapper kullanan AuditService'i ekle

// API Controller'lar� ve di�er servisler
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Ayarlar�n� oku
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

// JWT Kimlik Do�rulamay� Ekle
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();