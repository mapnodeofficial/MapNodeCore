using Core.Application.ViewModels.DrinkToEarn;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;

namespace Core.Application.Interfaces
{
    public interface IDrinkToEarnService
    {
        public void SyncFreeCup(Guid appUserId);

        PagedResult<CupItemViewModel> GetAllUserCupPaging(Guid userId, int pageIndex = 1, int pageSize = 20);

        GenericResult ProcessNewDrink(int cupItemId, long storeId, Guid appUserId, string lat, string lng);

        GenericResult ProcessSwapHashToMNO(int cupItemHistoryId, string appUserId);

        void SyncIsAtStoreStatus(Guid appUserId, string lat, string lng);

        void SyncDrinkAtStoreStatus();

        string GenerateDrinkTokenByUser(Guid userId);

        Guid GetUserIdByDrinkToken(string code);

        void DeleteUserDrinkCode(Guid userId);

        GenericResult ProcessStopDrink(int cupItemHistoryId, Guid appUserId, string lat, string lng, DateTime stopDate);

        decimal CalculateTokenBetweenTimes(DateTime from, DateTime to, decimal hashrate);

        PagedResult<DrinkHistoryViewModel> GetAllDrinkHistoryPaging(
            Guid userId, string keyword = "", int pageIndex = 1, int pageSize = 20);

        List<CupItemViewModel> GetAllCupItems();

        List<MachineItemViewModel> GetAllMachineItems();

        PagedResult<CupItemViewModel> GetAllCupItemPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20);

        PagedResult<MachineItemViewModel> GetAllMachineItemPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20);

        CupItemViewModel GetCupItemById(int id);

        void SaveCupItem(CupItemViewModel model);

        void SaveMachineItem(MachineItemViewModel model);

        MachineItemViewModel GetMachineItemById(int id);

        PagedResult<UserCupHistoryViewModel> GetAllUserCupHistoryPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20);

        PagedResult<DrinkHistoryViewModel> GetAllUserDrinkHistoryPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20);

        PagedResult<ShopItemsViewModel> GetAllShopItemsPaging(string keyword = "",
            int pageIndex = 1,
            int pageSize = 20);

        List<ShopItemsViewModel> GetAllShopItems();

        ShopItemsViewModel GetShopItemById(int id);

        void SaveShopItem(ShopItemsViewModel model);

    }
}
