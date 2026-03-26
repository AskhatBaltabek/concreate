using ContentGenerator.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContentGenerator.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest req, UserManager<User> userManager) =>
        {
            var user = new User { UserName = req.Email, Email = req.Email, FirstName = req.FirstName, LastName = req.LastName };
            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);
            return Results.Ok(new { Message = "User registered successfully." });
        });

        group.MapPost("/login", async (LoginRequest req, UserManager<User> userManager, IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "SuperSecretKeyForContentGeneratorApi12345!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                    new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = config["Jwt:Issuer"] ?? "ContentGeneratorApi",
                Audience = config["Jwt:Audience"] ?? "ContentGeneratorApp",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
        });
    }
}

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password);
