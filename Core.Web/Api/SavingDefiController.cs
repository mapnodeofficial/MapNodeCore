using BeCoreApp.Web.Configuration;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Web.Models.RequestParams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Web.Api
{
    [Produces("application/json")]
    [Route("[Controller]/[action]")]
    [ApiController]

    public class SavingDefiController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISavingDefiRepository _savingDefiRepository;
        private readonly IQueueTaskRepository _queueTaskRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SavingDefiController> _logger;
        private readonly ISavingDefiService _savingDefiService;
        private readonly IDrinkToEarnService _drinkToEarnService;
        private readonly UserManager<AppUser> _userManager;
        public const string DAppTransactionId = "DAppTransactionId";
        private readonly RoleManager<AppRole> _roleManager;
        private static object _blockCreateUser = new();
        private static Dictionary<string, DateTime> _requestedUsers;
        private readonly SignInManager<AppUser> _signInManager;
        static Dictionary<string, DateTime> RequestedUsers
        {
            get
            {
                if (_requestedUsers == null)
                {
                    lock (_blockCreateUser)
                    {
                        _requestedUsers = new Dictionary<string, DateTime>();
                    }
                }

                return _requestedUsers;
            }
        }

        public string AccessAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(Request.Headers["connectedaddress"]))
                    return Request.Headers["connectedaddress"];

                return string.Empty;
            }
        }

        public SavingDefiController(
            ISavingDefiRepository savingDefiRepository,
            IUnitOfWork unitOfWork,
            ILogger<SavingDefiController> logger,
            ISavingDefiService savingDefiService,
            UserManager<AppUser> userManager,
            IQueueTaskRepository queueTaskRepository,
            RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager,
            IDrinkToEarnService drinkToEarnService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _drinkToEarnService = drinkToEarnService;
            _savingDefiRepository = savingDefiRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _savingDefiService = savingDefiService;
            _userManager = userManager;
            _queueTaskRepository = queueTaskRepository;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> InitializeTransactionProgress(
            [FromBody] DappInitializationParams model)
        {
            try
            {

                var user = await _userManager.FindByNameAsync(AccessAddress);

                model.AppUserId = user.Id.ToString();

                _logger.LogInformation("Start call InitializeTransactionProgress with param: {@0}", model);

                var res = await _savingDefiService.InitializeTransactionProgress(model, user.Id);

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
        /// <param name="transactionHash"></param>
        /// <returns></returns>
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

            var temptransactionId = _savingDefiService
                .GetLatestTransactionByWalletAddress(user.Id, request.DappTxnHash);

            if (string.IsNullOrEmpty(temptransactionId))
            {
                return BadRequest(new GenericResult(false,
                    "Somethings went wrong! Please Contact administrator for support."));
            }

            var dappTxn = _savingDefiRepository.FindById(Guid.Parse(temptransactionId));

            dappTxn.TokenTransactionHash = request.TransactionHash;

            _savingDefiRepository.Update(dappTxn);

            _unitOfWork.Commit();

            try
            {
                await _queueTaskRepository.AddAsync(new QueueTask
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Admin",
                    Job = "VerifyTransaction",
                    Setting = JsonSerializer.Serialize(new TransactionVerificationSetting
                    {
                        TempDAppTransactionId = temptransactionId,
                        TransactionHash = request.TransactionHash,
                        IsBNB = request.IsBNB,
                        IsSaving = true
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

                    var metamaskTransaction = _savingDefiRepository.FindById(Guid.Parse(temptransactionId));

                    metamaskTransaction.Remarks = e.Message;
                    metamaskTransaction.TokenTransactionHash = request.TransactionHash;
                    metamaskTransaction.TransactionState = SavingDefiTransactionState.Failed;

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

            var result = _savingDefiRepository.FindById(Guid.Parse(transactionId));

            switch (model.ErrorCode)
            {
                case "4001":
                    result.TransactionState = SavingDefiTransactionState.Rejected;
                    break;

                case "-32603":
                    result.TransactionState = SavingDefiTransactionState.Failed;
                    break;

                default:
                    result.TransactionState = SavingDefiTransactionState.Failed;
                    break;
            }

            _unitOfWork.Commit();
            return Ok(new GenericResult(true, "Successed to update."));
        }

        //[HttpGet("{amount}")]
        //public async Task<IActionResult> GetAmountICDPerBNB(decimal amount)
        //{
        //    try
        //    {
        //        decimal priceBNBBep20 = _blockChain.GetCurrentPrice("BNB", "USD");
        //        if (priceBNBBep20 == 0)
        //            return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

        //        decimal amountICD = _savingDefiService.ConculateTokenAmount(amount, priceBNBBep20);

        //        return Ok(new GenericResult(true, amountICD));
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(new GenericResult(false, "Invalid amount"));
        //    }
        //}

        //[HttpGet]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DAppConnect([FromQuery] string address, [FromQuery] string referral)
        //{
        //    try
        //    {
        //        if (Request.Cookies[ConfigurationConsts.ReferralCookie] != null)
        //        {
        //            _logger.LogInformation($"Process Cookie {ConfigurationConsts.ReferralCookie}");
        //            referral = Request.Cookies[ConfigurationConsts.ReferralCookie];
        //        }

        //        _logger.LogInformation($"DAppConnect {address}-{referral}");

        //        //return BadRequest(new GenericResult(false, "Something went wrong!"));
        //        AddressUtil addressUtil = new();
        //        var convertedAddress = addressUtil.ConvertToChecksumAddress(address);

        //        if (!addressUtil.IsChecksumAddress(convertedAddress) || !addressUtil.IsValidAddressLength(convertedAddress))
        //            return new OkObjectResult(new GenericResult(false, "Address not in standard format BEP20."));

        //        address = address.ToLower();

        //        lock (_blockCreateUser)
        //        {
        //            if (!RequestedUsers.ContainsKey(address))
        //                RequestedUsers.Add(address, DateTime.Now);
        //            else
        //                return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = referral }));
        //        }

        //        var user = await _userManager.FindByNameAsync(address);

        //        if (user != null)
        //        {
        //            _drinkToEarnService.SyncFreeCup(user.Id);

        //            //if (User.Identity.IsAuthenticated)
        //            //{
        //            //    lock (_blockCreateUser)
        //            //    {
        //            //        if (RequestedUsers.ContainsKey(address))
        //            //            RequestedUsers.Remove(address);
        //            //    }

        //            //    if (IsAlreadyAuthenDrink())
        //            //        return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = user.Sponsor }));

        //            //    if (User.Identity.Name.Equals(address))
        //            //        return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = user.Sponsor }));
        //            //}

        //            var userRoles = await _userManager.GetRolesAsync(user);

        //            if (userRoles.Count == 0)
        //                await _userManager.AddToRoleAsync(user, "Customer");

                    
        //            lock (_blockCreateUser)
        //            {
        //                if (RequestedUsers.ContainsKey(address))
        //                    RequestedUsers.Remove(address);
        //            }

        //            return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = user.Sponsor }));
        //        }



        //        //if (referral.Trim().ToLower() == address.Trim().ToLower())
        //        //{
        //        //    var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.IsSystem);
        //        //    referral = admin.UserName;
        //        //}

        //        var refId = Guid.Parse("111cae9f-f7ce-46eb-0615-08d6c5ba1111");
        //        if (!string.IsNullOrEmpty(referral))
        //        {
        //            _logger.LogInformation($"DAppConnect {address}-{referral} - Referral not empty ");
        //            //var isParsed = int.TryParse(referral, out int refInt);
        //            //if (isParsed)
        //            //{
        //            //    _logger.LogInformation($"DAppConnect {address}-{referral} - Referral parse success {refInt}");

        //            //    var userSponsor = _userManager.Users.FirstOrDefault(x => x.Sponsor == refInt);

        //            //    if (userSponsor != null)
        //            //        refId = userSponsor.Id;
        //            //    else
        //            //        _logger.LogInformation($"DAppConnect {address}-{referral} - Referral null user");
        //            //}
        //        }
        //        int newSponsor = _userManager.Users.Max(x => x.Sponsor ?? 0);

        //        var appUser = new AppUser
        //        {
        //            UserName = address,
        //            DateCreated = DateTime.Now,
        //            IsSystem = false,
        //            ReferralId = refId,
        //            Sponsor = ++newSponsor,
        //            Status = Status.Active,
        //            DateModified = DateTime.Now,
        //            EmailConfirmed = true,
        //            Email = address + "@gmail.com"

        //        };

        //        var result = await _userManager.CreateAsync(appUser);

        //        if (result.Succeeded)
        //        {
        //            await _userManager.UpdateSecurityStampAsync(appUser);

        //            _drinkToEarnService.SyncFreeCup(appUser.Id);

        //            var appRole = await _roleManager.FindByNameAsync("Customer");
        //            if (appRole == null)
        //            {
        //                await _roleManager.CreateAsync(new AppRole
        //                {
        //                    Name = "Customer",
        //                    NormalizedName = "Customer",
        //                    Description = "Customer is role use for member"
        //                });

        //            }

        //            result = await _userManager.AddToRoleAsync(appUser, "Customer");
                    
        //            lock (_blockCreateUser)
        //            {
        //                if (RequestedUsers.ContainsKey(address))
        //                    RequestedUsers.Remove(address);
        //            }
        //            return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = appUser.Sponsor }));
        //        }
        //        lock (_blockCreateUser)
        //        {
        //            if (RequestedUsers.ContainsKey(address))
        //                RequestedUsers.Remove(address);
        //        }
        //        return BadRequest(new GenericResult(false, "Something went wrong!"));


        //    }
        //    catch (Exception e)
        //    {
        //        lock (_blockCreateUser)
        //        {
        //            if (RequestedUsers.ContainsKey(address))
        //                RequestedUsers.Remove(address);
        //        }

        //        _logger.LogError($"DAppConnect Exception {address}-{referral} - {e.Message}");

        //        _logger.LogError($"DAppConnect Exception {address}-{referral} - {e.StackTrace}");

        //        return BadRequest(new GenericResult(false, "Something went wrong!"));
        //    }
        //}

        bool IsAlreadyAuthenDrink()
        {
            if (!User.Identity.IsAuthenticated)
                return false;


            var userName = User.FindFirstValue("DrinkToEarn");

            if (string.IsNullOrEmpty(userName))
                return false;

            return userName.Equals(User.Identity.Name);
        }

        [HttpGet]
        public async Task<IActionResult> DAppDisconnect()
        {
            try
            {
                //if (User.Identity.IsAuthenticated)
                //{
                //    var isonDrink = IsAlreadyAuthenDrink();
                //    if (!isonDrink)
                //    {
                //        await _signInManager.SignOutAsync();
                //    }
                //}

            }
            catch (Exception e)
            {

            }
            return Ok(new GenericResult(true, "success signout"));
        }

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> GetTransactions([FromQuery] string key, string type, [FromQuery] int page, [FromQuery] int pageSize)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                var transactions = await _savingDefiService.GetTransactionsAsync(key, page, pageSize, type, userId);

                return Ok(transactions);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
