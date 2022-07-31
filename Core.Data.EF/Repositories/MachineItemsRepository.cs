using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{
    public class MachineItemsRepository : EFRepository<MachineItems, int>, IMachineItemsRepository
    {
        public MachineItemsRepository(AppDbContext context) : base(context)
        {
        }
    }
}
