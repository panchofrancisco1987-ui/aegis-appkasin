using System;
using System.Data.Common;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Aegis.Governance.Api.Health
{
    public class AwsKmsHealthCheck : IHealthCheck
    {
        private readonly IAmazonKeyManagementService _kmsClient;
        private readonly string _keyArn;
        private readonly ILogger<AwsKmsHealthCheck> _logger;

        public AwsKmsHealthCheck(IAmazonKeyManagementService kmsClient, string keyArn, ILogger<AwsKmsHealthCheck> logger)
        {
            _kmsClient = kmsClient ?? throw new ArgumentNullException(nameof(kmsClient));
            _keyArn = keyArn ?? throw new ArgumentNullException(nameof(keyArn));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _kmsClient.DescribeKeyAsync(new DescribeKeyRequest { KeyId = _keyArn }, cancellationToken);
                if (response.KeyMetadata.Enabled)
                    return HealthCheckResult.Healthy("Conexión con AWS KMS establecida y clave inmutable activa.");
                return HealthCheckResult.Degraded($"La clave AWS KMS está deshabilitada o en estado: {response.KeyMetadata.KeyState}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Error al comunicarse con AWS KMS HSM.", ex);
            }
        }
    }

    public class SiemConnectivityHealthCheck : IHealthCheck
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger<SiemConnectivityHealthCheck> _logger;

        public SiemConnectivityHealthCheck(string host, int port, ILogger<SiemConnectivityHealthCheck> logger)
        {
            _host = host;
            _port = port;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var tcpClient = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                await tcpClient.ConnectAsync(_host, _port, cts.Token);
                return HealthCheckResult.Healthy($"Socket TCP/TLS hacia SIEM ({_host}:{_port}) respondiendo correctamente.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"No se pudo establecer conexión TCP con el servidor SIEM ({_host}:{_port}).", ex);
            }
        }
    }

    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(DbConnection dbConnection, ILogger<DatabaseHealthCheck> logger)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool wasClosed = false;
            try
            {
                if (_dbConnection.State != System.Data.ConnectionState.Open)
                {
                    await _dbConnection.OpenAsync(cancellationToken);
                    wasClosed = true;
                }
                using var command = _dbConnection.CreateCommand();
                command.CommandText = "SELECT 1;";
                await command.ExecuteScalarAsync(cancellationToken);
                return HealthCheckResult.Healthy("Conexión y ejecución de prueba en Base de Datos exitosas.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Fallo crítico en la conexión con la base de datos de auditoría.", ex);
            }
            finally
            {
                if (wasClosed && _dbConnection.State == System.Data.ConnectionState.Open)
                    await _dbConnection.CloseAsync();
            }
        }
    }
}
