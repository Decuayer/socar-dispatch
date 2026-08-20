using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Infrastructure.Persistence;
using SocarDispatch.Infrastructure.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace SocarDispatch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration["DB_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Firebase Admin Initialization & Push Notification Service Registration
        var credPath = configuration["Firebase:CredentialsPath"] ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");
        if (!string.IsNullOrEmpty(credPath) && File.Exists(credPath))
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credPath)
                });
            }
        }
        services.AddSingleton<IPushNotificationService, FirebasePushNotificationService>();

        // Register MediatR notification handlers from Infrastructure assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}