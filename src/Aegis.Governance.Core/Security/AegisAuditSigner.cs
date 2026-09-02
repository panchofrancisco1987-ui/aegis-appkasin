using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Microsoft.Extensions.Logging;

namespace Aegis.Governance.Core.Security
{
    public interface IAegisAuditSigner
    {
        Task<AuditSignedRecord> SignEventAsync(string eventPayload, CancellationToken cancellationToken = default);
        Task<bool> VerifySignatureAsync(string eventPayload, string base64Signature, CancellationToken cancellationToken = default);
    }

    public record AuditSignedRecord(string Payload, string SignatureSha256Base64, string KeyArn, DateTime TimestampUtc);

    public class AegisAuditSigner : IAegisAuditSigner
    {
        private readonly IAmazonKeyManagementService _kmsClient;
        private readonly string _keyArn;
        private readonly ILogger<AegisAuditSigner> _logger;

        public AegisAuditSigner(IAmazonKeyManagementService kmsClient, string keyArn, ILogger<AegisAuditSigner> logger)
        {
            _kmsClient = kmsClient ?? throw new ArgumentNullException(nameof(kmsClient));
            _keyArn = keyArn ?? throw new ArgumentNullException(nameof(keyArn));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuditSignedRecord> SignEventAsync(string eventPayload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventPayload))
                throw new ArgumentException("El payload del evento de auditoría no puede estar vacío.", nameof(eventPayload));

            byte[] payloadBytes = Encoding.UTF8.GetBytes(eventPayload);
            byte[] payloadHash = SHA256.HashData(payloadBytes);

            var signRequest = new SignRequest
            {
                KeyId = _keyArn,
                Message = new MemoryStream(payloadHash),
                MessageType = MessageType.DIGEST,
                SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256
            };

            try
            {
                using (signRequest.Message)
                {
                    var signResponse = await _kmsClient.SignAsync(signRequest, cancellationToken);
                    using var signatureStream = signResponse.Signature;
                    byte[] signatureBytes = new byte[signatureStream.Length];
                    await signatureStream.ReadExactlyAsync(signatureBytes, cancellationToken);
                    string signatureBase64 = Convert.ToBase64String(signatureBytes);

                    _logger.LogInformation("Evento de auditoría firmado exitosamente usando AWS KMS Key ARN: {KeyArn}", _keyArn);

                    return new AuditSignedRecord(
                        Payload: eventPayload,
                        SignatureSha256Base64: signatureBase64,
                        KeyArn: _keyArn,
                        TimestampUtc: DateTime.UtcNow
                    );
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al firmar el evento de auditoría en AWS KMS HSM.");
                throw;
            }
        }

        public async Task<bool> VerifySignatureAsync(string eventPayload, string base64Signature, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventPayload))
                throw new ArgumentException("El payload no puede estar vacío.", nameof(eventPayload));
            if (string.IsNullOrWhiteSpace(base64Signature))
                throw new ArgumentException("La firma no puede estar vacía.", nameof(base64Signature));

            byte[] payloadBytes = Encoding.UTF8.GetBytes(eventPayload);
            byte[] payloadHash = SHA256.HashData(payloadBytes);
            byte[] signatureBytes = Convert.FromBase64String(base64Signature);

            using var messageStream = new MemoryStream(payloadHash);
            using var signatureStream = new MemoryStream(signatureBytes);

            var verifyRequest = new VerifyRequest
            {
                KeyId = _keyArn,
                Message = messageStream,
                MessageType = MessageType.DIGEST,
                Signature = signatureStream,
                SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256
            };

            var response = await _kmsClient.VerifyAsync(verifyRequest, cancellationToken);
            return response.SignatureValid;
        }
    }
}
