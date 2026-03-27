using ContentGenerator.Domain.Entities;
using ContentGenerator.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ContentGenerator.Application.Interfaces;
using ContentGenerator.Infrastructure.Services;
using ContentGenerator.Infrastructure.Services.Mocks;
using ContentGenerator.Infrastructure.Consumers;
using MassTransit;
using System.Text;

namespace ContentGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ClaudeScriptGeneratorService>();
        services.AddHttpClient<GeminiScriptGeneratorService>();
        services.AddHttpClient<GeminiVoiceGeneratorService>();
        services.AddHttpClient<GenAiProVoiceGeneratorService>();
        services.AddHttpClient<ElevenLabsVoiceGeneratorService>();

        services.AddTransient<IScriptGeneratorService, ClaudeScriptGeneratorService>();
        services.AddTransient<IScriptGeneratorService, GeminiScriptGeneratorService>();
        services.AddTransient<IScriptGeneratorService, MockScriptGeneratorService>();

        services.AddTransient<IVoiceGeneratorService, GenAiProVoiceGeneratorService>();
        services.AddTransient<IVoiceGeneratorService, ElevenLabsVoiceGeneratorService>();
        services.AddTransient<IVoiceGeneratorService, GeminiVoiceGeneratorService>();

        services.AddTransient<IMediaStorageService, LocalMediaStorageService>();
        services.AddTransient<IVideoGeneratorStrategy, MockLumaVideoGeneratorService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<GenerateAudioConsumer>();
            x.AddConsumer<GenerateVideoConsumer>();

            var rabbitMqUri = configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost";
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqUri), h => {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "ContentGeneratorApi",
                ValidAudience = configuration["Jwt:Audience"] ?? "ContentGeneratorApp",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "SuperSecretKeyForContentGeneratorApi12345!"))
            };
        });

        return services;
    }
}
