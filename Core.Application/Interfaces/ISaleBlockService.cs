using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ISaleBlockService
    {
        PagedResult<SaleBlockViewModel> GetAllPaging(
            string keyword,string userName, int pageIndex, int pageSize);

        Task<GenericResult> LockBlockAsync(Guid userId, decimal tokenAmount);

        Task<GenericResult> UnlockBlockAsync();

        void Save();
    }
}
