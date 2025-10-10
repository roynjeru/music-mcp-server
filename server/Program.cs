using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Server;
using server.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Logging.AddConsole();

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = McpAuthenticationDefaults.AuthenticationScheme;
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SpotifyTools>();

// add telemetry after deployment
// configure OpenTelemetry
// builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddHttpClient("SpotifyApi", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/");
});

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