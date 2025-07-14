using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public class PasswordHasherService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string hashedPassword, string plainPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, plainPassword);
        return result == PasswordVerificationResult.Success;
    }
}
