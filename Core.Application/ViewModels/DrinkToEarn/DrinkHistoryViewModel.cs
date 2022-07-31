using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.DrinkToEarn
{
    public class DrinkHistoryViewModel
    {
        public Guid AppUserId { get;set;}

        public long StoreId { get;set;}

        public int CupItemId { get;set;}

        public DateTime? LeaveOn { get;set;}

        public int Id { get;set;}

        public DateTime LastPingTime { get;set;}

        public string StoreName { get;set;}

        public string CupName { get;set; }

        public decimal HashRate { get;set;}

        public DateTime StartOn { get;set;}

        public decimal HashEarn { get;set;}

        public string CupImageUrl { get;set;}

        public string StoreImageUrl { get;set;}

        public string Status { get;set;}

        public int CurrentHashEarn { get;set;}

        public string AppUserName { get;set;}

        public string Sponsor { get;set;}

        public string Remarks { get;set;}

    }
}
