using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class SaleBlockService : ISaleBlockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ISaleBlockRepository _saleBlockRepository;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletTransactionService _walletTransactionService;

        public SaleBlockService(
          IUnitOfWork unitOfWork,
          IBlockChainService blockChainService,
          IConfiguration configuration,
          UserManager<AppUser> userManager,
          ISaleBlockRepository saleBlockRepository,
          IWalletTransactionService walletTransactionService
            )
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _saleBlockRepository = saleBlockRepository;
            _walletTransactionService = walletTransactionService;
        }
        public PagedResult<SaleBlockViewModel> GetAllPaging(
            string keyword, string userName, int pageIndex, int pageSize)
        {
            var query = _saleBlockRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.UserName.Contains(keyword));

            if (!string.IsNullOrEmpty(userName))
               query = query.Where(x => x.AppUser.UserName == userName);

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new SaleBlockViewModel()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    DateUpdated = x.DateUpdated,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    RoundTypeName = x.RoundType.GetDescription(),
                    RoundType = x.RoundType,
                    StartOn = x.StartOn,
                    Sponsor = x.AppUser.Sponsor
                }).ToList();

            return new PagedResult<SaleBlockViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public async Task<GenericResult> LockBlockAsync(Guid userId, decimal totalTokenAmount)
        {
            var appUser = await _userManager.FindByIdAsync(userId.ToString());

            if (appUser == null)
            {
                return new GenericResult(false, "User not found");
            }

            DateTime nowDate = DateTime.Now.Date;

            DateTime endLockDate = nowDate.AddMonths(4);

            int blockCount = 10;

            decimal toalTokenLock = Math.Round(totalTokenAmount * 0.8m, 2);

            decimal tokenOnBlock = Math.Round(toalTokenLock / blockCount, 2);

            for (int i = 1; i <= blockCount; i++)
            {
                _saleBlockRepository.Add(new SaleBlock
                {
                    Amount = tokenOnBlock,
                    AppUserId = appUser.Id,
                    RoundType = SaleDefiTransactionType.SeedRound,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    Type = SaleBlockType.Pending,
                    StartOn = endLockDate.AddMonths(i)
                });
            }

            Save();

            return new GenericResult(true);
        }

        public async Task<GenericResult> UnlockBlockAsync()
        {
            DateTime nowDate = DateTime.Now.Date;

            var blockUnlocks = _saleBlockRepository.FindAll();

            blockUnlocks = blockUnlocks.Where(x => x.Type == SaleBlockType.Pending && x.StartOn <= nowDate);

            foreach (var blockUnlock in blockUnlocks)
            {
                var appUser = await _userManager.FindByIdAsync(blockUnlock.AppUserId.ToString());

                if (appUser == null)
                    continue;


                decimal balanceTransfer = blockUnlock.Amount;

                var transaction = await _blockChainService.SendERC20Async(
                            CommonConstants.BEP20TransferPrKey, appUser.UserName,
                            CommonConstants.BEP20MNIContract, balanceTransfer,
                            CommonConstants.BEP20MNIDP, CommonConstants.BEP20Url);

                if (transaction.Succeeded(true))
                {
                    blockUnlock.DateUpdated = DateTime.Now;
                    blockUnlock.Type = SaleBlockType.Successed;
                    blockUnlock.TransactionHash = transaction.TransactionHash;

                    _walletTransactionService.Add(new WalletTransactionViewModel
                    {
                        AddressFrom = CommonConstants.BEP20TransferPuKey,
                        AddressTo = appUser.UserName,
                        Amount = balanceTransfer,
                        AmountReceive = balanceTransfer,
                        AppUserId = appUser.Id,
                        DateCreated = DateTime.Now,
                        TransactionHash = transaction.TransactionHash,
                        Fee = 0,
                        FeeAmount = 0,
                        Type = WalletTransactionType.UnlockBlock,
                        Unit = "MNI",
                        Remarks = JsonConvert.SerializeObject(blockUnlock)
                    });

                    Save();
                }
            }

            return new GenericResult(true);
        }
    }
}
