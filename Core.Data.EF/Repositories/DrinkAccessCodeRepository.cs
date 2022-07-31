using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{


    public class DrinkAccessCodeRepository : EFRepository<DrinkAccessCode, int>, IDrinkAccessCodeRepository
    {
        public DrinkAccessCodeRepository(AppDbContext context) : base(context)
        {
        }
    }
}
