using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    public class AppUsersCupItemHistories : DomainEntity<int>, IDateTracking
    {
        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public int CupItemId { get;set; }

        public decimal HashRate { get; set; }

        public decimal MaxOut { get; set; }

        public decimal TimeToUse { get; set; }

        public decimal Price { get; set; }

        public Guid AppUserId { get; set; }

        public decimal RemainTime { get;set;}

        public CupItemStatus ItemStatus { get;set;}

        public int CurrentHashEarn { get; set; }

        public virtual AppUser AppUser { get;set;}

        public virtual CupItems CupItem { get;set;}
    }
}
