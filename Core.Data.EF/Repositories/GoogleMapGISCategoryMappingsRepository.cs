using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{
    public class GoogleMapGISCategoryMappingsRepository : EFRepository<GoogleMapGISCategoryMappings, long>, IGoogleMapGISCategoryMappingsRepository
    {
        public GoogleMapGISCategoryMappingsRepository(AppDbContext context) : base(context)
        {
        }
    }
}
