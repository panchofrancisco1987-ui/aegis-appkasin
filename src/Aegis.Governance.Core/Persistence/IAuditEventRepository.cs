using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aegis.Governance.Core.Entities;

namespace Aegis.Governance.Core.Persistence
{
    public interface IAuditEventRepository
    {
        Task AddAsync(AuditEventEntity auditEvent, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditEventEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
