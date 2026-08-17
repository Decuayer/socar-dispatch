using dotenv.net;
using SocarDispatch.API.Middlewares;
using SocarDispatch.Application;
using SocarDispatch.Infrastructure;

// Read the .env file in the parent directory and load it into Environment
DotEnv.Load(options: new DotEnvOptions(
    probeForEnv: true, 
    probeLevelsToSearch: 5));

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration provider
builder.Configuration.AddEnvironmentVariables();

// Infrastructure and DbContext registration
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();