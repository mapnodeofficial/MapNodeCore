using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    [Table("GoogleMapGISCategoryMappings")]
    public class GoogleMapGISCategoryMappings : DomainEntity<long>
    {
        public string StoreId { get; set; }

        public string StoreCategoryId { get;set;}
    }
}
