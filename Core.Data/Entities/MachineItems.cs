using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;

namespace Core.Data.Entities
{
    public class MachineItems : DomainEntity<int>, IDateTracking
    {
        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public string Code { get; set; }

        public string ImageUrl { get; set; }

        public decimal HashRate { get; set; }

        public decimal TimeToUse { get; set; }

        public decimal Price { get; set; }

        public string Name { get; set; }
    }
}
