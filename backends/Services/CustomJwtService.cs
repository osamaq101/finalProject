using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


public class CustomJwtService
{
    private readonly IConfiguration _config;

    public CustomJwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string email)
    {
        string Jwt;

        Jwt = "YourVeryStrongKey123!";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {

                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var token = new JwtSecurityToken("Osama", "People", claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
