
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using Microsoft.Extensions.Configuration;
using TaskTracker.Models;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskTracker.Services;

public class AuthService(
    AppDbContext context,
    IConfiguration configuration)
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<User> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
        {
            throw new ArgumentException("Invalid email.");
        }

        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 8)
        {
            throw new ArgumentException(
                "Password must be at least 8 characters.");
        }

        email = email.Trim().ToLowerInvariant();

        var existingUser = await context.Users
            .AnyAsync(user => user.Email == email, cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException(
                "Email is already registered.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = string.Empty
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            password);

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException &&
                  postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
                  postgresException.ConstraintName == "IX_Users_Email")
        {
            throw new InvalidOperationException(
                "Email is already registered.", ex);
        }

        return user;
    }

    public string GenerateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is missing.");

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing.");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing.");

        var keyBytes = Convert.FromBase64String(key);

        var securityKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        var user = await context.Users
            .FirstOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);

        if (user is null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                password);

            await context.SaveChangesAsync(cancellationToken);
        }

        return user;
    }
}