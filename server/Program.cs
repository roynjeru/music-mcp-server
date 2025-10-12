using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Server;
using server.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Logging.AddConsole();
var authority = builder.Configuration["jwt-authority"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = authority;
    options.TokenValidationParameters = new()
    {
        ValidAudience = "my-mcp-server",
        ValidateAudience = true,
        ValidateIssuer = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SpotifyTools>();

builder.Services.AddHttpClient("SpotifyApi", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/");
});

// configure OpenTelemetry
builder.Services.AddOpenTelemetry().UseAzureMonitor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// only redirect to HTTPS in non-development environments so local HTTP POSTs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapMcp();

app.Run();