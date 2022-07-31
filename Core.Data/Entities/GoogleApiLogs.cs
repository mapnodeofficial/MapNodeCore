using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    [Table("GoogleApiLogs")]
    public class GoogleApiLogs : DomainEntity<long>, IDateTracking
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public string Remarks { get;set;}

        public string Lat { get;set;}

        public string Lng { get;set;}

        public decimal Radius { get;set;}

    }
}
