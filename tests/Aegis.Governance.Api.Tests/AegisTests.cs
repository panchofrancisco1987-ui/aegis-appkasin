using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Aegis.Governance.Core.Compliance;
using Aegis.Governance.Core.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Aegis.Governance.Api.Tests
{
    public class AegisEuAiActReportGeneratorTests
    {
        [Fact]
        public void GenerateArticle12Report_RetornaCumplimientoTrue_CuandoTodasLasFirmasSonValidas()
        {
            var generator = new AegisEuAiActReportGenerator();
            var records = new System.Collections.Generic.List<AuditSignedRecordWithStatus>
            {
                new("evt-001", true, DateTime.UtcNow.AddMinutes(-10)),
                new("evt-002", true, DateTime.UtcNow.AddMinutes(-5))
            };
            var report = generator.GenerateArticle12Report(records);
            Assert.True(report.CompliesWithArticle12);
            Assert.Equal(2, report.TotalEventsAnalyzed);
            Assert.Equal(2, report.VerifiedSignaturesCount);
        }
    }
}
