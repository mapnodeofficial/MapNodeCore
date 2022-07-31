using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("DrinkAccessCode")]
    public class DrinkAccessCode : DomainEntity<int>
    {
        public string TokenCode { get; set; }

        public Guid AppUserId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
