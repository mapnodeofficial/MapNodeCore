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
    public interface IWalletTransactionService
    {
        PagedResult<WalletTransactionViewModel> GetAllPaging(
            string keyword, Guid? userId, int pageIndex, int pageSize);

        void Add(WalletTransactionViewModel Model);

        Task<GenericResult> ProcessPaymentSaleAffiliate(Guid userId, decimal tokenAmount);

        Task<GenericResult> PaySavingAffiliateDirectAsync(Guid userId, decimal usdSaving);

        Task LeaderShip(Guid appUserId, decimal stakingToken);

        void AddTransaction(AppUser appUser, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string remarks = "");

        void Save();

        PagedResult<WalletTransactionViewModel> GetExchangeLeaderBoardPaging(
            string keyword, int pageIndex, int pageSize);

        void AddTransaction(Guid appUserId, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string remarks = "");

        void AddTransaction(AppUser appUser, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string transactionHash, string remarks = "");


        Task<GenericResult> ProcessPaymentSwapAffiliate(Guid userId, decimal amount);
    }
}
