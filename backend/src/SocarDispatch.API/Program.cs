using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SocarDispatch.API.Middlewares;
using SocarDispatch.Application;
using SocarDispatch.Infrastructure;
using Swashbuckle.AspNetCore.Filters;

// Installing .env
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 5));

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 1. JWT Authentication & Authorization
var jwtSecretKey = builder.Configuration["JWT_SECRET_KEY"] 
    ?? builder.Configuration["JwtSettings:SecretKey"] 
    ?? "SOCAR_Super_Secret_Key_For_Emergency_Dispatch_System_2026";

var jwtIssuer = builder.Configuration["JWT_ISSUER"] 
    ?? builder.Configuration["JwtSettings:Issuer"] 
    ?? "socar-dispatch-api";

var jwtAudience = builder.Configuration["JWT_AUDIENCE"] 
    ?? builder.Configuration["JwtSettings:Audience"] 
    ?? "socar-dispatch-clients";

builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger & Bearer Auth Configuration (Filter-based)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "SOCAR Dispatch API", 
        Version = "v1",
        Description = "Real-Time Emergency Dispatch and Response System API"
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header: 'Bearer {token}'",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOCAR Dispatch API v1");
    });
}

app.UseHttpsRedirection();

// Sorting: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();


// Seed Default Teams if database has no teams
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SocarDispatch.Infrastructure.Persistence.ApplicationDbContext>();
    if (!context.Teams.Any())
    {
        context.Teams.AddRange(
            new SocarDispatch.Domain.Entities.Team
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TeamName = "A Blok İSG ve İtfaiye Ekibi",
                Status = "Idle"
            },
            new SocarDispatch.Domain.Entities.Team
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TeamName = "B Blok Kurtarma ve İlk Yardım Ekibi",
                Status = "Idle"
            }
        );
        context.SaveChanges();
    }
}


app.MapControllers();

app.Run();