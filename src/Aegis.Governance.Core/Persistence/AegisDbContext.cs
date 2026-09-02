using Microsoft.EntityFrameworkCore;
using Aegis.Governance.Core.Entities;

namespace Aegis.Governance.Core.Persistence
{
    public class AegisDbContext : DbContext
    {
        public DbSet<AuditEventEntity> AuditEvents => Set<AuditEventEntity>();

        public AegisDbContext(DbContextOptions<AegisDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuditEventEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.Signature).IsRequired();
                entity.Property(e => e.KeyArn).IsRequired().HasMaxLength(256);
                entity.Property(e => e.TimestampUtc).IsRequired();
            });
        }
    }
}
