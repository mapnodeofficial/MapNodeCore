using Core.Application.ViewModels;
using Core.Application.ViewModels.Saving;
using Core.Data.Entities;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ISavingService
    {
        ProcessResultModel AddSaving(ProcessSavingModel request);

        List<Saving> GetActiveSavingContracts();

        Task<GenericResult> ProcessDailySavingAffiliate();

        PagedResult<SavingViewModel> GetAllPaging(string userName, string keyword = "", int pageIndex = 1, int pageSize = 20);

        PagedResult<SavingRewardViewModel> GetAllSavingRewardPaging(
            string userName, string keyword = "", int pageIndex = 1, int pageSize = 20);

        PagedResult<SavingRewardViewModel> GetAllSavingCommissionPaging(
            string userName, string keyword = "", int pageIndex = 1, int pageSize = 20);

        PagedResult<SavingViewModel> GetAllLeaderBoardPaging(int pageIndex = 1, int pageSize = 20);
    }
}
