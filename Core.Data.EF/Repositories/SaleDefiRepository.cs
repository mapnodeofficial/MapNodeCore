using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
  

    public class SaleDefiRepository : EFRepository<SaleDefi, Guid>, ISaleDefiRepository
    {
        public SaleDefiRepository(AppDbContext context) : base(context)
        {
        }
    }
}
