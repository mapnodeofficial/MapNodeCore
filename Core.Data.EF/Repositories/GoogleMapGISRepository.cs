using Core.Data.Entities;
using Core.Data.IRepositories;
using Core.Data.SpEntities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Core.Data.EF.Repositories
{
    public class GoogleMapGISRepository : EFRepository<GoogleMapGIS, long>, IGoogleMapGISRepository
    {
        private readonly AppDbContext _context;


        public GoogleMapGISRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public List<GoogleMapGISNearby> GetNeabyStoreByCurrentPosition(string lat,string lng,decimal radius)
        {
            //SqlGeometry geo = SqlGeometry.Point(double.Parse(lat), double.Parse(lng), 4326);

            lat = lat.Replace(",", ".");
            lng = lng.Replace(",", ".");

            string query = "DECLARE @currentLocation geometry;" +
                "SET @currentLocation = geometry::STGeomFromText('POINT(" + lat + " " + lng + ")', 4326);" +
                "EXEC sp_GetStoreNearby " +
                "@distance={0}," +
                "@pageSize={1}," +
                "@currentLocation=@currentLocation";

            var results = _context.GoogleMapGISNearby.FromSqlRaw(
                query, 
                radius,
                10).ToList();

            return results;
        }

        public GoogleMapGISNearby GetCurrentActiveStoreByCurrentPosition(string lat, string lng, decimal radius)
        {
            lat = lat.Replace(",",".");
            lng = lng.Replace(",", ".");

            string query = "DECLARE @currentLocation geometry;" +
                "SET @currentLocation = geometry::STGeomFromText('POINT(" + lat + " " + lng + ")', 4326);" +
                "EXEC sp_GetStoreNearby " +
                "@distance={0}," +
                "@pageSize={1}," +
                "@currentLocation=@currentLocation";

            var result = _context.GoogleMapGISNearby.FromSqlRaw(
                query,
                radius,
                1).AsEnumerable().FirstOrDefault();

            return result;
        }
    }
}
