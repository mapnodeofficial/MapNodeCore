using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.DrinkToEarn;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    public class NFTMarketplaceController : Controller
    {
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ITokenConfigService _tokenConfigService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<WalletController> _logger;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly IDrinkToEarnService _drinkToEarnService;
        public NFTMarketplaceController(
            IWalletTransactionService walletTransactionService,
            IWalletService walletService,
            ILogger<WalletController> logger,
            UserManager<AppUser> userManager,
            IUserService userService,
            ITokenConfigService tokenConfigService,
            IDrinkToEarnService drinkToEarnService
            )
        {
            _drinkToEarnService = drinkToEarnService;
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
        public IActionResult GetNFTItems()
        {
            var model = new NftMarketplaceViewModel
            {
                CupItems = _drinkToEarnService.GetAllCupItems(),
                MachineItems = _drinkToEarnService.GetAllMachineItems(),
                ShopItems = _drinkToEarnService.GetAllShopItems()
            };

            return new OkObjectResult(model);
        }
    }
}
