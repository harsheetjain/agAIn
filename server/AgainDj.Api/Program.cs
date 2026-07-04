using System.Text.Json.Serialization;
using AgainDj.Api.Contracts;
using AgainDj.Api.Hosting;
using AgainDj.Api.Hubs;
using AgainDj.Api.Realtime;
using AgainDj.Application.Sessions;
using AgainDj.Domain.Abstractions;
using AgainDj.Infrastructure.Audio;
using AgainDj.Infrastructure.Learning;
using AgainDj.Infrastructure.Library;
using AgainDj.Infrastructure.Policies;
using AgainDj.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

// Deterministic dev port so the Vite proxy / dev tunnel can find the API.
builder.WebHost.UseUrls("http://localhost:5215");

const string CorsPolicy = "console";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
    .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services
    .AddControllers()
    .AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions));

builder.Services
    .AddSignalR()
    .AddJsonProtocol(options => ConfigureJson(options.PayloadSerializerOptions));

// --- Composition root (clean architecture: interfaces -> implementations) ---
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ITrackLibrary, InMemoryTrackLibrary>();
builder.Services.AddSingleton<IStyleStore>(_ =>
    new JsonStyleStore(Path.Combine(builder.Environment.ContentRootPath, "data", "style.json")));
builder.Services.AddSingleton<IStyleTrainer, OnlineStyleTrainer>();
builder.Services.AddSingleton<IAudioAnalyzer, RunningAudioAnalyzer>();
builder.Services.AddSingleton<IMixingPolicy, RuleMixingPolicy>();
builder.Services.AddSingleton<ISessionCoordinator>(_ => new SessionCoordinator(TimeSpan.FromSeconds(8)));
builder.Services.AddSingleton<IConsoleGateway, SignalRConsoleGateway>();
builder.Services.AddSingleton<MixSession>();
builder.Services.AddHostedService<AutonomousLoopService>();

var app = builder.Build();

app.UseCors(CorsPolicy);
app.MapControllers();
app.MapHub<DjHub>("/hub/dj");

app.Run();

static void ConfigureJson(System.Text.Json.JsonSerializerOptions options)
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new CamelotKeyJsonConverter());
}