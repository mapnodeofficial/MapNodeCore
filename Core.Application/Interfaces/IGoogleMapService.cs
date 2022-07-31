using Core.Data.SpEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IGoogleMapService
    {
        Task<bool> SyncStoreAsync(string lat,string lng);

        List<GoogleMapGISNearby> GetNeabyStoreByCurrentPosition(string lat, string lng, decimal radius = 1); // radius = km;

        bool IsInsideStore(double lat1, double lon1, double lat2, double lon2);

        GoogleMapGISNearby GetActiveStoreByCurrentPosition(string lat, string lng);

        double GetDistanceOf(double lat1, double lon1, double lat2, double lon2);

        double GetDistanceOf(string lat1, string lon1, string lat2, string lon2);


    }
}
