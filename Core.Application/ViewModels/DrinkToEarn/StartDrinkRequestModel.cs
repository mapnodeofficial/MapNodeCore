using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.DrinkToEarn
{
    public class StartDrinkRequestModel
    {
        public int Id { get; set; }

        public string Lat { get; set; } 

        public string Lng { get;set;}

        //int id, int storeId, string lat, string lng
    }
}
