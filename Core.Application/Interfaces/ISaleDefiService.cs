using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.Saving;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{

    public interface ISaleDefiService
    {
        PagedResult<SaleDefiViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize);

        Task<(GenericResult result, string transactionId)> InitializeTransactionProgress(
            SaleInitializationParams model,
            Guid userId);
        Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry);

        string GetLatestTransactionByWalletAddress(
            Guid userId, string dappTxnHash);

        Task<GenericResult> ProcessBuyTokenByBNBsync(decimal tokenAmount, Guid appUserId);


        Task<GenericResult> ProcessSwapTokenByMNOAsync(decimal mnoAmount,
            Guid appUserId, TokenConfigViewModel tokenConfig);
    }
}
