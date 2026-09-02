using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aegis.Governance.Core.Entities;

namespace Aegis.Governance.Core.Persistence
{
    public class AuditEventRepository : IAuditEventRepository
    {
        private readonly AegisDbContext _context;

        public AuditEventRepository(AegisDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(AuditEventEntity auditEvent, CancellationToken cancellationToken = default)
        {
            await _context.AuditEvents.AddAsync(auditEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditEventEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AuditEvents.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
