using BeCoreApp.Data.Enums;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class WalletTransactionService : IWalletTransactionService
    {

        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWalletService _walletService;
        public WalletTransactionService(
          IWalletTransactionRepository walletTransactionRepository,
          IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
          IConfiguration configuration,
          IWalletService walletService)
        {
            _configuration = configuration;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _walletService = walletService;
        }
        public PagedResult<WalletTransactionViewModel> GetAllPaging(
            string keyword, Guid? userId, int pageIndex, int pageSize)
        {
            var query = _walletTransactionRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.UserName.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            if (userId != null)
                query = query.Where(x => x.AppUserId == userId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Email = x.AppUser.Email,
                    Sponsor = $"{x.AppUser.Sponsor}",
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    Unit = x.Unit,
                    UnitName = x.Unit.GetDescription(),
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Remarks = x.Remarks
                }).ToList();

            return new PagedResult<WalletTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }


        public PagedResult<WalletTransactionViewModel> GetExchangeLeaderBoardPaging(
            string keyword, int pageIndex, int pageSize)
        {
            var query = _walletTransactionRepository.FindAll(
                x => x.Type == WalletTransactionType.BuySeedRound
                && x.Unit == "MNI"
            , x => x.AppUser
            );

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.UserName.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));


            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Amount)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    Unit = x.Unit,
                    UnitName = x.Unit.GetDescription(),
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Remarks = x.Remarks
                }).ToList();

            PrepareBNBAmountLeaderBoard(data);

            return new PagedResult<WalletTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        private void PrepareBNBAmountLeaderBoard(List<WalletTransactionViewModel> leaderboards)
        {
            foreach (var item in leaderboards)
            {
                var bnb = _walletTransactionRepository.FindAll(x => x.TransactionHash.Equals(item.TransactionHash)
                    && x.Unit.Equals(WalletTransactionUnit.BNB.GetDescription())
                    && x.Type == WalletTransactionType.BuySeedRound).SingleOrDefault();

                if (bnb != null)
                    item.BNBAmount = bnb.Amount;
            }
        }

        public void Add(WalletTransactionViewModel model)
        {
            var transaction = new WalletTransaction()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,
                Fee = model.Fee,
                FeeAmount = model.FeeAmount,
                AmountReceive = model.AmountReceive,
                Amount = model.Amount,
                AppUserId = model.AppUserId,
                DateCreated = model.DateCreated,
                TransactionHash = model.TransactionHash,
                Type = model.Type,
                Unit = model.Unit,
                Remarks = model.Remarks
            };

            _walletTransactionRepository.Add(transaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public async Task<GenericResult> ProcessPaymentSaleAffiliate(Guid userId, decimal bnbAmount)
        {
            var userInfo = await _userManager.FindByIdAsync(userId.ToString());

            if (userInfo == null)
                return new GenericResult(false);

            AppUser f1User = null;
            AppUser f2User = null;
            AppUser f3User = null;

            #region F1

            if (userInfo.ReferralId != null)
                f1User = await _userManager.FindByIdAsync(userInfo.ReferralId.Value.ToString());

            if (f1User == null)
                return new GenericResult(false, "F1 not found");

            decimal rate = 9;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF1Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF1Rate");

            var refAmount = bnbAmount * (rate / 100);

            var updateF1Balance = _walletService
                .DepositToRegularWallet(f1User.Id, refAmount, (int)TokenConfigEnum.BNB);

            if (updateF1Balance)
            {
                AddTransaction(f1User, refAmount, refAmount, WalletTransactionType.AffiliateSeedRound,
                        "System", $"Wallet BNB", WalletTransactionUnit.BNB.GetDescription(), 0, 0);
            }

            #endregion

            #region F2

            if (f1User.ReferralId != null)
                f2User = await _userManager.FindByIdAsync(f1User.ReferralId.Value.ToString());

            if (f2User == null)
                return new GenericResult(false, "F2 not found");

            rate = 6;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF2Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF2Rate");

            refAmount = bnbAmount * (rate / 100);

            var updateF2Balance = _walletService.DepositToRegularWallet(f2User.Id, refAmount, (int)TokenConfigEnum.BNB);
            if (updateF2Balance)
            {
                AddTransaction(f2User, refAmount, refAmount, WalletTransactionType.AffiliateSeedRound,
                        "System", $"Wallet BNB", WalletTransactionUnit.BNB.GetDescription(), 0, 0);
            }
            #endregion

            #region F3

            if (f2User.ReferralId != null)
                f3User = await _userManager.FindByIdAsync(f2User.ReferralId.Value.ToString());

            if (f3User == null)
                return new GenericResult(false, "F3 not found");

            rate = 3;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF3Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF3Rate");

            refAmount = bnbAmount * (rate / 100);

            var updateF3Balance = _walletService.DepositToRegularWallet(f3User.Id, refAmount, (int)TokenConfigEnum.BNB);

            if (updateF3Balance)
            {
                AddTransaction(f3User, refAmount, refAmount, WalletTransactionType.AffiliateSeedRound,
                        "System", $"Wallet BNB", WalletTransactionUnit.BNB.GetDescription(), 0, 0);
            }
            #endregion

            return new GenericResult(true);
        }

        public async Task<GenericResult> PaySavingAffiliateDirectAsync(Guid appUserId, decimal usdSaving)
        {
            var userInfo = await _userManager.FindByIdAsync(appUserId.ToString());

            if (userInfo == null)
                return new GenericResult(false);

            AppUser f1User = null;

            if (userInfo.ReferralId != null)
            {
                f1User = await _userManager.FindByIdAsync(userInfo.ReferralId.Value.ToString());
            }

            if (f1User == null)
                return new GenericResult(false, "F1 not found");

            decimal rate = 6;
            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingReferralDirect"]))
            {
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingReferralDirect");
            }

            var refAmount = usdSaving * (rate / 100);

            var result = _walletService.DepositToRegularWallet(f1User.Id, refAmount, (int)TokenConfigEnum.MNO);
            if (result)
            {
                AddTransaction(f1User, refAmount, refAmount,
                    WalletTransactionType.SavingReferralDirect,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}", WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

            return new GenericResult(true);
        }

        public async Task LeaderShip(Guid appUserId, decimal usdAmount)
        {
            var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

            if (appUser != null)
            {
                appUser.SavingAmount += usdAmount;

                await _userManager.UpdateAsync(appUser);

                if (appUser.ReferralId.HasValue == true && appUser.IsSystem == false)
                {
                    decimal leadership = 0;

                    bool isContinue = true;

                    while (isContinue)
                    {
                        appUser = _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString()).Result;

                        if (appUser != null && appUser.IsSystem == false)
                        {
                            appUser.SavingAffiliateAmount += usdAmount;

                            var childrenF1s = _userManager.Users
                                    .Where(x => x.ReferralId == appUser.Id);

                            if (childrenF1s.Count() >= 2)
                            {
                                var childrenMax = childrenF1s
                                    .OrderByDescending(x => x.SavingAmount + x.SavingAffiliateAmount)
                                    .FirstOrDefault();

                                decimal maxSavingAmount = childrenMax.SavingAmount + childrenMax.SavingAffiliateAmount;

                                var childrenOthers = childrenF1s.Where(x => x.Id != childrenMax.Id);

                                decimal otherSavingAmount = childrenOthers.Sum(x => x.SavingAmount + x.SavingAffiliateAmount);

                                if (appUser.SavingLevel != SavingLevel.Start1)
                                {
                                    if (maxSavingAmount >= 20000 &&
                                        otherSavingAmount >= 20000 && otherSavingAmount < 50000)
                                    {
                                        leadership = 200;
                                        appUser.SavingLevel = SavingLevel.Start1;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start2)
                                {
                                    if (maxSavingAmount >= 50000 &&
                                        otherSavingAmount >= 50000 && otherSavingAmount < 100000)
                                    {
                                        leadership = 1000;
                                        appUser.SavingLevel = SavingLevel.Start2;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start3)
                                {
                                    if (maxSavingAmount >= 100000 &&
                                        otherSavingAmount >= 100000 && otherSavingAmount < 300000)
                                    {
                                        leadership = 3000;
                                        appUser.SavingLevel = SavingLevel.Start3;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start4)
                                {
                                    if (maxSavingAmount >= 300000 &&
                                        otherSavingAmount >= 300000 && otherSavingAmount < 600000)
                                    {
                                        leadership = 12000;
                                        appUser.SavingLevel = SavingLevel.Start4;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start5)
                                {
                                    if (maxSavingAmount >= 600000 &&
                                        otherSavingAmount >= 600000 && otherSavingAmount < 1000000)
                                    {
                                        leadership = 30000;
                                        appUser.SavingLevel = SavingLevel.Start5;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start6)
                                {
                                    if (maxSavingAmount >= 1000000 &&
                                        otherSavingAmount >= 1000000 && otherSavingAmount < 2000000)
                                    {
                                        leadership = 60000;
                                        appUser.SavingLevel = SavingLevel.Start6;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start7)
                                {
                                    if (maxSavingAmount >= 2000000 &&
                                        otherSavingAmount >= 2000000 && otherSavingAmount < 5000000)
                                    {
                                        leadership = 140000;
                                        appUser.SavingLevel = SavingLevel.Start7;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start8)
                                {
                                    if (maxSavingAmount >= 5000000 &&
                                        otherSavingAmount >= 5000000 && otherSavingAmount < 10000000)
                                    {
                                        leadership = 400000;
                                        appUser.SavingLevel = SavingLevel.Start8;
                                    }
                                }
                                else if (appUser.SavingLevel != SavingLevel.Start9)
                                {
                                    if (maxSavingAmount >= 10000000 &&
                                        otherSavingAmount >= 10000000)
                                    {
                                        leadership = 900000;
                                        appUser.SavingLevel = SavingLevel.Start9;
                                    }
                                }
                            }

                            var updateLeaderShip = _userManager.UpdateAsync(appUser).Result;

                            if (updateLeaderShip.Succeeded)
                            {
                                if (leadership > 0)
                                {
                                    var updateLeadership = _walletService
                                           .DepositToRegularWallet(appUser.Id, leadership, (int)TokenConfigEnum.MNO);

                                    if (updateLeadership)
                                    {
                                        AddTransaction(appUser, leadership, leadership,
                                            WalletTransactionType.SavingLeadershipCommission,
                                            "System", "Wallet MNO", CommonConstants.TOKEN_OUT_CODE, 0, 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                }
            }

        }

        public void AddTransaction(AppUser appUser, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string remarks = "")
        {
            Add(new WalletTransactionViewModel
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                AmountReceive = amountReceive,
                Amount = amount,
                AppUserId = appUser.Id,
                TransactionHash = Guid.NewGuid().ToString("N"),
                FeeAmount = feeAmount,
                Fee = fee,
                DateCreated = DateTime.Now,
                Unit = unit,
                Type = type,
                Remarks = remarks
            });

            Save();
        }

        public void AddTransaction(Guid appUserId, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string remarks = "")
        {
            Add(new WalletTransactionViewModel
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                AmountReceive = amount,
                Amount = amount,
                AppUserId = appUserId,
                TransactionHash = Guid.NewGuid().ToString("N"),
                FeeAmount = feeAmount,
                Fee = fee,
                DateCreated = DateTime.Now,
                Unit = unit,
                Type = type,
                Remarks = remarks
            });

            Save();
        }

        public void AddTransaction(AppUser appUser, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            string unit, decimal fee, decimal feeAmount, string transactionHash, string remarks = "")
        {
            Add(new WalletTransactionViewModel
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                AmountReceive = amountReceive,
                Amount = amount,
                AppUserId = appUser.Id,
                TransactionHash = transactionHash,
                FeeAmount = feeAmount,
                Fee = fee,
                DateCreated = DateTime.Now,
                Unit = unit,
                Type = type,
                Remarks = remarks
            });

            Save();
        }

        public async Task<GenericResult> ProcessPaymentSwapAffiliate(Guid userId, decimal amount)
        {
            var userInfo = await _userManager.FindByIdAsync(userId.ToString());

            if (userInfo == null)
                return new GenericResult(false);

            AppUser f1User = null;
            AppUser f2User = null;
            AppUser f3User = null;

            #region F1

            if (userInfo.ReferralId != null)
                f1User = await _userManager.FindByIdAsync(userInfo.ReferralId.Value.ToString());

            if (f1User == null)
                return new GenericResult(false, "F1 not found");

            decimal rate = 9;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF1Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF1Rate");

            var refAmount = amount * (rate / 100);

            var updateF1Balance = _walletService
                .DepositToRegularWallet(f1User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateF1Balance)
            {
                AddTransaction(f1User, refAmount, refAmount, WalletTransactionType.AffiliateSwapMNOToMNI,
                        "System",
                        $"Wallet {WalletTransactionUnit.MNO.GetDescription()}",
                        WalletTransactionUnit.MNO.GetDescription(),
                        0,
                        0,
                        $"Affiliate Swap MNO to MNI from user {userInfo.Email}");
            }

            #endregion

            #region F2

            if (f1User.ReferralId != null)
                f2User = await _userManager.FindByIdAsync(f1User.ReferralId.Value.ToString());

            if (f2User == null)
                return new GenericResult(false, "F2 not found");

            rate = 6;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF2Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF2Rate");

            refAmount = amount * (rate / 100);

            var updateF2Balance = _walletService
                .DepositToRegularWallet(f2User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateF2Balance)
            {
                AddTransaction(f2User, refAmount, refAmount, WalletTransactionType.AffiliateSwapMNOToMNI,
                        "System", $"Wallet {WalletTransactionUnit.MNO.GetDescription()}",
                        WalletTransactionUnit.MNO.GetDescription(), 0, 0,
                    $"Affiliate Swap MNO to MNI from user {userInfo.Email}");
            }
            #endregion

            #region F3

            if (f2User.ReferralId != null)
                f3User = await _userManager.FindByIdAsync(f2User.ReferralId.Value.ToString());

            if (f3User == null)
                return new GenericResult(false, "F3 not found");

            rate = 3;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SaleF3Rate"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SaleF3Rate");

            refAmount = amount * (rate / 100);

            var updateF3Balance = _walletService
                .DepositToRegularWallet(f3User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateF3Balance)
            {
                AddTransaction(f3User,
                    refAmount,
                    refAmount,
                    WalletTransactionType.AffiliateSwapMNOToMNI,
                    "System",
                    $"Wallet {WalletTransactionUnit.MNO.GetDescription()}",
                    WalletTransactionUnit.MNO.GetDescription(),
                    0,
                    0,
                    $"Affiliate Swap MNO to MNI from user {userInfo.Email}");
            }
            #endregion

            return new GenericResult(true);
        }
    }
}
