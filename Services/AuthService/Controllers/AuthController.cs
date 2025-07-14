using AuthService.Data;
using AuthService.Services;
using MGSystem.Shared.DTOs;
using MGSystem.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly PasswordHasherService _hasher;

    public AuthController(AppDbContext db, JwtService jwt, PasswordHasherService hasher)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
        if (user is null || !_hasher.VerifyPassword(user.PasswordHash, dto.Password))
        {
            return Unauthorized(ApiResponse<string>.Fail("Geçersiz kullanıcı adı veya şifre."));
        }


        var token = _jwt.GenerateToken(user);
        return Ok(ApiResponse<object>.Ok(new
        {
            token,
            user = new { user.Id, user.Username, user.Role }
        }));
    }
}
