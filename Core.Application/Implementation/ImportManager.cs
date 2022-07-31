using Core.Application.Helpers;
using Core.Application.Interfaces;
using Core.Application.ViewModels.ImportExport;
using Core.Data.Entities;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class ImportManager : IImportManager
    {

        private readonly IGoogleMapGISRepository _googleMapGISRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ImportManager(IGoogleMapGISRepository googleMapGISRepository,
            IUnitOfWork unitOfWork) { 
            _googleMapGISRepository = googleMapGISRepository;
            _unitOfWork = unitOfWork;   
        }   

        public ImportResponseResult ImportStoreFromXlsx(Stream stream)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<GoogleGISDto>(worksheet);

            var manager = new PropertyManager<GoogleGISDto>(properties);
            var iRow = 2;
            int successCount = 0;
            var currentStores = _googleMapGISRepository.FindAll().Select(x=> x.StoreId).ToList();
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                if (allColumnsAreEmpty)
                    break;

                //get store by data in xlsx file if it possible, or create new category

                var store = GetGoogleDataFromXlsx(manager, worksheet, iRow);

                if (currentStores.Any(d=>d == store.PlaceId))
                {
                    iRow++;
                    continue;
                }

                _googleMapGISRepository.Add(new GoogleMapGIS
                {
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    ImgUrl = store.Featuredimage,
                    Lat = store.Latitude.ToString(),
                    Lng = store.Longitude.ToString(),
                    Phone = store.Phone,
                    StoreAddress = store.Fulladdress,
                    StoreId = store.PlaceId,
                    StoreName = store.Name,
                    Website = store.Website,
                    Cid = store.Cid,
                    PermanentlyClosed=false,
                    TemporarilyClosed=false
                });
                _unitOfWork.Commit();
                successCount++;

                iRow++;
            }

            return new ImportResponseResult
            {
                IsSuccess = successCount > 0,
                ErrorMsg = $"Success {successCount}"
            };
        }

        protected virtual GoogleGISDto GetGoogleDataFromXlsx(PropertyManager<GoogleGISDto> manager,
            ExcelWorksheet worksheet, int iRow)
        {
            manager.ReadFromXlsx(worksheet, iRow);

            var store = new GoogleGISDto();

            foreach (var property in manager.GetProperties)
            {
                switch (property.PropertyName)
                {
                    case nameof(GoogleGISDto.Name):
                        store.Name = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.Fulladdress):
                        store.Fulladdress = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.Cid):
                        store.Cid = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.PlaceId):
                        store.PlaceId = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.Phone):
                        store.Phone = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.Website):
                        store.Website = property.StringValue;
                        break;
                    case nameof(GoogleGISDto.Latitude):
                        store.Latitude = property.DoubleValue;
                        break;
                    case nameof(GoogleGISDto.Longitude):
                        store.Longitude = property.DoubleValue;
                        break;
                    case nameof(GoogleGISDto.Featuredimage):
                        store.Featuredimage = property.StringValue;
                        break;
                }
            }

            return store;
        }



            public static IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(ExcelWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Cells[1, poz];

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }
    }
}
