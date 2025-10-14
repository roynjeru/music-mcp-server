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
var serverUrl = builder.Configuration["serverUrl"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = authority;
    options.TokenValidationParameters = new()
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = "my-mcp-server",
        ValidIssuer = authority
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Additional custom validation can be done here if needed
            Console.WriteLine("Token validated successfully.");
            Console.WriteLine($"Context: {context.SecurityToken}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Handle the challenge response if needed
            Console.WriteLine("OnChallenge error: " + context.Error);
            return Task.CompletedTask;
        }
    };
})
.AddMcp(options =>
{
    options.ResourceMetadata = new()
    {
        Resource = new Uri(serverUrl),
        AuthorizationServers = { new Uri(authority) },
        ScopesSupported = { "openid", "profile", "email", "mcp:tools" }
    };
});


builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi();
builder.Services.AddAntiforgery();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SpotifyTools>();

builder.Services.AddHttpClient("SpotifyApi", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/");
});

builder.Services.AddHttpClient("OAuthServer", client =>
{
    client.BaseAddress = new Uri(authority);
});

// configure OpenTelemetry
builder.Services.AddOpenTelemetry().UseAzureMonitor();

var app = builder.Build();

app.UseRouting();

// 1) Authenticate first
app.UseAuthentication();

// 2) Then authorize
app.UseAuthorization();

app.UseAntiforgery();
app.UseEndpoints(_ =>{});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.MapControllers();

app.MapMcp().RequireAuthorization();

app.Run();