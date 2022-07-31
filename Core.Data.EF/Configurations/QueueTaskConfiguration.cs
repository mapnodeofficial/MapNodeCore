using Core.Data.EF.Extensions;
using Core.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Data.EF.Configurations
{
    public class QueueTaskConfiguration : DbEntityConfiguration<QueueTask>
    {
        public override void Configure(EntityTypeBuilder<QueueTask> entity)
        {
            entity.HasKey(c => c.Id);
            // etc.
        }
    }
}
