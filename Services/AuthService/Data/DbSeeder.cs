using AuthService.Models;
using AuthService.Services;

namespace AuthService.Data;

public static class DbSeeder
{
    public static async Task SeedUsersAsync(AppDbContext db, PasswordHasherService hasher)
    {
        if (db.Users.Any())
            return; // Zaten kullanıcı varsa ekleme

        var users = new List<AppUser>
        {
            new() { Username = "admin", PasswordHash = hasher.HashPassword("1234"), Role = "Admin" },
            new() { Username = "onur", PasswordHash = hasher.HashPassword("1234"), Role = "User" },
            new() { Username = "mehmet", PasswordHash = hasher.HashPassword("1234"), Role = "User" },
            new() { Username = "ayse", PasswordHash = hasher.HashPassword("1234"), Role = "Editor" },
            new() { Username = "ali", PasswordHash = hasher.HashPassword("1234"), Role = "Viewer" }
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }
}
