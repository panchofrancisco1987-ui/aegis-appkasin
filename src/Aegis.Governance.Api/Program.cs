using System.Linq;
using System.Text;
using System.Text.Json;
using Aegis.Governance.Api.Health;
using Aegis.Governance.Api.Middleware;
using Aegis.Governance.Core.Compliance;
using Aegis.Governance.Core.Persistence;
using Aegis.Governance.Core.Security;
using Amazon.KeyManagementService;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Auditor", policy => policy.RequireRole("Auditor", "Admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator", "Auditor", "Admin"));
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule { Endpoint = "*", Limit = 200, Period = "1m" }
    };
});
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Database
builder.Services.AddDbContext<AegisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuditDatabase")));

// Repositories
builder.Services.AddScoped<IAuditEventRepository, AuditEventRepository>();

// AWS KMS
builder.Services.AddAWSService<IAmazonKeyManagementService>();

// Business services
builder.Services.AddScoped<IAegisAuditSigner>(sp =>
    new AegisAuditSigner(
        sp.GetRequiredService<IAmazonKeyManagementService>(),
        builder.Configuration["Aegis:KmsKeyArn"]!,
        sp.GetRequiredService<ILogger<AegisAuditSigner>>(),
        sp.GetService<IAuditEventRepository>()));

builder.Services.AddSingleton<AegisEuAiActReportGenerator>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<AwsKmsHealthCheck>("aws_kms", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready", "crypto" })
    .AddCheck("siem_syslog", new SiemConnectivityHealthCheck(
        builder.Configuration["Aegis:SiemHost"] ?? "127.0.0.1",
        int.Parse(builder.Configuration["Aegis:SiemPort"] ?? "6514"),
        builder.Services.BuildServiceProvider().GetRequiredService<ILogger<SiemConnectivityHealthCheck>>()
    ), failureStatus: HealthStatus.Degraded, tags: new[] { "ready", "siem" });

var app = builder.Build();

static Task WriteJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var json = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            component = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            durationMs = e.Value.Duration.TotalMilliseconds,
            exception = e.Value.Exception?.Message
        })
    }, new JsonSerializerOptions { WriteIndented = true });
    return context.Response.WriteAsync(json);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting();
app.UseMiddleware<AegisPerimeterSecurityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpMetrics();
app.MapMetrics();
app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteJsonResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteJsonResponse
});

app.Run();

public partial class Program { }
