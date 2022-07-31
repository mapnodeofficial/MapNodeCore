using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ITokenConfigService _tokenConfigService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<WalletController> _logger;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        public TransactionController(
            IWalletTransactionService walletTransactionService,
            IWalletService walletService,
            ILogger<WalletController> logger,
            UserManager<AppUser> userManager,
            IUserService userService,
            ITokenConfigService tokenConfigService
            )
        {
            _walletTransactionService = walletTransactionService;
            _logger = logger;
            _userManager = userManager;
            _walletService = walletService;
            _userService = userService;
            _tokenConfigService = tokenConfigService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaging(string keyword, int page, int pageSize, string address)
        {
            //if (string.IsNullOrEmpty(address))
            //    return new OkObjectResult(new PagedResult<WalletTransactionViewModel>());

            var userInfo = await _userManager.FindByIdAsync(CurrentUserId);

            if (userInfo == null)
                return new OkObjectResult(new PagedResult<WalletTransactionViewModel>());

            var model = _walletTransactionService
                .GetAllPaging(keyword, userInfo.Id, page, pageSize);

            return new OkObjectResult(model);
        }

    }
}
