using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.DrinkToEarn
{
    public class UserCupHistoryViewModel
    {
        public int Id { get;set;}

        public string AppUserName { get;set;}

        public string Sponsor { get;set;}

        public string Name { get;set;}

        public string Code { get;set;}

        public decimal HashRate { get;set;}

        public decimal TimeToUse { get;set;}

        public decimal Price { get;set;}

        public decimal RemainTime { get;set;}

        public DateTime DateCreated { get;set;}

        public decimal CurrentHashEarn { get;set;}

        public string ImageUrl { get;set;}


    }
}
