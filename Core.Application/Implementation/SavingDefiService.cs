using Core.Application.Interfaces;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Application.ViewModels.Saving;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.SmartContracts.FunctionMessages.ICD;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class SavingDefiService : BaseService, ISavingDefiService
    {
        private readonly IBlockChainService _blockChain;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SavingDefiService> _logger;
        private readonly ISavingService _savingService;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly ITokenConfigService _tokenConfigService;
        private readonly ISavingDefiRepository _savingDefiRepository;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;
        public const string DAppTransactionId = "DAppTransactionId";

        private static AsyncRetryPolicy _policy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(15)
              });

        public SavingDefiService(
                           IUnitOfWork unitOfWork,
                           UserManager<AppUser> userManager,
                           IBlockChainService blockChain,
                           ILogger<SavingDefiService> logger,
                           ISavingService savingService,
                           TelegramBotWrapper botTelegramService,
                           ITokenConfigService tokenConfigService,
                           ISavingDefiRepository savingDefiRepository,
                           IWalletTransactionService walletTransactionService,
                           IWalletService walletService
            ) : base(userManager)
        {
            _walletService = walletService;
            _logger = logger;
            _blockChain = blockChain;
            _unitOfWork = unitOfWork;
            _savingService = savingService;
            _botTelegramService = botTelegramService;
            _tokenConfigService = tokenConfigService;
            _savingDefiRepository = savingDefiRepository;
            _walletTransactionService = walletTransactionService;
        }

        public async Task<(GenericResult result, string transactionId)>
            InitializeTransactionProgress(DappInitializationParams model, Guid userId)
        {
            decimal priceToken = _blockChain.GetCurrentPrice(model.TokenCode, "USD");

            if (priceToken == 0)
            {
                return (new GenericResult(false,
                    "There is a problem loading the currency value!"), string.Empty);
            }

            var tokenConfig = _tokenConfigService.GetByCode(model.TokenCode);

            if (model.Amount < tokenConfig.MinSaving)
            {
                return (new GenericResult(false,
                    $"Min saving {tokenConfig.MinSaving}"), string.Empty);
            }

            var transaction = new SavingDefi
            {
                Id = Guid.NewGuid(),
                AddressFrom = model.Address,
                AddressTo = CommonConstants.BEP20ReceivePuKey,
                DateCreated = DateTime.Now,
                TransactionState = SavingDefiTransactionState.Requested,
                IsDevice = model.IsDevice,
                TokenConfigId = tokenConfig.Id,
                TokenAmount = model.Amount,
                USDAmount = Math.Round(model.Amount * priceToken, 8),
                AppUserId = userId,
                Timeline = model.Timeline
            };

            await _savingDefiRepository.AddAsync(transaction);

            _unitOfWork.Commit();

            var message = new BlockchainParams
            {
                TransactionHex = transaction.Id.ToString().ToHexUTF8(),
                From = model.Address,
                To = CommonConstants.BEP20ReceivePuKey,
                Value = transaction.TokenAmount,
                Gas = "0x55f0",//22000
                GasPrice = "0x2540be400",
                Decimals = tokenConfig.Decimals
            };

            return (GenericResult.ToSuccess("Successed to initialize Transaction",
                message), transaction.Id.ToString());
        }

        public async Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry = false)
        {
            try
            {
                _logger.LogInformation($"Start calling VerifyMetaMaskRequest with transaction hash: {transactionHash}");

                var transactionReceipt = await _policy.ExecuteAsync(async () =>
                {
                    var result = await _blockChain.GetTransactionReceiptByTransactionID(transactionHash, CommonConstants.BEP20Url);
                    if (result == null)
                    {
                        _logger.LogInformation("retry get receipt of transaction hash: {0}", transactionHash);

                        throw new ArgumentNullException($"Cannot GetTransactionReceipt By {transactionHash}");
                    }

                    return result;
                });

                var transaction = await _blockChain.GetTransactionByTransactionID(transactionHash, CommonConstants.BEP20Url);

                var savingDefi = _savingDefiRepository.FindById(Guid.Parse(transaction.Input.HexToUTF8String()));

                var balance = Web3.Convert.FromWei(transaction.Value);

                if (!transactionReceipt.Succeeded(true))
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare dapp transaction with blockchain transaction
                if (!isRetry && savingDefi.TransactionState != SavingDefiTransactionState.Requested)
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {savingDefi.TransactionState}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {savingDefi.TransactionState}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare transaction with blockchain transaction
                if (savingDefi.AddressFrom.ToLower() != transaction.From.ToLower()
                    || savingDefi.AddressTo.ToLower() != transaction.To.ToLower())
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's infor was not matched:";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's infor was not matched: ", transaction);

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare amount
                if (balance != savingDefi.TokenAmount)
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                savingDefi.TransactionState = SavingDefiTransactionState.Confirmed;
                savingDefi.TokenTransactionHash = transactionHash;
                savingDefi.DateUpdated = DateTime.Now;

                _unitOfWork.Commit();


                var depositMessage = TelegramBotHelper
                    .BuildReportDepositSavingMessage(new DeFiMessageParam
                    {
                        Title = "Saving",
                        AmountUSD = savingDefi.USDAmount,
                        DepositAt = DateTime.Now,
                        AmountToken = savingDefi.TokenAmount,
                        UserWallet = savingDefi.AddressFrom,
                        SystemWallet = savingDefi.AddressTo,
                        Email = savingDefi.AddressFrom,
                        Currency = "BNB"
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);


                #region Saving

                var interestRate = _tokenConfigService.GetInterestRate(
                    savingDefi.TokenConfigId, (SavingTimeLine)savingDefi.Timeline);

                var result = _savingService.AddSaving(new ProcessSavingModel
                {
                    AppUserId = savingDefi.AppUserId,
                    Timeline = (SavingTimeLine)savingDefi.Timeline,
                    AddressFrom = savingDefi.AddressFrom,
                    TokenConfigId = savingDefi.TokenConfigId,
                    Amount = savingDefi.TokenAmount,
                    DaapTransactionId = savingDefi.Id,
                    AmountUSD = savingDefi.USDAmount,
                    InterestRate = interestRate,
                    ExpectedInterest = savingDefi.USDAmount * interestRate / 100
                });

                await _walletTransactionService.PaySavingAffiliateDirectAsync(
                    savingDefi.AppUserId, savingDefi.USDAmount);

                #endregion

                _logger.LogInformation($"End call VerifyMetaMaskRequest with transaction hash:");

                return new GenericResult(true, $"Successed to Buy {CommonConstants.TOKEN_IN_CODE}");
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var metamaskTransaction = _savingDefiRepository.FindById(Guid.Parse(tempDappTransaction));

                    metamaskTransaction.Remarks = e.Message;
                    metamaskTransaction.TokenTransactionHash = transactionHash;
                    metamaskTransaction.TransactionState = SavingDefiTransactionState.Failed;

                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return new GenericResult(false, e.Message);
            }
        }

        public async Task<GenericResult> ProcessVerificationSmartContractTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry = false)
        {
            try
            {
                _logger.LogInformation($"Start calling VerifyMetaMaskRequest " +
                    $"with transaction hash: {transactionHash}");

                var transactionReceipt = await _policy.ExecuteAsync(async () =>
                {
                    var result = await _blockChain.GetTransactionReceiptByTransactionID(
                        transactionHash, CommonConstants.BEP20Url);

                    if (result == null)
                    {
                        _logger.LogInformation("retry get receipt of transaction hash: {0}", transactionHash);

                        throw new ArgumentNullException($"Cannot GetTransactionReceipt By {transactionHash}");
                    }

                    return result;
                });

                var transaction = await _blockChain.GetTransactionByTransactionID(
                    transactionHash, CommonConstants.BEP20Url);

                if (transaction == null)
                {
                    _logger.LogInformation("retry get receipt of transaction hash: {0}",
                        transactionHash);

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                var transfer = new TransferFunction().DecodeInput(transaction.Input);

                var savingDefi = _savingDefiRepository.FindById(Guid.Parse(tempDappTransaction));

                var tokenConfig = _tokenConfigService.GetById(savingDefi.TokenConfigId);

                var balance = Web3.Convert.FromWei(transfer.TokenAmount, tokenConfig.Decimals);

                if (!transactionReceipt.Succeeded(true))
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare dapp transaction with blockchain transaction
                if (!isRetry && savingDefi.TransactionState != SavingDefiTransactionState.Requested)
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {savingDefi.TransactionState}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {savingDefi.TransactionState}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare transaction with blockchain transaction
                if (savingDefi.AddressFrom.ToLower() != transaction.From.ToLower()
                    ||
                    savingDefi.AddressTo.ToLower() != transfer.To.ToLower())
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's infor was not matched: ";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's infor was not matched: ", transaction);

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare amount
                if (balance != savingDefi.TokenAmount)
                {
                    savingDefi.DateUpdated = DateTime.Now;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                savingDefi.TransactionState = SavingDefiTransactionState.Confirmed;
                savingDefi.TokenTransactionHash = transactionHash;
                savingDefi.DateUpdated = DateTime.Now;

                _unitOfWork.Commit();


                var depositMessage = TelegramBotHelper
                    .BuildReportDepositSavingMessage(new DeFiMessageParam
                    {
                        Title = "Saving",
                        AmountUSD = savingDefi.USDAmount,
                        DepositAt = DateTime.Now,
                        AmountToken = savingDefi.TokenAmount,
                        UserWallet = savingDefi.AddressFrom,
                        SystemWallet = savingDefi.AddressTo,
                        Email = savingDefi.AddressFrom,
                        Currency = tokenConfig.TokenCode
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);


                #region Saving

                var interestRate = _tokenConfigService.GetInterestRate(
                    savingDefi.TokenConfigId, (SavingTimeLine)savingDefi.Timeline);

                var result = _savingService.AddSaving(
                    new ProcessSavingModel
                    {
                        AppUserId = savingDefi.AppUserId,
                        Timeline = (SavingTimeLine)savingDefi.Timeline,
                        AddressFrom = savingDefi.AddressFrom,
                        TokenConfigId = savingDefi.TokenConfigId,
                        Amount = savingDefi.TokenAmount,
                        DaapTransactionId = savingDefi.Id,
                        AmountUSD = savingDefi.USDAmount,
                        InterestRate = interestRate,
                        ExpectedInterest = interestRate * savingDefi.USDAmount / 100
                    });

                await _walletTransactionService.PaySavingAffiliateDirectAsync(
                    savingDefi.AppUserId, savingDefi.USDAmount);

                #endregion

                _logger.LogInformation($"End call VerifyMetaMaskRequest with transaction hash:");

                return new GenericResult(true, "Successed");
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var savingDefi = _savingDefiRepository.FindById(Guid.Parse(tempDappTransaction));

                    savingDefi.Remarks = e.Message;
                    savingDefi.TokenTransactionHash = transactionHash;
                    savingDefi.TransactionState = SavingDefiTransactionState.Failed;

                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return new GenericResult(false, e.Message);
            }
        }

        public async Task<PagedResult<SavingDefi>> GetTransactionsAsync(
            string key, int pageIndex, int pageSize, string type, string userId)
        {

            var query = _savingDefiRepository
                .FindAll(x => x.TransactionState == SavingDefiTransactionState.Confirmed
                && x.AppUserId == Guid.Parse(userId));

            if (!key.IsMissing())
            {
                query = query.Where(x => x.AddressFrom.Contains(key)
                || x.AddressTo.Contains(key)
                || x.TokenTransactionHash.Contains(key)
                || x.TokenTransactionHash.Contains(key));
            }

            var totalRow = query.Count();

            var transactions = await query.Skip((pageIndex - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            return new PagedResult<SavingDefi>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = transactions,
                RowCount = totalRow
            };
        }

        public async Task<PagedResult<DAppTransactionView>> GetTransactionsAsync(
            string keyword, int pageIndex, int pageSize)
        {

            var query = _savingDefiRepository.FindAll();

            if (!keyword.IsMissing())
            {
                query = query.Where(x => x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.TokenTransactionHash.Contains(keyword)
                || x.TokenTransactionHash.Contains(keyword));
            }

            var totalRow = query.Count();

            var transactions = await query.Skip((pageIndex - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            var data = transactions
                .Select(x => new DAppTransactionView
                {
                    Id = x.Id,
                    AddressTo = x.AddressTo,
                    AddressFrom = x.AddressFrom,
                    AppUserId = x.AppUserId,
                    TransactionState = x.TransactionState,
                    DAppTransactionStateName = x.TransactionState.GetDescription(),
                    BNBAmount = x.USDAmount,
                    TokenAmount = x.TokenAmount,
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated,
                    TokenTransactionHash = x.TokenTransactionHash,
                    IsDevice = x.IsDevice
                }).ToList();

            return new PagedResult<DAppTransactionView>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public string GetLatestTransactionByWalletAddress(Guid userId, string dappTxnHash)
        {
            var txnId = dappTxnHash.HexToUTF8String();

            var guidId = Guid.Parse(txnId);

            var query = _savingDefiRepository.FindAll(x => x.AppUserId == userId &&
            x.TransactionState == SavingDefiTransactionState.Requested && x.Id == guidId);

            query = query.OrderByDescending(x => x.DateCreated);

            var txn = query.FirstOrDefault();

            if (txn == null)
                return string.Empty;

            return txn.Id.ToString();
        }

        public PagedResult<SavingDefiViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize)
        {
            var query = _savingDefiRepository
                .FindAll(x => x.TransactionState == SavingDefiTransactionState.Confirmed);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TokenTransactionHash.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.USDAmount)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new SavingDefiViewModel()
                {
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    DateCreated = x.DateCreated,
                    TokenAmount = x.TokenAmount,
                    TransactionStateName = x.TransactionState.GetDescription(),
                    TransactionState = x.TransactionState,
                    USDAmount = x.USDAmount,
                    DateUpdated = x.DateUpdated,
                    TokenTransactionHash = x.TokenTransactionHash,
                    TokenConfigName = x.TokenConfig.Name,
                    Timeline = x.Timeline,
                    TokenConfigCode = x.TokenConfig.TokenCode,
                    TokenConfigImage = x.TokenConfig.TokenImageUrl,
                    Remarks = x.Remarks
                }).ToList();

            return new PagedResult<SavingDefiViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }


        public async Task<GenericResult> ProcessPaymentSaving(
            int tokenConfigId, int timeline, Guid appUserId, decimal tokenAmount)
        {
            var tokenConfig = _tokenConfigService.GetById(tokenConfigId);

            decimal priceToken = _blockChain.GetCurrentPrice(tokenConfig.TokenCode, "USD");

            if (priceToken == 0)
            {
                return new GenericResult(false,
                    "There is a problem loading the currency value!");
            }

            if (tokenAmount < tokenConfig.MinSaving)
            {
                return new GenericResult(false,
                    $"Min saving {tokenConfig.MinSaving} {tokenConfig.TokenCode}");
            }

            var interestRate = _tokenConfigService
                .GetInterestRate(tokenConfigId, (SavingTimeLine)timeline);

            var usdAmount = priceToken * tokenAmount;

            var ret = _walletService.WithdrawToRegularWallet(appUserId, tokenAmount, tokenConfigId);

            if (!ret)
            {
                return new GenericResult(false, $"Payment failed");
            }

            var txnHash = Guid.NewGuid().ToString("N");

            _walletTransactionService.Add(new WalletTransactionViewModel
            {
                AddressFrom = $"Wallet {tokenConfig.TokenCode}",
                AddressTo = $"System",
                Amount = tokenAmount,
                AmountReceive = tokenAmount,
                AppUserId = appUserId,
                DateCreated = DateTime.Now,
                TransactionHash = txnHash,
                Fee = 0,
                FeeAmount = 0,
                Type = WalletTransactionType.Saving,
                Unit = tokenConfig.TokenCode,
                Remarks = $"{appUserId} - {tokenAmount}"
            });

            _unitOfWork.Commit();

            var result = _savingService.AddSaving(
                new ProcessSavingModel
                {
                    AppUserId = appUserId,
                    Timeline = (SavingTimeLine)timeline,
                    AddressFrom = $"Wallet {tokenConfig.TokenCode}",
                    TokenConfigId = tokenConfigId,
                    Amount = tokenAmount,
                    AmountUSD = usdAmount,
                    InterestRate = interestRate,
                    ExpectedInterest = timeline * usdAmount * (interestRate / 100)
                });

            await _walletTransactionService.PaySavingAffiliateDirectAsync(appUserId, usdAmount);

            await _walletTransactionService.LeaderShip(appUserId, usdAmount);

            return new GenericResult(true, "Saving successfully");
        }
    }
}
