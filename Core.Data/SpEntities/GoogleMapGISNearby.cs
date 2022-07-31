using System.ComponentModel.DataAnnotations;

namespace Core.Data.SpEntities
{
    public class GoogleMapGISNearby
    {
        [Key]
        public long Id { get;set;}

        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string StoreAddress { get; set; }

        public string ImgUrl { get; set; }

        public string Lat { get; set; }

        public string Lng { get; set; }

        public double Distance { get;set;}

        public string Website { get; set; }

        public string Phone { get; set; }
    }
}
