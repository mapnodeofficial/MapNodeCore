using BeCoreApp.Data.Entities;
using BeCoreApp.Data.IRepositories;
using System;
using Microsoft.EntityFrameworkCore;
using Core.Data.EF;

namespace BeCoreApp.Data.EF.Repositories
{
    public class WalletRepository : EFRepository<Wallet, int>, IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void GenerateUserDefaultWallet(Guid appUserId)
        {
            object value = _context.Database.ExecuteSqlRaw($"exec spGenerateUserWallet @userId = '{appUserId}'");
        }
    }
}
