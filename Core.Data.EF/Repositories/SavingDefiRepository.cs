using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Core.Data.EF.Repositories
{
    public class SavingDefiRepository : EFRepository<SavingDefi, Guid>, ISavingDefiRepository
    {
        public SavingDefiRepository(AppDbContext context) : base(context)
        {
        }
    }
}
