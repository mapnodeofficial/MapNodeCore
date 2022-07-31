using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Linq;

namespace Core.Data.EF.Repositories
{


    public class AppUsersCupItemHistoriesRepository : EFRepository<AppUsersCupItemHistories, int>, IAppUsersCupItemHistoriesRepository
    {
        public AppUsersCupItemHistoriesRepository(AppDbContext context) : base(context)
        {
        }

        public AppUsersCupItemHistories GetByUser(int id, Guid appUserId)
        {
            return FindAll(x=>x.Id==id && x.AppUserId == appUserId).SingleOrDefault();
        }
    }
}
