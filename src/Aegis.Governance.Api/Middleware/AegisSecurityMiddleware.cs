using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aegis.Governance.Api.Middleware
{
    public class AegisPerimeterSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AegisPerimeterSecurityMiddleware> _logger;
        private const string ApiKeyHeaderName = "X-Aegis-Api-Key";

        public AegisPerimeterSecurityMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AegisPerimeterSecurityMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path;
            if (path.StartsWithSegments("/health") || path.StartsWithSegments("/metrics") || path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            bool hasApiKey = context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey);
            bool hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");

            if (!hasApiKey && !hasAuthHeader)
            {
                _logger.LogWarning("Intento de acceso sin autenticación desde {RemoteIp}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Acceso denegado: Se requiere autenticación.");
                return;
            }

            if (hasApiKey)
            {
                string configuredApiKey = _configuration["Aegis:ApiKey"] ?? string.Empty;
                var expectedBytes = Encoding.UTF8.GetBytes(configuredApiKey);
                var providedBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());
                bool isValid = expectedBytes.Length == providedBytes.Length &&
                    CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);

                if (!isValid)
                {
                    _logger.LogWarning("Intento de acceso con API Key inválida desde {RemoteIp}", context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Acceso denegado: API Key inválida.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
