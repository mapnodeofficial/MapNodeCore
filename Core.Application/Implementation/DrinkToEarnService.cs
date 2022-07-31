using Core.Application.Interfaces;
using Core.Application.ViewModels.DrinkToEarn;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class DrinkToEarnService : IDrinkToEarnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IConfiguration _configuration;
        private readonly ILogger<DrinkToEarnService> _logger;
        private readonly IGoogleMapService _googleMapService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICupItemsRepository _cupItemsRepository;
        private readonly IMachineItemsRepository _machineItemsRepository;
        private readonly IGoogleMapGISRepository _googleMapGISRepository;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IDrinkAccessCodeRepository _drinkAccessCodeRepository;
        private readonly IDrinkToEarnHistoriesRepository _drinkToEarnHistoriesRepository;
        private readonly IAppUsersCupItemHistoriesRepository _appUsersCupItemHistoriesRepository;
        private readonly IShopItemsRepository _shopItemsRepository;
        private readonly IWalletService _walletService;

        public DrinkToEarnService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<DrinkToEarnService> logger,
            UserManager<AppUser> userManager,
            IGoogleMapService googleMapService,
            ICupItemsRepository cupItemsRepository,
            IMachineItemsRepository machineItemsRepository,
            IGoogleMapGISRepository googleMapGISRepository,
            IWalletTransactionService walletTransactionService,
            IDrinkAccessCodeRepository drinkAccessCodeRepository,
            IDrinkToEarnHistoriesRepository drinkToEarnHistoriesRepository,
            IAppUsersCupItemHistoriesRepository appUsersCupItemHistoriesRepository,
            IWalletService walletService,
            IShopItemsRepository shopItemsRepository
            )
        {
            _shopItemsRepository = shopItemsRepository;
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _googleMapService = googleMapService;
            _cupItemsRepository = cupItemsRepository;
            _machineItemsRepository = machineItemsRepository;
            _googleMapGISRepository = googleMapGISRepository;
            _walletTransactionService = walletTransactionService;
            _appUsersCupItemHistoriesRepository = appUsersCupItemHistoriesRepository;
            _drinkToEarnHistoriesRepository = drinkToEarnHistoriesRepository;
            _drinkAccessCodeRepository = drinkAccessCodeRepository;
            _walletService = walletService;
        }

        public void SyncFreeCup(Guid appUserId)
        {
            var freeCup = _cupItemsRepository.FindAll(x => x.IsFree).FirstOrDefault();

            if (freeCup == null)
                return;

            var isAnyFreeCup = _appUsersCupItemHistoriesRepository.FindAll(x => x.AppUserId == appUserId && x.CupItemId == freeCup.Id).Any();

            if (isAnyFreeCup)
                return;

            _appUsersCupItemHistoriesRepository.Add(new AppUsersCupItemHistories
            {
                AppUserId = appUserId,
                CupItemId = freeCup.Id,
                TimeToUse = freeCup.TimeToUse,
                HashRate = freeCup.HashRate,
                MaxOut = freeCup.MaxOut,
                Price = 0,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                ItemStatus = CupItemStatus.Active,
                RemainTime = freeCup.TimeToUse,
                CurrentHashEarn = 0
            });

            _unitOfWork.Commit();

        }

        public PagedResult<CupItemViewModel> GetAllUserCupPaging(Guid userId, int pageIndex = 1, int pageSize = 20)
        {

            var query = _appUsersCupItemHistoriesRepository.FindAll();

            query = query.Where(x => x.AppUserId == userId);

            var totalRow = query.Count();

            var data = query.OrderBy(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable()
                .ToList();

            var responseData = data.Select((x => PrepareUsersCupItemModel(x))).ToList();

            //if (responseData.Any(d => d.IsInDrink))
                //_unitOfWork.Commit();

            return new PagedResult<CupItemViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = responseData,
                RowCount = totalRow
            };
        }

        CupItemViewModel PrepareUsersCupItemModel(
            AppUsersCupItemHistories cupItemHistory)
        {
            var cupInfo = _cupItemsRepository.FindById(cupItemHistory.CupItemId);

            var model = new CupItemViewModel()
            {
                Id = cupItemHistory.Id,
                Code = cupInfo.Code,
                TimeToUse = cupItemHistory.TimeToUse,
                HashRate = cupItemHistory.HashRate,
                ImageUrl = cupInfo.ImageUrl,
                MaxOut = cupItemHistory.MaxOut,
                Name = cupInfo.Name,
                Price = cupItemHistory.Price,
                IsInDrink = _drinkToEarnHistoriesRepository.IsOnDrink(cupItemHistory.Id),
                CurrentHashRate = cupItemHistory.CurrentHashEarn
            };

            if (model.IsInDrink)
            {
                var currentDate = DateTime.Now;

                var drinkItem = _drinkToEarnHistoriesRepository.GetLatestActiveDrink(cupItemHistory.Id);

                if (drinkItem != null)
                {
                    //drinkItem.EarnResult = CalculateHashEarn(currentDate, drinkItem.LastPingTime, drinkItem.HashRate);

                    //model.CurrentHashRate += drinkItem.EarnResult ?? 0;

                    //UpdateCupHistoryHashRateCurrent(drinkItem.CupItemHistoryId,
                    //    drinkItem.AppUserId,
                    //    currentDate,
                    //    drinkItem.LastPingTime,
                    //    drinkItem.EarnResult ?? 0);

                    //drinkItem.LastPingTime = currentDate;

                    //_drinkToEarnHistoriesRepository.Update(drinkItem);

                }
            }

            return model;
        }

        public GenericResult ProcessNewDrink(int cupItemHistoryId, long storeId, Guid appUserId, string lat, string lng)
        {
            var isOnDrink = _drinkToEarnHistoriesRepository.IsOnDrink(cupItemHistoryId);

            if (isOnDrink)
                return new GenericResult { Success = false, Message = "This cup is on using now" };

            var storeInfo = _googleMapGISRepository.FindById(storeId);

            var cupInfo = _appUsersCupItemHistoriesRepository.FindById(cupItemHistoryId);

            if (cupInfo.TimeToUse <= 0)
                return new GenericResult { Success = false, Message = "This cup is out of time to use" };

            var current = DateTime.Now;

            _drinkToEarnHistoriesRepository.Add(new DrinkToEarnHistories
            {
                AppUserId = appUserId,
                CupItemId = cupInfo.CupItemId,
                StoreId = storeId,
                DateCreated = current,
                DateModified = current,
                HashRate = cupInfo.HashRate,
                LastPingTime = current,
                StorePlaceId = storeInfo.StoreId,
                UserLat = lat,
                UserLng = lng,
                StatusId = DrinkHistoryStatus.Active,
                CupItemHistoryId = cupItemHistoryId,
                EarnResult = 0

            });

            _unitOfWork.Commit();

            return new GenericResult { Success = true };
        }

        public GenericResult ProcessSwapHashToMNO(int cupItemHistoryId, string appUserId)
        {
            _logger.LogInformation($"ProcessSwapHashTo{CommonConstants.TOKEN_OUT_CODE} - Cup Item History {cupItemHistoryId} - App User ID {appUserId}");
            var isOnDrink = _drinkToEarnHistoriesRepository.IsOnDrink(cupItemHistoryId);

            if (isOnDrink)
                return new GenericResult(false, $"Please, stop drinking on this cup before swap to {CommonConstants.TOKEN_OUT_CODE}");

            var cupInfo = _appUsersCupItemHistoriesRepository.FindById(cupItemHistoryId);

            var hashrateSwapAmount = cupInfo.CurrentHashEarn;

            var minSwap = CommonConstants.SWAP_EXCHANGE_RATE * 10;

            if (hashrateSwapAmount < minSwap)
                return new GenericResult(false, $"Min to swap to {CommonConstants.TOKEN_OUT_CODE} is  {minSwap} hash");

            decimal receiveAmount = Math.Round(hashrateSwapAmount / CommonConstants.SWAP_EXCHANGE_RATE, 2);

            _logger.LogInformation($"ProcessSwapHashTo {CommonConstants.TOKEN_OUT_CODE} - Cup Item History {cupItemHistoryId} " +
                $"- App User ID {appUserId} - Current Hash {hashrateSwapAmount}" +
                $"- {CommonConstants.TOKEN_OUT_CODE} Receive {receiveAmount}");

            var res =  _walletService.DepositToRegularWallet(Guid.Parse(appUserId), receiveAmount, (int)TokenConfigEnum.MNO);

            if (res)
            {
                _walletTransactionService
                    .AddTransaction(Guid.Parse(appUserId), receiveAmount, receiveAmount,
                    WalletTransactionType.SwapHashToMNO, "System", $"Wallet {CommonConstants.TOKEN_OUT_CODE}", 
                    WalletTransactionUnit.MNO.GetDescription(), 0, 0);

                cupInfo.CurrentHashEarn = 0;
                _appUsersCupItemHistoriesRepository.Update(cupInfo);

                _unitOfWork.Commit();
            }
            else
                return new GenericResult(false,$"Payment {CommonConstants.TOKEN_OUT_CODE} failed");

            return new GenericResult(true, $"Swap {hashrateSwapAmount} hash to {receiveAmount} {CommonConstants.TOKEN_OUT_CODE} is success");
        }


        public GenericResult ProcessStopDrink(int cupItemHistoryId, Guid appUserId, string lat, string lng, DateTime stopDate)
        {
            var isOnDrink = _drinkToEarnHistoriesRepository.IsOnDrink(cupItemHistoryId);

            if (!isOnDrink)
                return new GenericResult { Success = false, Message = "This cup is not on drink" };

            var historyInfo = _drinkToEarnHistoriesRepository.GetLatestActiveDrink(cupItemHistoryId);

            var currentLastPing = historyInfo.LastPingTime;

            historyInfo.StatusId = DrinkHistoryStatus.Inactive;
            historyInfo.DateModified = stopDate;
            historyInfo.LeavedOn = stopDate;
            historyInfo.LastPingTime = stopDate;
            historyInfo.LeaveLat = lat;
            historyInfo.LeaveLng = lng;
            historyInfo.Remarks = "User manual stop drink";

            var earn = CalculateHashEarn(stopDate, currentLastPing, historyInfo.HashRate);
            historyInfo.EarnResult += earn;

            _drinkToEarnHistoriesRepository.Update(historyInfo);

            _unitOfWork.Commit();

            UpdateCupHistoryHashRateCurrent(cupItemHistoryId,
                appUserId,
                stopDate,
                currentLastPing,
                earn);

            return new GenericResult { Success = true };
        }

        void UpdateCupHistoryHashRateCurrent(int cupItemHistoryId,
            Guid appUserId,
            DateTime currentDate,
            DateTime lastPingTime,
            int earnHashrate)
        {
            var currentCup = _appUsersCupItemHistoriesRepository.GetByUser(cupItemHistoryId, appUserId);

            if (currentCup == null) return;

            currentCup.CurrentHashEarn += earnHashrate;

            currentCup.RemainTime -= Convert.ToDecimal((currentDate - lastPingTime).TotalHours, CultureInfo.InvariantCulture);

            _appUsersCupItemHistoriesRepository.Update(currentCup);

            _unitOfWork.Commit();
        }



        public List<DrinkToEarnHistories> GetUserActiveDrinkHistories(Guid appUserId)
        {
            var query = _drinkToEarnHistoriesRepository.FindAll(x => x.LeavedOn == null
               && x.AppUserId == appUserId && x.StatusId == DrinkHistoryStatus.Active);

            return query.ToList();
        }

        public List<DrinkToEarnHistories> GetUserActiveDrinkHistories()
        {
            var query = _drinkToEarnHistoriesRepository.FindAll(x => x.LeavedOn == null && x.StatusId == DrinkHistoryStatus.Active);

            return query.ToList();
        }

        public void SyncIsAtStoreStatus(Guid appUserId, string lat, string lng)
        {
            _logger.LogInformation($"SyncIsAtStoreStatus - UserId {appUserId} - {lat} - {lng}");
            var activeDrinks = GetUserActiveDrinkHistories(appUserId);

            if (activeDrinks.Count == 0)
                return;

            foreach (var drinkItem in activeDrinks)
            {
                var storeInfo = _googleMapGISRepository.FindById(drinkItem.StoreId);

                var currentDate = DateTime.Now;
                if (!_googleMapService.IsInsideStore(double.Parse(storeInfo.Lat, CultureInfo.InvariantCulture),
                    double.Parse(storeInfo.Lng, CultureInfo.InvariantCulture),
                    double.Parse(lat, CultureInfo.InvariantCulture), double.Parse(lng, CultureInfo.InvariantCulture)))
                {
                    _logger.LogInformation($"SyncIsAtStoreStatus - UserId {appUserId} - {lat} " +
                        $"- {lng} - Not inside store {storeInfo.Id} - {storeInfo.Lat} " +
                        $"- {storeInfo.Lng}" +
                        $" Distance - {_googleMapService.GetDistanceOf(double.Parse(storeInfo.Lat, CultureInfo.InvariantCulture), double.Parse(storeInfo.Lng, CultureInfo.InvariantCulture), double.Parse(lat, CultureInfo.InvariantCulture), double.Parse(lng, CultureInfo.InvariantCulture))}");

                    drinkItem.StatusId = DrinkHistoryStatus.Inactive;

                    drinkItem.LeavedOn = currentDate;
                    drinkItem.LeaveLat = lat;
                    drinkItem.LeaveLng = lng;
                    drinkItem.Remarks = "User stop by leaving the store";
                }

                
                var earn = CalculateHashEarn(currentDate, drinkItem.LastPingTime, drinkItem.HashRate);

                drinkItem.EarnResult += earn;


                UpdateCupHistoryHashRateCurrent(drinkItem.CupItemHistoryId,
                appUserId,
                currentDate,
                drinkItem.LastPingTime,
                earn);

                drinkItem.LastPingTime = currentDate;
                drinkItem.DateModified = currentDate;

                _drinkToEarnHistoriesRepository.Update(drinkItem);
                _unitOfWork.Commit();
            }
        }

        public void SyncDrinkAtStoreStatus()
        {
            var activeDrinks = GetUserActiveDrinkHistories();

            if (activeDrinks.Count == 0)
                return;

            var currentDate = DateTime.Now;

            foreach (var drinkItem in activeDrinks)
            {

                _logger.LogInformation($"SyncDrinkAtStoreStatus - UserId - {drinkItem.AppUserId} " +
                    $"- Total Minutes {(currentDate - drinkItem.LastPingTime).TotalMinutes}" +
                    $"- Cup ID : {drinkItem.CupItemHistoryId}" +
                    $"- Store ID : {drinkItem.StoreId}");
                if ((currentDate -  drinkItem.LastPingTime).TotalMinutes > 10 )
                {

                    drinkItem.LeavedOn = currentDate;
                    drinkItem.Remarks = $"Leave by larger {(DateTime.Now - drinkItem.LastPingTime).TotalMinutes} minutes not checking in store";
                    drinkItem.StatusId = DrinkHistoryStatus.Inactive;

                    var earn = CalculateHashEarn(currentDate, drinkItem.LastPingTime, drinkItem.HashRate);

                    drinkItem.EarnResult += earn;

                UpdateCupHistoryHashRateCurrent(drinkItem.CupItemHistoryId,
                drinkItem.AppUserId,
                currentDate,
                drinkItem.LastPingTime,
                earn);


                drinkItem.LastPingTime = currentDate;

                    _drinkToEarnHistoriesRepository.Update(drinkItem);

                    _unitOfWork.Commit();
                }

            }
        }

        #region Drink Access Token

        public string GenerateDrinkTokenByUser(Guid userId)
        {
            var query = _drinkAccessCodeRepository.FindAll(x => !x.IsDeleted);


            string accessCode = StringExtention.RandomString(8);

            while (query.Any(d => d.TokenCode.Equals(accessCode)))
            {

                accessCode = StringExtention.RandomString(8);
            }

            var currentUsersCodes = _drinkAccessCodeRepository.FindAll(x => !x.IsDeleted
                && x.AppUserId == userId).ToList();

            foreach (var currentCode in currentUsersCodes)
            {
                currentCode.IsDeleted = true;
                currentCode.ModifiedOn = DateTime.Now;
                _drinkAccessCodeRepository.Update(currentCode);
            }

            _drinkAccessCodeRepository.Add(new DrinkAccessCode
            {
                AppUserId = userId,
                TokenCode = accessCode,
                CreatedOn = DateTime.Now,
                IsDeleted = false,
                ModifiedOn = null,

            });

            _unitOfWork.Commit();


            return accessCode;

        }

        public Guid GetUserIdByDrinkToken(string code)
        {
            var tokenInfo = _drinkAccessCodeRepository.FindAll(x => !x.IsDeleted
                && x.TokenCode == code.ToUpper()).FirstOrDefault();

            if (tokenInfo == null)
                return Guid.Empty;

            return tokenInfo.AppUserId;
        }

        public void DeleteUserDrinkCode(Guid userId)
        {
            var currentUsersCodes = _drinkAccessCodeRepository.FindAll(x => !x.IsDeleted
                && x.AppUserId == userId).ToList();

            foreach (var currentCode in currentUsersCodes)
            {
                currentCode.IsDeleted = true;
                currentCode.ModifiedOn = DateTime.Now;
                _drinkAccessCodeRepository.Update(currentCode);
            }

            _unitOfWork.Commit();
        }

        #endregion


        public decimal CalculateTokenBetweenTimes(DateTime from, DateTime to, decimal hashrate)
        {
            var milisecs = (to - from).TotalMilliseconds;

            var totalMint = milisecs * (double)hashrate;

            var earnAmount = 3600000 / totalMint;

            return Convert.ToDecimal(earnAmount);
        }

        public int CalculateHashEarn(DateTime end, DateTime start, decimal hashRate)
        {
            var currentHash = Math.Abs((end - start).TotalSeconds) * (double)hashRate;

            return Convert.ToInt32(currentHash);
        }

        public PagedResult<DrinkHistoryViewModel> GetAllDrinkHistoryPaging(
            Guid userId, string keyword = "", int pageIndex = 1, int pageSize = 20)
        {
            var query = _drinkToEarnHistoriesRepository
                .FindAll(x => x.AppUserId == userId);

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => PrepareDrinkHistoryModel(x))
                .ToList();

            return new PagedResult<DrinkHistoryViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        DrinkHistoryViewModel PrepareDrinkHistoryModel(DrinkToEarnHistories entity)
        {
            var storeInfo = _googleMapGISRepository.FindById(entity.StoreId);
            var cupInfo = _cupItemsRepository.FindById(entity.CupItemId);
            return new DrinkHistoryViewModel
            {
                CupItemId = entity.CupItemId,
                LeaveOn = entity.LeavedOn,
                CupName = cupInfo.Name,
                StoreName = storeInfo.StoreName,
                HashRate = entity.HashRate,
                StartOn = entity.DateCreated,
                HashEarn = entity.EarnResult ?? 0
            };
        }

        public PagedResult<CupItemViewModel> GetAllCupItemPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            var query = _cupItemsRepository
                .FindAll();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword) 
                    || x.Code.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => new CupItemViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    HashRate = x.HashRate,
                    ImageUrl = x.ImageUrl,
                    MaxOut = x.MaxOut,
                    Name = x.Name,
                    Price = x.Price,
                    TimeToUse = x.TimeToUse,
                    DateCreated = x.DateCreated
                })
                .ToList();

            return new PagedResult<CupItemViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public List<CupItemViewModel> GetAllCupItems()
        {
            var query = _cupItemsRepository.FindAll(x => !x.IsFree);

            return query.Select(x => new CupItemViewModel
            {
                Code = x.Code,
                HashRate = x.HashRate,
                ImageUrl = x.ImageUrl,
                MaxOut = x.MaxOut,
                Name = x.Name,
                Price = x.Price,
                TimeToUse = x.TimeToUse
            }).ToList();
        }

        public CupItemViewModel GetCupItemById(int id)
        {
            var entity = _cupItemsRepository.FindById(id);

            if (entity!=null)
            {
                return new CupItemViewModel
                {
                  Id = entity.Id,
                  Code = entity.Code,
                  HashRate = entity.HashRate,
                  ImageUrl = entity.ImageUrl,
                  MaxOut = entity.MaxOut,
                  TimeToUse = entity.TimeToUse,
                  Name = entity.Name,
                  Price = entity.Price,
                  DateCreated = entity.DateCreated
                };
            }

            return null;
        }

        public void SaveCupItem(CupItemViewModel model)
        {
            var entity = _cupItemsRepository.FindById(model.Id);

            entity.DateModified = DateTime.Now;
            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Price = model.Price;
            entity.TimeToUse = model.TimeToUse;
            entity.MaxOut = model.MaxOut;
            entity.ImageUrl = model.ImageUrl;
            entity.HashRate = model.HashRate;
            
            _unitOfWork.Commit();
        }

        public MachineItemViewModel GetMachineItemById(int id)
        {
            var entity = _machineItemsRepository.FindById(id);

            if (entity != null)
            {
                return new MachineItemViewModel
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    HashRate = entity.HashRate,
                    ImageUrl = entity.ImageUrl,
                    TimeToUse = entity.TimeToUse,
                    Name = entity.Name,
                    Price = entity.Price,
                    DateCreated = entity.DateCreated
                };
            }

            return null;
        }

        public void SaveMachineItem(MachineItemViewModel model)
        {
            var entity = _machineItemsRepository.FindById(model.Id);

            entity.DateModified = DateTime.Now;
            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Price = model.Price;
            entity.TimeToUse = model.TimeToUse;
            entity.ImageUrl = model.ImageUrl;
            entity.HashRate = model.HashRate;

            _unitOfWork.Commit();
        }


        public List<MachineItemViewModel> GetAllMachineItems()
        {
            var query = _machineItemsRepository.FindAll();

            return query.Select(x => new MachineItemViewModel
            {
                Code = x.Code,
                HashRate = x.HashRate,
                ImageUrl = x.ImageUrl,
                Name = x.Name,
                Price = x.Price,
                TimeToUse = x.TimeToUse
            }).ToList();
        }

        public PagedResult<MachineItemViewModel> GetAllMachineItemPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            var query = _machineItemsRepository
                .FindAll();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => new MachineItemViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    HashRate = x.HashRate,
                    ImageUrl = x.ImageUrl,
                    Name = x.Name,
                    Price = x.Price,
                    TimeToUse = x.TimeToUse,
                    DateCreated = x.DateCreated
                })
                .ToList();

            return new PagedResult<MachineItemViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<UserCupHistoryViewModel> GetAllUserCupHistoryPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            var query = _appUsersCupItemHistoriesRepository.FindAll(x=>x.AppUser , x=>x.CupItem);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AppUser.UserName.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => new UserCupHistoryViewModel
                {
                    Id = x.Id,
                    AppUserName = x.AppUser.UserName,
                    Name = x.CupItem.Name,
                    Code = x.CupItem.Code,
                    HashRate = x.HashRate,
                    RemainTime = x.RemainTime,
                    Sponsor = x.AppUser.Sponsor,
                    Price = x.Price,
                    TimeToUse = x.TimeToUse,
                    DateCreated = x.DateCreated,
                    CurrentHashEarn = x.CurrentHashEarn,
                    ImageUrl = x.CupItem.ImageUrl
                })
                .ToList();

            return new PagedResult<UserCupHistoryViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }


        public PagedResult<DrinkHistoryViewModel> GetAllUserDrinkHistoryPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            var query = _drinkToEarnHistoriesRepository.FindAll(x=>x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AppUser.UserName.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(PrepareUserDrinkHistoryModel)
                .ToList();

            return new PagedResult<DrinkHistoryViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        DrinkHistoryViewModel PrepareUserDrinkHistoryModel(DrinkToEarnHistories drinkToEarnHistory)
        {
            var store = _googleMapGISRepository.FindById(drinkToEarnHistory.StoreId);

            var cup = _cupItemsRepository.FindById(drinkToEarnHistory.CupItemId);

            var drinkHistory = new DrinkHistoryViewModel
            {
                StoreId = drinkToEarnHistory.StoreId,
                StoreName = store.StoreName,
                CupName = cup.Name,
                CupImageUrl = cup.ImageUrl,
                StoreImageUrl = store.ImgUrl,
                HashRate = drinkToEarnHistory.HashRate,
                LastPingTime = drinkToEarnHistory.LastPingTime,
                StartOn = drinkToEarnHistory.DateCreated,
                LeaveOn = drinkToEarnHistory.LeavedOn,
                CurrentHashEarn = drinkToEarnHistory.EarnResult??0,
                Status = drinkToEarnHistory.StatusId.GetDescription(),
                AppUserName = drinkToEarnHistory.AppUser.UserName,
                Sponsor = drinkToEarnHistory.AppUser.Sponsor,
                Remarks = drinkToEarnHistory.Remarks
            };

            if (string.IsNullOrEmpty(drinkHistory.StoreImageUrl))
            {
                drinkHistory.StoreImageUrl = "/images/large_shop_default.jpg";
            }

            return drinkHistory;
        }


        public PagedResult<ShopItemsViewModel> GetAllShopItemsPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            var query = _shopItemsRepository
                .FindAll();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => new ShopItemsViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    HashRate = x.HashRate,
                    ImageUrl = x.ImageUrl,
                    Name = x.Name,
                    Price = x.Price,
                    TimeToUse = x.TimeToUse,
                    DateCreated = x.DateCreated
                })
                .ToList();

            return new PagedResult<ShopItemsViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public List<ShopItemsViewModel> GetAllShopItems()
        {
            var query = _shopItemsRepository.FindAll();

            return query.Select(x => new ShopItemsViewModel
            {
                Code = x.Code,
                HashRate = x.HashRate,
                ImageUrl = x.ImageUrl,
                Name = x.Name,
                Price = x.Price,
                TimeToUse = x.TimeToUse
            }).ToList();
        }

        public ShopItemsViewModel GetShopItemById(int id)
        {
            var entity = _shopItemsRepository.FindById(id);

            if (entity != null)
            {
                return new ShopItemsViewModel
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    HashRate = entity.HashRate,
                    ImageUrl = entity.ImageUrl,
                    TimeToUse = entity.TimeToUse,
                    Name = entity.Name,
                    Price = entity.Price,
                    DateCreated = entity.DateCreated
                };
            }

            return null;
        }

        public void SaveShopItem(ShopItemsViewModel model)
        {
            var entity = _shopItemsRepository.FindById(model.Id);

            entity.DateModified = DateTime.Now;
            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Price = model.Price;
            entity.TimeToUse = model.TimeToUse;
            entity.ImageUrl = model.ImageUrl;
            entity.HashRate = model.HashRate;

            _unitOfWork.Commit();
        }
    }
}
