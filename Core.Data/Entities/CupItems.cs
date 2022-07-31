using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    public  class CupItems : DomainEntity<int>, IDateTracking
    {
        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public string Code { get; set; }

        public string ImageUrl { get; set; }

        public decimal HashRate { get; set; }

        public decimal TimeToUse { get; set; }

        public decimal MaxOut { get; set; }

        public decimal Price { get; set; }

        public bool IsFree { get; set; }

        public string Name { get; set; }
    }
}
