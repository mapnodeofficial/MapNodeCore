using BeCoreApp.Data.Entities;
using Core.Infrastructure.Interfaces;
using System;

namespace BeCoreApp.Data.IRepositories
{
    public interface IWalletRepository : IRepository<Wallet, int>
    {
        void GenerateUserDefaultWallet(Guid appUserId);
    }
}