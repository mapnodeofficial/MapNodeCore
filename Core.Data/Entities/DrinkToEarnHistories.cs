using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;

namespace Core.Data.Entities
{
    public class DrinkToEarnHistories : DomainEntity<int>, IDateTracking
    {
        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public Guid AppUserId { get;set;}

        public long StoreId { get;set;}

        public int CupItemId { get;set;}

        public DateTime? LeavedOn { get;set;}

        public decimal HashRate { get;set;}

        public string Remarks { get;set;}

        public string UserLat { get;set;}

        public string UserLng { get;set;}

        public string StorePlaceId { get;set;}

        public DateTime LastPingTime { get;set;}

        public DrinkHistoryStatus StatusId { get;set;}

        public int? EarnResult { get;set;} = 0;

        public string LeaveLat { get;set;}

        public string LeaveLng { get;set;}

        public int CupItemHistoryId { get;set;}

        public virtual AppUser AppUser { get;set;}

    }
}
