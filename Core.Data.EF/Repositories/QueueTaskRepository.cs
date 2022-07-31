using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{

    public class QueueTaskRepository : EFRepository<QueueTask, int>, IQueueTaskRepository
    {
        public QueueTaskRepository(AppDbContext context) : base(context)
        {
        }
    }
}
