using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aegis.Governance.Core.Compliance;
using Aegis.Governance.Core.Persistence;
using Aegis.Governance.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aegis.Governance.Api.Controllers
{
    [ApiController]
    [Route("api/v1/governance/audit")]
    [Authorize]
    public class AegisAuditController : ControllerBase
    {
        private readonly IAegisAuditSigner _auditSigner;
        private readonly AegisEuAiActReportGenerator _reportGenerator;
        private readonly IAuditEventRepository _auditRepository;
        private readonly ILogger<AegisAuditController> _logger;

        public AegisAuditController(IAegisAuditSigner auditSigner, AegisEuAiActReportGenerator reportGenerator, IAuditEventRepository auditRepository, ILogger<AegisAuditController> logger)
        {
            _auditSigner = auditSigner ?? throw new ArgumentNullException(nameof(auditSigner));
            _reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
            _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("sign")]
        [ProducesResponseType(typeof(AuditSignedRecord), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignAuditEvent([FromBody] AuditPayloadRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EventPayload))
                return BadRequest(new { error = "El payload del evento no puede estar vacío." });

            try
            {
                var signedRecord = await _auditSigner.SignEventAsync(request.EventPayload, cancellationToken);
                var auditEntity = new AuditEventEntity
                {
                    Id = Guid.NewGuid(),
                    Payload = signedRecord.Payload,
                    Signature = signedRecord.SignatureSha256Base64,
                    KeyArn = signedRecord.KeyArn,
                    TimestampUtc = signedRecord.TimestampUtc
                };
                await _auditRepository.AddAsync(auditEntity, cancellationToken);
                _logger.LogInformation("Evento firmado y persistido. Id: {EventId}", auditEntity.Id);
                return Ok(signedRecord);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Solicitud de firma inválida.");
                return BadRequest(new { error = ex.Message });
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de firma.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Fallo crítico interno." });
            }
        }

        [HttpPost("verify")]
        [ProducesResponseType(typeof(VerificationResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyAuditEvent([FromBody] AuditVerifyRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Payload) || string.IsNullOrWhiteSpace(request.SignatureBase64))
                return BadRequest(new { error = "Se requiere payload y firma Base64." });

            bool isValid = await _auditSigner.VerifySignatureAsync(request.Payload, request.SignatureBase64, cancellationToken);
            return Ok(new VerificationResultResponse(isValid, DateTime.UtcNow));
        }

        [HttpPost("reports/eu-ai-act/article-12")]
        [ProducesResponseType(typeof(ComplianceReportSummary), StatusCodes.Status200OK)]
        public IActionResult GenerateArticle12Report([FromBody] IEnumerable<AuditSignedRecordWithStatus> records)
        {
            var summary = _reportGenerator.GenerateArticle12Report(records);
            return Ok(summary);
        }
    }

    public record AuditPayloadRequest(string EventPayload);
    public record AuditVerifyRequest(string Payload, string SignatureBase64);
    public record VerificationResultResponse(bool IsValid, DateTime VerifiedAtUtc);
}
