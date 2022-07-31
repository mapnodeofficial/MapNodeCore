using Core.Application.Interfaces;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.Saving;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{

    public class SaleDefiService : BaseService, ISaleDefiService
    {
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaleDefiService> _logger;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly ISaleBlockService _saleBlockService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;
        private readonly ISaleDefiRepository _saleDefiRepository;
        private readonly ITokenConfigService _tokenConfigService;
        public const string DAppTransactionId = "DAppTransactionId";
        private IConfiguration _configuration;

        private static AsyncRetryPolicy _policy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(15)
              });

        public SaleDefiService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<SaleDefiService> logger,
            UserManager<AppUser> userManager,
            ISaleBlockService saleBlockService,
            IBlockChainService blockChainService,
            TelegramBotWrapper botTelegramService,
            ISaleDefiRepository saleDefiRepository,
            IWalletTransactionService walletTransactionService,
            IWalletService walletService,
            ITokenConfigService tokenConfigService
            )
            : base(userManager)
        {
            _logger = logger;
            _blockChainService = blockChainService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _saleBlockService = saleBlockService;
            _botTelegramService = botTelegramService;
            _saleDefiRepository = saleDefiRepository;
            _walletTransactionService = walletTransactionService;
            _walletService = walletService;
            _tokenConfigService = tokenConfigService;
        }

        public PagedResult<SaleDefiViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize)
        {
            var query = _saleDefiRepository
                .FindAll(x => x.TransactionState == SaleDefiTransactionState.Confirmed);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.BNBTransactionHash.Contains(keyword)
                || x.FBTransactionHash.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.BNBAmount)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new SaleDefiViewModel()
                {
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    DateCreated = x.DateCreated,
                    Type = x.Type,
                    BNBAmount = x.BNBAmount,
                    TokenAmount = x.TokenAmount,
                    FBTransactionHash = x.FBTransactionHash,
                    BNBTransactionHash = x.BNBTransactionHash,
                    TransactionStateName = x.TransactionState.GetDescription(),
                    TransactionState = x.TransactionState,
                    USDAmount = x.USDAmount,
                    DateUpdated = x.DateUpdated,
                    TypeName = x.Type.GetDescription(),
                    Remarks = x.Remarks
                }).ToList();

            return new PagedResult<SaleDefiViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public async Task<(GenericResult result, string transactionId)>
            InitializeTransactionProgress(SaleInitializationParams model, Guid userId)
        {
            decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");

            if (priceBNBBep20 == 0)
            {
                return (new GenericResult(false,
                    "There is a problem loading the currency value!"), string.Empty);
            }

            var transaction = new SaleDefi
            {
                AddressFrom = model.Address,
                AddressTo = CommonConstants.BEP20ReceivePuKey,
                DateCreated = DateTime.Now,
                TransactionState = SaleDefiTransactionState.Requested,
                IsDevice = model.IsDevice,
                Type = (SaleDefiTransactionType)model.TypeId,
                USDAmount = Math.Round(model.BNBAmount * priceBNBBep20, 2),
                AppUserId = userId,
                WalletType = model.WalletType,
                BNBAmount = model.BNBAmount,
                TokenAmount = ConculateTokenAmount(model.BNBAmount, priceBNBBep20)
            };

            await _saleDefiRepository.AddAsync(transaction);

            _unitOfWork.Commit();

            // luu wallet transaction gom txnhash ,

            var message = new BlockchainParams
            {
                TransactionHex = transaction.Id.ToString().ToHexUTF8(),
                From = model.Address,
                To = CommonConstants.BEP20ReceivePuKey,
                Value = transaction.BNBAmount,
                //Gas = "0x55f0",//22000
                //GasPrice = "0x2540be400"
            };

            return (GenericResult.ToSuccess("Successed to initialize Transaction", message), transaction.Id.ToString());
        }

        public async Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash, string tempDappTransaction, bool isRetry)
        {
            try
            {
                _logger.LogInformation(
                    $"Start calling VerifyMetaMaskRequest with transaction hash: {transactionHash}");

                var transactionReceipt = await _policy.ExecuteAsync(async () =>
                {
                    var result = await _blockChainService.GetTransactionReceiptByTransactionID(
                        transactionHash, CommonConstants.BEP20Url);

                    if (result == null)
                    {
                        _logger.LogInformation("retry get receipt of transaction hash: {0}", transactionHash);

                        throw new ArgumentNullException($"Cannot GetTransactionReceipt By {transactionHash}");
                    }

                    return result;
                });

                var transaction = await _blockChainService.GetTransactionByTransactionID(
                    transactionHash, CommonConstants.BEP20Url);

                var uft8Convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

                var transactionId = uft8Convertor.ConvertFromHex(transaction.Input);

                var saleDefi = _saleDefiRepository.FindById(Guid.Parse(transactionId));

                var balance = Web3.Convert.FromWei(transaction.Value);

                if (!transactionReceipt.Succeeded(true))
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //send chat bot
                var depositMessage = TelegramBotHelper
                    .BuildReportDepositSaleRoundMessage(
                    new DeFiMessageParam
                    {
                        Title = saleDefi.Type.GetDescription(),
                        AmountBNB = balance,
                        DepositAt = DateTime.Now,
                        AmountToken = saleDefi.TokenAmount,
                        UserWallet = saleDefi.AddressFrom,
                        SystemWallet = saleDefi.AddressTo,
                        Email = saleDefi.AddressFrom,
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);


                //compare dapp transaction with blockchain transaction
                if (!isRetry && saleDefi.TransactionState != SaleDefiTransactionState.Requested)
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {saleDefi.TransactionState}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {saleDefi.TransactionState}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare transaction with blockchain transaction
                if (saleDefi.AddressFrom.ToLower() != transaction.From.ToLower()
                    || saleDefi.AddressTo.ToLower() != transaction.To.ToLower())
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: Transaction's infor was not matched";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: Transaction's infor was not matched: ", transaction);

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare amount
                if (balance != saleDefi.BNBAmount)
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                saleDefi.TransactionState = SaleDefiTransactionState.Confirmed;
                saleDefi.BNBTransactionHash = transactionHash;
                saleDefi.DateUpdated = DateTime.Now;

                _unitOfWork.Commit();

                #region Transfer token
                var balanceTransfer = Math.Round(saleDefi.TokenAmount * 0.2m, 2);

                var tranansaction = await _blockChainService.SendERC20Async(
                    CommonConstants.BEP20TransferPrKey, saleDefi.AddressFrom,
                           CommonConstants.BEP20MNIContract, balanceTransfer,
                           CommonConstants.BEP20MNIDP, CommonConstants.BEP20Url);

                if (tranansaction.Succeeded(true))
                {
                    saleDefi.FBTransactionHash = tranansaction.TransactionHash;

                    _unitOfWork.Commit();

                    #region Add wallet txn deposit token

                    _walletTransactionService.Add(new WalletTransactionViewModel
                    {
                        AddressFrom = CommonConstants.BEP20TransferPuKey,
                        AddressTo = saleDefi.AddressFrom,
                        Amount = balanceTransfer,
                        AmountReceive = balanceTransfer,
                        AppUserId = saleDefi.AppUserId,
                        DateCreated = DateTime.Now,
                        TransactionHash = tranansaction.TransactionHash,
                        Fee = 0,
                        FeeAmount = 0,
                        Type = WalletTransactionType.BuySeedRound,
                        Unit = WalletTransactionUnit.MNI.GetDescription(),
                        Remarks = JsonConvert.SerializeObject(saleDefi)
                    });
                    _unitOfWork.Commit();

                    #endregion

                    #region Break Block

                    await _saleBlockService.LockBlockAsync(saleDefi.AppUserId, saleDefi.TokenAmount);

                    #endregion

                    #region Add Affiliate User

                    var result = await _walletTransactionService
                        .ProcessPaymentSaleAffiliate(saleDefi.AppUserId, saleDefi.TokenAmount);

                    if (!result.Success)
                    {
                        _logger.LogInformation($"ProcessPaymentAffiliate " +
                            $"{saleDefi.AppUserId} {saleDefi.TokenAmount}");
                    }

                    #endregion
                }
                #endregion


                _logger.LogInformation($"End call VerifyMetaMaskRequest with transaction hash:");

                return new GenericResult(true, $"Successed to buy {CommonConstants.TOKEN_IN_CODE}");
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var metamaskTransaction = _saleDefiRepository.FindById(Guid.Parse(tempDappTransaction));
                    metamaskTransaction.Remarks = e.Message;
                    metamaskTransaction.BNBTransactionHash = transactionHash;
                    metamaskTransaction.TransactionState = SaleDefiTransactionState.Failed;

                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return new GenericResult(false, e.Message);
            }
        }

        public decimal ConculateTokenAmount(decimal bnbAmount, decimal priceBNBBep20)
        {
            decimal priceToken = _configuration.GetValue<decimal>("TokenConfig:TokenPrice");

            var amountUSD = Math.Round(bnbAmount * priceBNBBep20, 2);

            return Math.Round(amountUSD / priceToken, 2);
        }

        public string GetLatestTransactionByWalletAddress(Guid userId, string dappTxnHash)
        {
            var txnId = dappTxnHash.HexToUTF8String();

            var guidId = Guid.Parse(txnId);

            var query = _saleDefiRepository.FindAll(x => x.AppUserId == userId
            && x.TransactionState == SaleDefiTransactionState.Requested
            && x.Id == guidId);

            query = query.OrderByDescending(x => x.DateCreated);

            var txn = query.FirstOrDefault();

            if (txn == null)
                return string.Empty;

            return txn.Id.ToString();
        }

        public async Task<GenericResult> ProcessBuyTokenByBNBsync(decimal bnbAmount, Guid appUserId)
        {
            var balance = _walletService.GetBNBBalance(appUserId);

            if (balance < bnbAmount)
            {
                return new GenericResult(false, "Balance not enough to buy token");
            }

            decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
            if (priceBNBBep20 == 0)
            {
                _logger.LogError("GetCurrentPrice BNB - USD = 0");
                return new GenericResult(false, "Payment failed.");
            }

            decimal tokenPrice = _configuration.GetValue<decimal>("TokenConfig:TokenPrice");

            var totalUSD = priceBNBBep20 * bnbAmount;

            var totalToken = Math.Round(totalUSD / tokenPrice, 4);

            var tokenUnlock = Math.Round(totalToken * 0.2m, 2);

            var isPaymented = _walletService.WithdrawToBNBWallet(appUserId, bnbAmount);

            if (!isPaymented)
            {
                return new GenericResult(false, "Payment failed");
            }

            var updateWalletMNI = _walletService
                .DepositToRegularWallet(appUserId, tokenUnlock, (int)TokenConfigEnum.MNI);

            if (!updateWalletMNI)
            {
                return new GenericResult(false, "Payment failed");
            }

            #region Add Transaction
            var txnHash = Guid.NewGuid().ToString();

            _walletTransactionService.Add(new WalletTransactionViewModel
            {
                AddressFrom = "Wallet BNB",
                AddressTo = "System",
                Amount = bnbAmount,
                AmountReceive = bnbAmount,
                AppUserId = appUserId,
                DateCreated = DateTime.Now,
                TransactionHash = txnHash,
                Fee = 0,
                FeeAmount = 0,
                Type = WalletTransactionType.BuySeedRound,
                Unit = WalletTransactionUnit.BNB.GetDescription(),
                Remarks = $"{appUserId} - {bnbAmount}"
            });

            _walletTransactionService.Add(new WalletTransactionViewModel
            {
                AddressFrom = "System",
                AddressTo = $"Wallet {CommonConstants.TOKEN_IN_CODE}",
                Amount = tokenUnlock,
                AmountReceive = tokenUnlock,
                AppUserId = appUserId,
                DateCreated = DateTime.Now,
                TransactionHash = txnHash,
                Fee = 0,
                FeeAmount = 0,
                Type = WalletTransactionType.BuySeedRound,
                Unit = WalletTransactionUnit.MNI.GetDescription(),
                Remarks = $"{appUserId} - {tokenUnlock}"
            });

            _unitOfWork.Commit();

            #endregion

            #region Break Block

            await _saleBlockService.LockBlockAsync(appUserId, totalToken);

            #endregion

            #region Add Affiliate User

            await _walletTransactionService
                .ProcessPaymentSaleAffiliate(appUserId, bnbAmount);

            #endregion


            return new GenericResult(true, "Payment token successfully");
        }

        public async Task<GenericResult> ProcessSwapTokenByMNOAsync(
            decimal mnoAmount, Guid appUserId, TokenConfigViewModel tokenConfig)
        {
            decimal tokenPrice = _configuration.GetValue<decimal>("TokenConfig:TokenPrice");

            var swapFeeAmount = mnoAmount * (tokenConfig.SwapFee / 100);

            var receiveMNO = mnoAmount - swapFeeAmount;

            var totalToken = Math.Round(receiveMNO / tokenPrice, 4);

            var tokenUnlock = Math.Round(totalToken * 0.2m, 2);

            var isPaymented = _walletService
                .WithdrawToRegularWallet(appUserId, mnoAmount, (int)TokenConfigEnum.MNO);

            if (!isPaymented)
            {
                return new GenericResult(false, "Payment failed");
            }

            var updateWalletMNI = _walletService
                .DepositToRegularWallet(appUserId, tokenUnlock, (int)TokenConfigEnum.MNI);

            if (!updateWalletMNI)
            {
                return new GenericResult(false, "Payment failed");
            }

            #region Add Transaction
            var txnHash = Guid.NewGuid().ToString();

            _walletTransactionService.Add(new WalletTransactionViewModel
            {
                AddressFrom = $"Wallet {WalletTransactionUnit.MNO.GetDescription()}",
                AddressTo = "System",
                Amount = mnoAmount,
                AmountReceive = receiveMNO,
                AppUserId = appUserId,
                DateCreated = DateTime.Now,
                TransactionHash = txnHash,
                Fee = tokenConfig.SwapFee,
                FeeAmount = swapFeeAmount,
                Type = WalletTransactionType.SwapMNOToMNI,
                Unit = WalletTransactionUnit.MNO.GetDescription(),
                Remarks = $"Swap {WalletTransactionUnit.MNO.GetDescription()} to {WalletTransactionUnit.MNI.GetDescription()}"
            });

            _walletTransactionService.Add(new WalletTransactionViewModel
            {
                AddressFrom = "System",
                AddressTo = $"Wallet {WalletTransactionUnit.MNI.GetDescription()}",
                Amount = tokenUnlock,
                AmountReceive = tokenUnlock,
                AppUserId = appUserId,
                DateCreated = DateTime.Now,
                TransactionHash = txnHash,
                Fee = 0,
                FeeAmount = 0,
                Type = WalletTransactionType.SwapMNOToMNI,
                Unit = WalletTransactionUnit.MNI.GetDescription(),
                Remarks = $"Swap {WalletTransactionUnit.MNO.GetDescription()} to {WalletTransactionUnit.MNI.GetDescription()}"
            });

            _unitOfWork.Commit();

            #endregion

            #region Break Block

            await _saleBlockService.LockBlockAsync(appUserId, totalToken);

            #endregion

            #region Add Affiliate User

            await _walletTransactionService
                .ProcessPaymentSwapAffiliate(appUserId, mnoAmount);

            #endregion


            return new GenericResult(true, "Payment token successfully");
        }
    }
}
