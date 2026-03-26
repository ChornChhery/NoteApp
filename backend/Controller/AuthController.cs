using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class AuthController : ControllerBase
{
    private readonly UserService _users;
    private readonly IConfiguration _config; // reading appsetting

    public AuthController(UserService users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _users.EmailExistsAsync(dto.Email))
        {
            return BadRequest(new { message = "Email already in use" });
        }

        var user = await _users.CreateAsync(dto);

        return Ok(new
        {
            token = BuildToken(user)
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(new
        {
            token = BuildToken(user)
        });
    }

    // Helper method to build JWT tokent that contains the user's id, name and email
    private string BuildToken(User user)
    {
        // sign the token with the secret key from appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // claims are the data stored inside the token
        // the backend reads these on every request to know who is calling
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}