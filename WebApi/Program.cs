using Infrastructure;
using Application;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using WebApi.Middleware;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAddApplication();
builder.Services.ConfigureAddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotificationPublisher, WebApi.Services.SignalRNotificationPublisher>();
builder.Services.AddScoped<IFileStorage, WebApi.Services.LocalFileStorage>();
builder.Services.Configure<WebApi.Services.SeedAdminSettings>(builder.Configuration.GetSection("SeedAdmin"));
builder.Services.AddHostedService<WebApi.Services.SeedAdminHostedService>();
builder.Services.Configure<WebApi.Services.DeadlineReminderOptions>(
    builder.Configuration.GetSection(WebApi.Services.DeadlineReminderOptions.SectionName));
builder.Services.AddHostedService<WebApi.Services.DeadlineReminderHostedService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KomSync API",
        Version = "v1"
    });

    const string bearerScheme = "Bearer";

    opt.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    opt.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(bearerScheme, document)] = new List<string>()
    });
});

builder.Services.AddCors(options =>
{
    var configuredOrigins = GetCorsOrigins(builder.Configuration);
    options.AddPolicy("AllowFrontendDev", policy =>
    {
        policy
            .WithOrigins(configuredOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? "Super_Secret_Key_At_Least_32_Chars_Long";
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/hubs"))
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken))
                        context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KomSync API V1");
        c.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KomSyncDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");

    try
    {
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex,
            "Failed to apply database migrations");
    }
    scope.Dispose();
}

app.UseCors("AllowFrontendDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WebApi.Hubs.NotificationHub>("/hubs/notifications")
    .RequireCors("AllowFrontendDev");

app.Run();

static string[] GetCorsOrigins(IConfiguration configuration)
{
    var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    var fromArray = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    foreach (var origin in fromArray)
        AddOrigin(result, origin);

    var csv = configuration["Cors:AllowedOriginsCsv"];
    if (!string.IsNullOrWhiteSpace(csv))
    {
        foreach (var origin in csv.Split(',', ';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            AddOrigin(result, origin);
    }

    var includeBackendOrigin = configuration.GetValue<bool?>("Cors:IncludeBackendOrigin") ?? true;
    if (includeBackendOrigin)
    {
        var urls = configuration["ASPNETCORE_URLS"];
        foreach (var origin in GetOriginsFromAspNetCoreUrls(urls))
            AddOrigin(result, origin);
    }

    if (result.Count == 0)
        throw new InvalidOperationException(
            "No CORS origins configured. Set Cors:AllowedOrigins or Cors:AllowedOriginsCsv.");

    return result.ToArray();
}

static void AddOrigin(ISet<string> set, string? origin)
{
    if (string.IsNullOrWhiteSpace(origin))
        return;

    if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        set.Add(uri.GetLeftPart(UriPartial.Authority));
    }
}

static IEnumerable<string> GetOriginsFromAspNetCoreUrls(string? urls)
{
    if (string.IsNullOrWhiteSpace(urls))
        yield break;

    foreach (var item in urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (Uri.TryCreate(item, UriKind.Absolute, out var uri))
        {
            yield return uri.GetLeftPart(UriPartial.Authority);
            continue;
        }

        var wildcard = Regex.Match(item, @"^(?<scheme>https?)://[\+\*]:(?<port>\d+)$", RegexOptions.IgnoreCase);
        if (wildcard.Success)
        {
            var scheme = wildcard.Groups["scheme"].Value.ToLowerInvariant();
            var port = wildcard.Groups["port"].Value;
            yield return $"{scheme}://localhost:{port}";
            yield return $"{scheme}://127.0.0.1:{port}";
        }
    }
}