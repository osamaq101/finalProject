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

    public string GenerateToken(string email, string name,string profileImageUrl)
    {
        // Define a strong secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourVeryStrongKey123!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define claims to include in the token payload
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim("email", email),
        new Claim("name", name),
        new Claim("profileImageUrl", profileImageUrl),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Create the token
        var token = new JwtSecurityToken(
            issuer: "Osama",
            audience: "People",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        // Return the serialized token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
