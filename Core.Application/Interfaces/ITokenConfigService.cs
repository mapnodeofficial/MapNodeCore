using Core.Application.ViewModels.Saving;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ITokenConfigService
    {
        PagedResult<TokenConfigViewModel> GetAllPaging(string keyword = "",
            TokenConfigType type = 0, int pageIndex = 1, int pageSize = 20);

        TokenConfigViewModel GetById(int id);

        void Update(TokenConfigViewModel model);

        void Add(TokenConfigViewModel model);

        void Delete(int id);

        decimal GetInterestRate(int id, SavingTimeLine timeline);

        TokenConfigViewModel GetByCode(string tokenCode);

        decimal GetDailyInterestRate(int id, SavingTimeLine timeline);
    }
}
