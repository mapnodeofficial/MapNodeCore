using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Utilities.Dtos;
using Core.Utilities.Dtos.Datatables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ISavingDefiService
    {
        PagedResult<SavingDefiViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize);


        Task<PagedResult<SavingDefi>> GetTransactionsAsync(
            string key, int pageIndex, int pageSize, string type, string userId);

        Task<(GenericResult result, string transactionId)> InitializeTransactionProgress(
            DappInitializationParams model, Guid userId);

        Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry);

        Task<PagedResult<DAppTransactionView>> GetTransactionsAsync(
            string keyword, int pageIndex, int pageSize);

        string GetLatestTransactionByWalletAddress(Guid userId, string dappTxnHash);

        Task<GenericResult> ProcessVerificationSmartContractTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry = false);

        Task<GenericResult> ProcessPaymentSaving(int tokenConfigId,
            int timeline,
            Guid appUserId,
            decimal tokenAmount);
    }
}
