using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("GoogleMapCategories")]
    public class GoogleMapCategories : DomainEntity<int>, IDateTracking
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public string Name { get; set; }
    }
}
