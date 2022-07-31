using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("GoogleMapGIS")]
    public class GoogleMapGIS : DomainEntity<long>, IDateTracking
    {
        public DateTime DateCreated {get;set; }
        public DateTime DateModified { get; set; }

        public string StoreId { get;set;}

        public string StoreName { get;set;}

        public string StoreAddress { get;set;}

        public string ImgUrl { get;set;}

        public string Lat { get;set; }

        public string Lng { get; set; }

        public string Website { get;set;}

        public string Phone { get;set;}

        public bool PermanentlyClosed { get;set;}

        public string Cid { get;set;}

        public bool TemporarilyClosed { get;set; }
    }
}
