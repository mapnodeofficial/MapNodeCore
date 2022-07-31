using BeCoreApp.Data.Entities;
using BeCoreApp.Data.IRepositories;
using Core.Application.Interfaces;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Implementation
{
    public class WalletService : IWalletService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletRepository _walletRepository;
        private IUnitOfWork _unitOfWork;
        public WalletService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            IWalletRepository walletRepository
            )
        {
            _unitOfWork = unitOfWork;
            _walletRepository = walletRepository;
            _userManager = userManager;
            _blockChainService = blockChainService;
        }

        public List<Wallet> GetAllByUserId(Guid appUserId)
        {
            var query = _walletRepository
                .FindAll(x => x.AppUserId == appUserId, tk => tk.TokenConfig)
                .OrderBy(x => x.TokenConfig.ArrangeOrder).ToList();

            return query;
        }

        public void GenerateUserDefaultWallet(Guid appUserId)
        {
            _walletRepository.GenerateUserDefaultWallet(appUserId);
        }

        public decimal GetBNBBalance(Guid appUserId)
        {
            var walletBnb = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == (int)TokenConfigEnum.BNB).SingleOrDefault();

            if (walletBnb == null)
                return 0;

            return walletBnb.Amount;
        }

        


        public decimal GetTokenBalance(Guid appUserId, int tokenConfigId)
        {
            var wallet = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == tokenConfigId).SingleOrDefault();

            if (wallet == null)
                return 0;

            return wallet.Amount;
        }

        public bool DepositToBNBWallet(Guid appUserId, decimal amount)
        {
            var walletBnb = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == (int)TokenConfigEnum.BNB).SingleOrDefault();
            if (walletBnb == null)
                return false;

            walletBnb.Amount += amount;

            _walletRepository.Update(walletBnb);

            _unitOfWork.Commit();

            return true;

        }

        public bool DepositToRegularWallet(Guid appUserId, decimal amount, int tokenConfigId)
        {
            var wallet = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == tokenConfigId).SingleOrDefault();

            if (wallet == null)
                return false;

            wallet.Amount += amount;
            wallet.UpdatedOn = DateTime.Now;

            _walletRepository.Update(wallet);
            _unitOfWork.Commit();

            return true;
        }

        public bool WithdrawToRegularWallet(Guid appUserId, decimal amount, int tokenConfigId)
        {
            var wallet = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == tokenConfigId).SingleOrDefault();

            if (wallet == null)
                return false;

            if (wallet.Amount<amount)
                return false;

            wallet.Amount -= amount;
            wallet.UpdatedOn = DateTime.Now;

            _walletRepository.Update(wallet);

            _unitOfWork.Commit();

            return true;

        }

        public bool WithdrawToBNBWallet(Guid appUserId, decimal amount)
        {
            var walletBnb = _walletRepository.FindAll(x => x.AppUserId == appUserId
                    && x.TokenConfigId == (int)TokenConfigEnum.BNB).SingleOrDefault();
            
            if (walletBnb == null)
                return false;

            walletBnb.Amount -= amount;
            walletBnb.UpdatedOn = DateTime.Now;

            _walletRepository.Update(walletBnb);

            _unitOfWork.Commit();

            return true;
        }

        public Wallet GetByTokenIdAndUserId(int tokenId, Guid userId)
        {
            var model = _walletRepository
                .FindAll(x => x.AppUserId == userId && x.TokenConfigId == tokenId).FirstOrDefault();

            return model;
        }

        public void Update(Wallet wallet)
        {
            wallet.UpdatedOn = DateTime.Now;
            _walletRepository.Update(wallet);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
