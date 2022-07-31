using Core.Application.Interfaces;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Dtos;
using Core.Web.Models.RequestParams;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Web.Api
{
    [Produces("application/json")]
    [Route("[Controller]/[action]")]
    [ApiController]

    public class SaleDefiController : Controller
    {
        private readonly IRepository<QueueTask, int> _queueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaleDefiController> _logger;
        private readonly ISaleDefiService _saleDefiService;
        private readonly ISaleDefiRepository _saleDefiRepository;
        private readonly UserManager<AppUser> _userManager;
        public const string DAppTransactionId = "DAppTransactionId";

        public string AccessAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(Request.Headers["connectedaddress"]))
                    return Request.Headers["connectedaddress"];

                return string.Empty;
            }
        }

        public SaleDefiController(
            IUnitOfWork unitOfWork,
            ILogger<SaleDefiController> logger,
            ISaleDefiService saleDefiService,
            UserManager<AppUser> userManager,
            IRepository<QueueTask, int> queueRepository,
            ISaleDefiRepository saleDefiRepository
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _saleDefiService = saleDefiService;
            _userManager = userManager;
            _queueRepository = queueRepository;
            _saleDefiRepository = saleDefiRepository;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> InitializeTransactionProgress(
            [FromBody] SaleInitializationParams model)
        {
            try
            {
                if (model.BNBAmount < 1)
                {
                    return BadRequest(new GenericResult(false, "Min buy 1 BNB"));
                }

                var user = await _userManager.FindByNameAsync(AccessAddress);

                model.AppUserId = user.Id.ToString();

                _logger.LogInformation("Start call InitializeTransactionProgress with param: {@0}", model);

                var res = await _saleDefiService.InitializeTransactionProgress(model, user.Id);

                if (!res.result.Success)
                {
                    return BadRequest(res.result);
                }

                _logger.LogInformation($"End call InitializeTransactionMetaMask");

                return Ok(res.result);
            }
            catch (Exception e)
            {
                _logger.LogError("InitializeTransactionProgress: {@0}", e);
                return BadRequest(new GenericResult(false, e.Message));
            }
        }

        /// <summary>
        /// VerifyTransactionRequest
        /// </summary>
        /// <param name = "transactionHash" ></ param >
        /// < returns ></ returns >
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTransactionRequest(
            [FromBody] DAppTransactionVerifyModel request)
        {
            var user = await _userManager.FindByNameAsync(AccessAddress);
            if (user == null)
            {
                return BadRequest(new GenericResult(false,
                    "Somethings went wrong! Please Contact administrator for support."));
            }

            var temptransactionId = _saleDefiService
                .GetLatestTransactionByWalletAddress(user.Id, request.DappTxnHash);

            if (string.IsNullOrEmpty(temptransactionId))
            {
                return BadRequest(new GenericResult(false,
                    "Somethings went wrong! Please Contact administrator for support."));
            }

            var dappTxn = _saleDefiRepository.FindById(Guid.Parse(temptransactionId));

            dappTxn.BNBTransactionHash = request.TransactionHash;

            _saleDefiRepository.Update(dappTxn);

            _unitOfWork.Commit();

            try
            {
                await _queueRepository.AddAsync(new QueueTask
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Admin",
                    Job = "VerifyTransaction",
                    Setting = JsonSerializer.Serialize(
                        new TransactionVerificationSetting
                        {
                            TempDAppTransactionId = temptransactionId,
                            TransactionHash = request.TransactionHash,
                            IsBNB = request.IsBNB,
                            IsSaving = false
                        }),
                    Status = QueueStatus.Pending
                });

                _unitOfWork.Commit();

                return Ok(new GenericResult(true, 
                    "We are processing your claim request, kindly wait few minutes and check your wallet."));
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    //var transactionId = HttpContext.Session.Get<string>(DAppTransactionId);

                    var metamaskTransaction = _saleDefiRepository.FindById(Guid.Parse(temptransactionId));

                    metamaskTransaction.Remarks = e.Message;
                    metamaskTransaction.BNBTransactionHash = request.TransactionHash;
                    metamaskTransaction.TransactionState = SaleDefiTransactionState.Failed;
                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return BadRequest(new GenericResult(false, 
                    "Somethings went wrong! Please Contact administrator for support."));
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult UpdateErrorMetaMask(MetaMaskErrorParams model)
        {

            _logger.LogInformation("UpdateErrorMetaMask: {@0}", JsonSerializer.Serialize(model));

            var convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

            var transactionId = convertor.ConvertFromHex(model.TransactionHex);

            _logger.LogInformation("UpdateErrorMetaMask: transactionId {@0}", transactionId);

            var result = _saleDefiRepository.FindById(Guid.Parse(transactionId));

            switch (model.ErrorCode)
            {
                case "4001":
                    result.TransactionState = SaleDefiTransactionState.Rejected;
                    break;

                case "-32603":
                    result.TransactionState = SaleDefiTransactionState.Failed;
                    break;

                default:
                    result.TransactionState = SaleDefiTransactionState.Failed;
                    break;
            }

            _unitOfWork.Commit();

            return Ok(new GenericResult(true, "Successed to update."));
        }
    }
}
