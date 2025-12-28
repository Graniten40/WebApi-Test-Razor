using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Text;

using Encryption.Options;

namespace Encryption.Extensions;

public static class JWTEncryptionsExtensions
{
    public static IServiceCollection AddJwtToken(this IServiceCollection services, IConfiguration configuration)
    {
        // ============================
        // SAFE JWT CONFIGURATION
        // ============================

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Missing config: Jwt:Issuer");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Missing config: Jwt:Audience");

        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Missing config: Jwt:Key");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        // =====================================
        // OLD JWT CONFIGURATION (COMMENTED OUT)
        // =====================================

        /*
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtOptions = configuration
                .GetSection(JwtOptions.Position)
                .Get<JwtOptions>();

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey)),
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidIssuer = jwtOptions.ValidIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidAudience = jwtOptions.ValidAudience,
                RequireExpirationTime = jwtOptions.RequireExpirationTime,
                ValidateLifetime = jwtOptions.RequireExpirationTime,
                ClockSkew = TimeSpan.FromDays(1)
            };
        });
        */

        // Still register JwtOptions for later use (token creation etc.)
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Position));
        services.AddTransient<JwtEncryptions>();

        return services;
    }
}
