using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.DrinkToEarn
{
    public  class NftMarketplaceViewModel
    {
        public List<CupItemViewModel> CupItems { get;set;}

        public List<MachineItemViewModel> MachineItems { get;set;}

        public List<ShopItemsViewModel> ShopItems { get; set; }
    }
}
