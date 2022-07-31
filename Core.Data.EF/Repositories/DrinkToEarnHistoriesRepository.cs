using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using System.Linq;

namespace Core.Data.EF.Repositories
{

    public class DrinkToEarnHistoriesRepository : EFRepository<DrinkToEarnHistories, int>, IDrinkToEarnHistoriesRepository
    {
        public DrinkToEarnHistoriesRepository(AppDbContext context) : base(context)
        {
        }

        public DrinkToEarnHistories GetLatestActiveDrink(int cupItemHistoryId)
        {
            return FindAll(x=>x.StatusId == Enums.DrinkHistoryStatus.Active && x.CupItemHistoryId == cupItemHistoryId).SingleOrDefault();
        }

        public bool IsOnDrink(int cupItemHistoryId)
        {

            var isOnDrink = FindAll(x => x.CupItemHistoryId == cupItemHistoryId
                    && x.StatusId == DrinkHistoryStatus.Active).Any();

            return isOnDrink;
        }
    }
}
