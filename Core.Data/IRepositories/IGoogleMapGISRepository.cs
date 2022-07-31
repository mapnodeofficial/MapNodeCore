using Core.Data.Entities;
using Core.Data.SpEntities;
using Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.IRepositories
{
   

    public interface IGoogleMapGISRepository : IRepository<GoogleMapGIS, long>
    {
        List<GoogleMapGISNearby> GetNeabyStoreByCurrentPosition(string lat, string lng, decimal radius);

        GoogleMapGISNearby GetCurrentActiveStoreByCurrentPosition(string lat, string lng, decimal radius);
    }
}
