using Core.Application.Interfaces;
using Core.Application.ViewModels;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Saving;
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
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class SavingService : ISavingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ISavingRepository _savingRepository;
        private readonly ITokenConfigService _tokenConfigService;
        private readonly ISavingRewardRepository _savingRewardRepository;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;

        public SavingService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ISavingRepository savingRepository,
            ITokenConfigService tokenConfigService,
            UserManager<AppUser> userManager,
            ISavingRewardRepository savingRewardRepository,
            IWalletTransactionService walletTransactionService,
            IWalletService walletService
            )
        {
            _walletService = walletService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _configuration = configuration;
            _tokenConfigService = tokenConfigService;
            _savingRepository = savingRepository;
            _walletTransactionService = walletTransactionService;
            _savingRewardRepository = savingRewardRepository;
        }

        public PagedResult<SavingViewModel> GetAllPaging(
            string userName, string keyword = "", int pageIndex = 1, int pageSize = 20)
        {

            var query = _savingRepository.FindAll(x => x.AppUser, x => x.TokenConfig);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(x => x.AppUser.UserName == userName);


            var totalRow = query.Count();

            var data = query.OrderBy(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable()
                .Select(x => PrepareSavingModel(x)).ToList();

            return new PagedResult<SavingViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<SavingViewModel> GetAllLeaderBoardPaging(int pageIndex = 1, int pageSize = 20)
        {

            var query = _savingRepository.FindAll(x => x.AppUser, x => x.TokenConfig);


            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.AmountUSD)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable()
                .Select(x => PrepareSavingModel(x)).ToList();

            return new PagedResult<SavingViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        static SavingViewModel PrepareSavingModel(Saving saving)
        {
            var model = new SavingViewModel()
            {
                Id = saving.Id,
                EndDate = saving.CreatedOn.AddDays((int)saving.Timeline),
                SavingDate = saving.CreatedOn,
                SavingAmount = saving.AmountSaving,
                TimeLine = (int)saving.Timeline,
                TokenCode = saving.TokenConfig.TokenCode,
                TokenImage = saving.TokenConfig.TokenImageUrl,
                TokenName = saving.TokenConfig.Name,
                USDAmount = saving.AmountUSD,
                ExpectedInterested = saving.ExpectedInterested,
                InterestedRate = saving.InterestedRate,
                Status = saving.Type.GetDescription(),
                ValueDate = saving.StartOn,
                Sponsor = saving.AppUser.Sponsor,
                UserName = saving.AppUser.UserName
            };

            return model;
        }

        public ProcessResultModel AddSaving(ProcessSavingModel request)
        {
            var savingInfo = new Saving
            {
                AmountSaving = request.Amount,
                AppUserId = request.AppUserId,
                CreatedOn = DateTime.Now,
                StartOn = DateTime.Now.AddDays(1),
                Timeline = request.Timeline,
                TimesReceived = (int)request.Timeline,
                TokenConfigId = request.TokenConfigId,
                Type = SavingType.Active,
                TransactionId = request.DaapTransactionId,
                AmountUSD = request.AmountUSD,
                ReceivedCount = 0,
                RemainCount = (int)request.Timeline,
                InterestedRate = request.InterestRate,
                ExpectedInterested = request.ExpectedInterest
            };

            _savingRepository.Add(savingInfo);

            _unitOfWork.Commit();

            return new ProcessResultModel
            {
                IsSuccess = true
            };
        }

        public async Task<GenericResult> ProcessDailySavingAffiliate()
        {
            var savings = GetActiveSavingContracts();

            if (savings.Count() == 0)
            {
                return new GenericResult(true);
            }

            foreach (var saving in savings)
            {
                ProcessContractStatus(saving);

                AddSavingProfitHistory(saving);

                await PaymentSavingProfit(saving);

                await ProcessPaymentSavingProfitOnReferral(saving);
            }

            return new GenericResult(true);
        }

        void ProcessContractStatus(Saving contract)
        {
            contract.RemainCount--;
            contract.ReceivedCount++;
            contract.LastReceived = DateTime.Now;

            if (contract.RemainCount == 0)
            {
                contract.Type = SavingType.Complete;
                contract.CompletionOn = DateTime.Now;
            }

            _savingRepository.Update(contract);

            _unitOfWork.Commit();
        }

        void AddSavingProfitHistory(Saving saving)
        {
            var profit = saving.AmountUSD * saving.InterestedRate / 100;

            _savingRewardRepository.AddSavingProfit(
                saving.AppUserId,
                saving.Id,
                saving.InterestedRate,
                profit,
                SavingRewardType.InterestRate);

            _unitOfWork.Commit();
        }

        void AddSavingProfitHistory(Guid userId, Guid? referralId, int contractId,
            decimal interestRate, decimal profit, SavingRewardType type, string remarks)
        {
            _savingRewardRepository.AddSavingProfit(
                userId,
                contractId,
                interestRate,
                profit,
                referralId,
                type,
                remarks);

            _unitOfWork.Commit();
        }

        async Task PaymentSavingProfit(Saving saving)
        {
            var tokenAmount = saving.AmountUSD * saving.InterestedRate / 100;

            var updateSavingProfit = _walletService.DepositToRegularWallet(
                saving.AppUserId, tokenAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfit)
            {
                var user = await _userManager.FindByIdAsync(saving.AppUserId.ToString());

                _walletTransactionService.AddTransaction(user, tokenAmount, tokenAmount,
                    WalletTransactionType.SavingProfit, "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

        }
        public List<Saving> GetActiveSavingContracts()
        {
            var nowDate = DateTime.Now.Date;
            var query = _savingRepository
                .FindAll(x => x.Type == SavingType.Active && x.StartOn.Date <= nowDate
                && (x.LastReceived == null || x.LastReceived.Value.Date < nowDate));

            return query.ToList();
        }

        private async Task<GenericResult> ProcessPaymentSavingProfitOnReferral(Saving saving)
        {
            var profitToday = saving.AmountUSD * (saving.InterestedRate / 100);

            var userInfo = await _userManager.FindByIdAsync(saving.AppUserId.ToString());

            if (userInfo == null)
            {
                return new GenericResult(false);
            }

            AppUser f1User = null;
            AppUser f2User = null;
            AppUser f3User = null;
            AppUser f4User = null;
            AppUser f5User = null;

            if (userInfo.ReferralId != null)
            {
                f1User = await _userManager.FindByIdAsync(userInfo.ReferralId.Value.ToString());
            }

            if (f1User == null)
            {
                return new GenericResult(false, "F1 not found");
            }

            decimal rate = 8m;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingProfitOnReferral1"]))
            {
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingProfitOnReferral1");
            }

            var refAmount = profitToday * (rate / 100);

            var updateSavingProfitOnReferral = _walletService.DepositToRegularWallet(
                f1User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfitOnReferral)
            {
                AddSavingProfitHistory(
                    f1User.Id,
                    userInfo.Id,
                    saving.Id,
                    rate,
                    refAmount,
                    SavingRewardType.Commission,
                    $"Payment saving referral for F1 of user {userInfo.Id} rate {rate}");

                _walletTransactionService.AddTransaction(f1User, refAmount,
                    refAmount, WalletTransactionType.SavingProfitOnReferral,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }


            if (f1User.ReferralId != null)
            {
                f2User = await _userManager.FindByIdAsync(f1User.ReferralId.Value.ToString());
            }

            if (f2User == null)
            {
                return new GenericResult(false, "F2 not found");
            }

            rate = 6m;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingProfitOnReferral2"]))
            {
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingProfitOnReferral2");
            }

            refAmount = profitToday * rate / 100;

            updateSavingProfitOnReferral = _walletService.DepositToRegularWallet(
               f2User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfitOnReferral)
            {
                AddSavingProfitHistory(
                    f2User.Id,
                    userInfo.Id,
                    saving.Id,
                    rate,
                    refAmount,
                    SavingRewardType.Commission,
                    $"Payment saving referral for F2 of user {userInfo.Id} rate {rate}");

                _walletTransactionService.AddTransaction(f2User, refAmount,
                    refAmount, WalletTransactionType.SavingProfitOnReferral,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

            if (f2User.ReferralId != null)
            {
                f3User = await _userManager.FindByIdAsync(f2User.ReferralId.Value.ToString());
            }

            if (f3User == null)
            {
                return new GenericResult(false, "F3 not found");
            }

            rate = 4m;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingProfitOnReferral3"]))
            {
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingProfitOnReferral3");
            }

            refAmount = profitToday * rate / 100;

            updateSavingProfitOnReferral = _walletService.DepositToRegularWallet(
               f3User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfitOnReferral)
            {
                AddSavingProfitHistory(
                    f3User.Id,
                    userInfo.Id,
                    saving.Id,
                    rate,
                    refAmount,
                    SavingRewardType.Commission,
                    $"Payment saving referral for F3 of user {userInfo.Id} rate {rate}");

                _walletTransactionService.AddTransaction(f3User, refAmount,
                    refAmount, WalletTransactionType.SavingProfitOnReferral,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

            if (f3User.ReferralId != null)
            {
                f4User = await _userManager.FindByIdAsync(f3User.ReferralId.Value.ToString());
            }

            if (f4User == null)
            {
                return new GenericResult(false, "F4 not found");
            }

            rate = 2m;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingProfitOnReferral4"]))
            {
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingProfitOnReferral4");
            }

            refAmount = profitToday * rate / 100;

            updateSavingProfitOnReferral = _walletService.DepositToRegularWallet(
               f4User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfitOnReferral)
            {
                AddSavingProfitHistory(
                    f4User.Id,
                    userInfo.Id,
                    saving.Id,
                    rate,
                    refAmount,
                    SavingRewardType.Commission,
                    $"Payment saving referral for F4 of user {userInfo.Id} rate {rate}");

                _walletTransactionService.AddTransaction(f4User, refAmount,
                    refAmount, WalletTransactionType.SavingProfitOnReferral,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

            if (f4User.ReferralId != null)
            {
                f5User = await _userManager.FindByIdAsync(f4User.ReferralId.Value.ToString());
            }

            if (f5User == null)
            {
                return new GenericResult(false, "f5 not found");
            }

            rate = 1m;

            if (!string.IsNullOrEmpty(_configuration["TokenConfig:SavingProfitOnReferral5"]))
                rate = _configuration.GetValue<decimal>("TokenConfig:SavingProfitOnReferral5");

            refAmount = profitToday * rate / 100;

            updateSavingProfitOnReferral = _walletService.DepositToRegularWallet(
               f5User.Id, refAmount, (int)TokenConfigEnum.MNO);

            if (updateSavingProfitOnReferral)
            {
                AddSavingProfitHistory(
                    f5User.Id,
                    userInfo.Id,
                    saving.Id,
                    rate,
                    refAmount,
                    SavingRewardType.Commission,
                    $"Payment saving referral for F5 of user {userInfo.Id} rate {rate}");

                _walletTransactionService.AddTransaction(f5User, refAmount,
                    refAmount, WalletTransactionType.SavingProfitOnReferral,
                    "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}",
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);
            }

            return new GenericResult(true);
        }


        public PagedResult<SavingRewardViewModel> GetAllSavingRewardPaging(
            string userName, string keyword = "", int pageIndex = 1, int pageSize = 20)
        {
            var query = _savingRewardRepository
                .FindAll(x => !x.ReferralId.HasValue, sv => sv.Saving, x => x.AppUser);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(x => x.AppUser.UserName == userName);

            var totalRow = query.Count();

            var data = query.OrderBy(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable()
                .Select(x => PrepareSavingRewardModel(x)
                .GetAwaiter()
                .GetResult()
                ).ToList();

            return new PagedResult<SavingRewardViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<SavingRewardViewModel> GetAllSavingCommissionPaging(
            string userName, string keyword = "", int pageIndex = 1, int pageSize = 20)
        {
            var query = _savingRewardRepository
                .FindAll(x => x.ReferralId.HasValue, sv => sv.Saving);

            query = query.Where(x => x.AppUser.UserName == userName);

            var totalRow = query.Count();

            var data = query.OrderBy(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable()
                .Select(x => PrepareSavingRewardModel(x).GetAwaiter().GetResult()).ToList();

            return new PagedResult<SavingRewardViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        private async Task<SavingRewardViewModel> PrepareSavingRewardModel(SavingReward savingReward)
        {
            var saving = savingReward.Saving;

            var tokenConfig = _tokenConfigService.GetById(saving.TokenConfigId);

            var referralName = string.Empty;

            if (savingReward.ReferralId.HasValue)
            {
                var referralSaving = await _userManager.FindByIdAsync(savingReward.ReferralId.Value.ToString());

                if (referralSaving != null)
                {
                    referralName = referralSaving.UserName;
                }
            }

            var model = new SavingRewardViewModel()
            {
                TypeName = savingReward.Type.GetDescription(),
                Type = savingReward.Type,
                Amount = savingReward.Amount,
                CreatedOn = savingReward.CreatedOn,
                InterestedRate = savingReward.InterestRate,
                ReferralId = savingReward.ReferralId,
                ReferralName = referralName,
                Remarks = savingReward.Remarks,
                TokenCode = tokenConfig.TokenCode,
                TokenName = tokenConfig.Name,
                TokenImage = tokenConfig.TokenImageUrl,
                AppUserName = saving.AppUser.UserName,
                Sponsor = saving.AppUser.Sponsor
            };

            return model;
        }
    }
}
