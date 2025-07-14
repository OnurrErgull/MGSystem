using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || user.PasswordHash != dto.Password) // düz karşılaştırma, hash yok şimdilik
        {
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" });
        }

        var token = _jwt.GenerateToken(user);

        return Ok(ApiResponse<string>.Ok(token));
    }
}
