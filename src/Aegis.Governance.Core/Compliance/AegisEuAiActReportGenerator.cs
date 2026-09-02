using System;
using System.Collections.Generic;
using System.Linq;

namespace Aegis.Governance.Core.Compliance
{
    public record ComplianceReportSummary(
        string Standard,
        DateTime GeneratedAtUtc,
        int TotalEventsAnalyzed,
        int VerifiedSignaturesCount,
        bool CompliesWithArticle12,
        string RiskLevel
    );

    public class AegisEuAiActReportGenerator
    {
        public ComplianceReportSummary GenerateArticle12Report(IEnumerable<AuditSignedRecordWithStatus> records)
        {
            var list = records?.ToList() ?? new List<AuditSignedRecordWithStatus>();
            int total = list.Count;
            int validSignatures = list.Count(r => r.IsSignatureValid);

            bool article12Complies = total > 0 && total == validSignatures;

            return new ComplianceReportSummary(
                Standard: "EU AI Act - Article 12 (Record-Keeping & Traceability)",
                GeneratedAtUtc: DateTime.UtcNow,
                TotalEventsAnalyzed: total,
                VerifiedSignaturesCount: validSignatures,
                CompliesWithArticle12: article12Complies,
                RiskLevel: "High-Risk AI System Verification"
            );
        }
    }

    public record AuditSignedRecordWithStatus(string EventId, bool IsSignatureValid, DateTime Timestamp);
}
