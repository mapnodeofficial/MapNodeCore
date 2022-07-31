using BeCoreApp.Data.Entities;
using Core.Application.ViewModels.BlockChain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IWalletService
    {
        List<Wallet> GetAllByUserId(Guid appUserId);

        void GenerateUserDefaultWallet(Guid appUserId);

        decimal GetBNBBalance(Guid appUserId);

        bool DepositToBNBWallet(Guid appUserId, decimal amount);

        bool WithdrawToBNBWallet(Guid appUserId, decimal amount);

        Wallet GetByTokenIdAndUserId(int tokenId, Guid userId);

        void Update(Wallet wallet);

        void Save();

        decimal GetTokenBalance(Guid appUserId, int tokenConfigId);

        bool DepositToRegularWallet(Guid appUserId, decimal amount, int tokenConfigId);

        bool WithdrawToRegularWallet(Guid appUserId, decimal amount, int tokenConfigId);
    }
}
