using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Usuario.API.Models;
using Usuario.API.Services;

namespace Usuario.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
	private readonly IConfiguration _configuration;
	private readonly IFixedUserCatalog _users;

	public AuthController(IConfiguration configuration, IFixedUserCatalog users) {
		_configuration = configuration;
		_users = users;
	}

	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request) {
		if (string.IsNullOrWhiteSpace(request.Email) || request.Password is null) {
			return BadRequest(new { error = "Email y contraseña son obligatorios." });
		}

		var user = _users.FindByCredentials(request.Email.Trim(), request.Password);
		if (user is null) {
			return Unauthorized(new { error = "Credenciales inválidas" });
		}

		var secretKey =
			Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
			?? _configuration["Bearer:SecretKey"]
			?? throw new InvalidOperationException(
				"La clave secreta JWT no está configurada. Define JWT_SECRET_KEY o Bearer:SecretKey.");

		var tokenHandler = new JwtSecurityTokenHandler();
		var byteKey = Encoding.UTF8.GetBytes(secretKey);

		var claims = new List<Claim> {
			new(ClaimTypes.NameIdentifier, user.Email),
			new(ClaimTypes.Name, user.Email),
			new(ClaimTypes.GivenName, user.Name),
			new(ClaimTypes.Surname, user.Lastname),
			new(ClaimTypes.Role, user.IsHost?"host":"guest"),
			new("address", user.Address)

		};

		var expiresAt = DateTime.UtcNow.AddDays(3);
		var tokenDescriptor = new SecurityTokenDescriptor {
			Subject = new ClaimsIdentity(claims),
			Expires = expiresAt,
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(byteKey),
				SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);
		var expiresInSeconds = (int)Math.Max(1, (expiresAt - DateTime.UtcNow).TotalSeconds);

		return Ok(new LoginResponse(
			tokenString,
			expiresInSeconds,
			"Bearer",
			user.ToPublic()));
	}
}

public sealed class LoginRequest {
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponse(
	string Token,
	int ExpiresIn,
	string TokenType,
	FixedUserPublic User);
