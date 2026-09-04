using System;

namespace Aegis.Governance.Core.Entities
{
    public class AuditEventEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string? CorrelationId { get; set; }
        public string? Payload { get; set; }
        public string? Signature { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}
