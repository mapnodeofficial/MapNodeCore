using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{
    public class ShopItemsRepository : EFRepository<ShopItem, int>, IShopItemsRepository
    {
        public ShopItemsRepository(AppDbContext context) : base(context)
        {
        }
    }
}
