using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services; // IAuditService için
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditService _auditService; // Yeni enjekte edilen servis

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditService auditService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _auditService = auditService; // Enjekte edilen servisi ata
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginModel model)
        {
            var user = new ApplicationUser { UserName = model.Username, Email = model.Username };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User"); // Varsayılan rol atama
                await _auditService.LogAsync(new AuditLog { EventType = "UserRegistered", Username = model.Username, Details = $"New user {model.Username} registered." });
                return Ok(new TokenResponse { Message = "Registration successful" });
            }

            await _auditService.LogAsync(new AuditLog { EventType = "RegistrationFailed", Username = model.Username, Details = $"Registration failed for {model.Username}: {string.Join(", ", result.Errors.Select(e => e.Description))}" });
            return BadRequest(new TokenResponse { Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                await _auditService.LogAsync(new AuditLog { EventType = "LoginFailed", Username = model.Username, Details = $"User {model.Username} not found." });
                return Unauthorized(new TokenResponse { Message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var encodedToken = tokenHandler.WriteToken(token);

                await _auditService.LogAsync(new AuditLog { EventType = "LoginSuccessful", Username = model.Username, Details = $"User {model.Username} logged in successfully." });
                return Ok(new TokenResponse
                {
                    Token = encodedToken,
                    Expiration = tokenDescriptor.Expires.Value,
                    Message = "Login successful"
                });
            }

            await _auditService.LogAsync(new AuditLog { EventType = "LoginFailed", Username = model.Username, Details = $"Login failed for {model.Username}." });
            return Unauthorized(new TokenResponse { Message = "Invalid credentials" });
        }

        [HttpGet("test-auth")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult TestAuth()
        {
            var username = User.Identity.Name;
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok($"Hello {username}! You are authorized. Your roles: {string.Join(", ", roles)}");
        }
    }
}