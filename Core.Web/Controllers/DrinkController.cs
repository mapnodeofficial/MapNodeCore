
using Core.Application.Interfaces;
using Core.Application.ViewModels.DrinkToEarn;
using Core.Application.ViewModels.GoogleApi;
using Core.Data.Entities;
using Core.Data.SpEntities;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    public class DrinkController : BaseController
    {
        private readonly ITokenConfigService _tokenConfigService;
        private readonly IBlockChainService _blockChainService;
        private readonly ISavingDefiService _savingDefiService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private IConfiguration _configuration;
        private readonly IGoogleMapService _googleMapService;
        private readonly IDrinkToEarnService _drinkToEarnService;
        private readonly ILogger<DrinkController> _logger;

        private readonly SignInManager<AppUser> _signInManager;
        public DrinkController(
           ITokenConfigService tokenConfigService,
           IBlockChainService blockChainService,
           ISavingDefiService savingDefiService,
           IConfiguration configuration,
           UserManager<AppUser> userManager,
           IUserService userService,
           SignInManager<AppUser> signInManager,
           IGoogleMapService googleMapService,
           IDrinkToEarnService drinkToEarnService,
           ILogger<DrinkController> logger)
        {
            _logger = logger;
            _drinkToEarnService = drinkToEarnService;
            _googleMapService = googleMapService;
            _userService = userService;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _tokenConfigService = tokenConfigService;
            _savingDefiService = savingDefiService;
            _configuration = configuration;
            _signInManager = signInManager;
        }


        public IActionResult Index()
        {

            int intervalHash = 200;

            if (_configuration.GetValue<int>("TokenConfig:IntervalHashRate") > 0)
                intervalHash = _configuration.GetValue<int>("TokenConfig:IntervalHashRate");

            ViewBag.IntervalHash = intervalHash;

            return View();
        }


        [HttpGet]
        public IActionResult GetStoreNearby(GoogleGeoModel model)
        {
           

            if (string.IsNullOrEmpty(model.lng) || string.IsNullOrEmpty(model.lat))
            {
                return new OkObjectResult(model);
            }
            //10.957136684016962, 106.86216176234595
#if DEBUG

            model.lat = "10.985346";
            model.lng = "106.863216";
#endif

            //await _googleMapService.SyncStoreAsync(model.lat, model.lng);

            var stores = _googleMapService.GetNeabyStoreByCurrentPosition(model.lat, model.lng, 0.05m);

            BindDefaultImage(stores);

            return new OkObjectResult(new PagedResult<GoogleMapGISNearby>()
            {
                Results = stores
            });
        }

        private void BindDefaultImage(List<GoogleMapGISNearby> stores)
        {
            foreach (var item in stores)
            {
                if (string.IsNullOrEmpty(item.ImgUrl))
                {
                    item.ImgUrl = "/images/large_shop_default.jpg";
                }
            }
        }

        private void BindDefaultImage(GoogleMapGISNearby store)
        {
            if (string.IsNullOrEmpty(store.ImgUrl))
            {
                store.ImgUrl = "/images/large_shop_default.jpg";
            }
        }

        public IActionResult Earning()
        {
            return View();
        }


        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult GetActiveStoreInformation(GoogleGeoModel model)
        {
           
            if (string.IsNullOrEmpty(model.lng) || string.IsNullOrEmpty(model.lat))
                return new OkObjectResult("Cant get user position");

#if DEBUG

            model.lat = "10.9691089";
            model.lng = "106.8628127";
#endif
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (appUserId.ToUpper() == "5E54029E-8FA2-46D9-2F78-08DA4E111FD8")
            //{
            //    model.lat = "10.9691089";
            //    model.lng = "106.8628127";
            //}

            _logger.LogInformation($"GetActiveStoreInformation - {User.Identity.Name} - {model.lat} - {model.lng}");

            var store = _googleMapService.GetActiveStoreByCurrentPosition(model.lat, model.lng);

            if (store != null)
            {
                _logger.LogInformation($"GetActiveStoreInformation - {User.Identity.Name} - {model.lat} - {model.lng} - found store {store.StoreName} - {store.Id}");
                BindDefaultImage(store);
                return new OkObjectResult(new
                {
                    IsSuccess = true,
                    Result = store,
                    Msg = ""
                });
            }

            _logger.LogInformation($"Store not found - {User.Identity.Name} - {model.lat} - {model.lng}");

            return new OkObjectResult(new
            {
                Msg = "Not found store"
            });
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult GetUserCups()
        {
           

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cups = _drinkToEarnService.GetAllUserCupPaging(Guid.Parse(appUserId));

            return new OkObjectResult(cups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwapHashrateToMNO(int id)
        {
            
            if (id == 0)
                return new OkObjectResult(new GenericResult(false, "parameters invalid"));

            

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            _logger.LogInformation($"SwapHashrateTo {CommonConstants.TOKEN_OUT_CODE} - User Id: {User.Identity.Name} - cupId {id}");

            var result = _drinkToEarnService.ProcessSwapHashToMNO(id, appUserId);

            return new OkObjectResult(result);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult StartDrink(StartDrinkRequestModel model)
        {

            if (model == null)
                return new OkObjectResult(new GenericResult { Success = false, Message = "user not valid" });

            
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (appUserId.ToUpper() == "5E54029E-8FA2-46D9-2F78-08DA4E111FD8")
            //{
            //    model.Lat = "10.9691089";
            //    model.Lng = "106.8628127";
            //}

#if DEBUG

            model.Lat = "10.9691089";
            model.Lng = "106.8628127";
#endif

            var store = _googleMapService.GetActiveStoreByCurrentPosition(model.Lat, model.Lng);

            if (store == null)
                return new OkObjectResult(new GenericResult { Success = false, Message = "store not valid" });


            _logger.LogInformation($"StartDrink - User Id: {User.Identity.Name} - cupId {model.Id} - store id {store.Id} - lat {model.Lat} - lng {model.Lng}");

            var result = _drinkToEarnService.ProcessNewDrink(model.Id, store.Id, Guid.Parse(appUserId), model.Lat, model.Lng);

            return new OkObjectResult(result);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult StopDrink(int Id, string lat, string lng)
        {
            var stopDate = DateTime.Now;

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation($"StopDrink - User Id: {User.Identity.Name} - cupId {Id} - lat {lat} - lng {lng}");

            var result = _drinkToEarnService.ProcessStopDrink(Id, Guid.Parse(appUserId), lat, lng, stopDate);

            return new OkObjectResult(result);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult SyncStatus(string lat, string lng)
        {
            
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
                return new OkObjectResult(true);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation($"SyncStatus - userId: {User.Identity.Name} - lat {lat} - lng {lng}");

#if DEBUG

            lat = "10.9691089";
            lng = "106.8628127";
#endif


            _drinkToEarnService.SyncIsAtStoreStatus(Guid.Parse(userId), lat, lng);

            return new OkObjectResult(true);
        }

        [HttpGet]
        public IActionResult GetAllDrinkHistoryPaging(string keyword, int page, int pageSize)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = _drinkToEarnService.GetAllDrinkHistoryPaging(
                Guid.Parse(appUserId),
                keyword,
                page,
                pageSize);

            return new OkObjectResult(model);
        }
    }
}
