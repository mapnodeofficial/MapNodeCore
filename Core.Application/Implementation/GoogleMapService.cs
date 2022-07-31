using Core.Application.Interfaces;
using Core.Application.ViewModels.GoogleApi;
using Core.Data.IRepositories;
using Core.Data.SpEntities;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class GoogleMapService : IGoogleMapService
    {
        private readonly IGoogleMapGISRepository _googleMapGISRepository;
        private readonly IGoogleMapCategoriesRepository _googleMapCategoriesRepository;
        private readonly IGoogleApiLogsRepository _googleApiLogsRepository;
        private readonly IGoogleMapGISCategoryMappingsRepository _googleMapGISCategoryMappingsRepository;
        private readonly ILogger<GoogleMapService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private IConfiguration _configuration;
        private readonly IHttpService _httpService;
        public GoogleMapService(
            IUnitOfWork unitOfWork,
            IGoogleMapGISRepository googleMapGISRepository,
            IConfiguration configuration,
            IHttpService httpService,
            IGoogleApiLogsRepository googleApiLogsRepository,
            IGoogleMapCategoriesRepository googleMapCategoriesRepository,
            IGoogleMapGISCategoryMappingsRepository googleMapGISCategoryMappingsRepository,
            ILogger<GoogleMapService> logger)
        {
            _logger = logger;   
            _configuration = configuration;
            _googleMapGISRepository = googleMapGISRepository;
            _unitOfWork = unitOfWork;
            _httpService = httpService;
            _googleApiLogsRepository = googleApiLogsRepository;
            _googleMapCategoriesRepository = googleMapCategoriesRepository;
            _googleMapGISCategoryMappingsRepository = googleMapGISCategoryMappingsRepository;
        }

        private string GoogleMapApi
        {
            get
            {
                return _configuration["GooglePlaceApi"];
            }
        }

        public async Task<bool> SyncStoreAsync(string lat, string lng)
        {
            var url = BuildGoogleUrl(lat, lng);

            //await ProcessGooglePlaceData(url);

            //SyncStoreGoogle();

            return true;
        }

        private string BuildGoogleUrl(string lat, string lng)
        {
            return "https://maps.googleapis.com/maps/api/place/nearbysearch/json?keyword=coffee&location=10.963106%2C106.855709&radius=50000&type=cafe&key=AIzaSyAqpMeKybB-v_DvCVJVYEWGrxRBXKP_wiY";
        }

        private async Task<GenericResult> FetchGooglePlaceInfoAsync(string url)
        {
            return await _httpService.GetAsync(url);
        }

        private async Task<bool> ProcessGooglePlaceData(string url)
        {
            var data = await FetchGooglePlaceInfoAsync(url);

            var formattedData = JsonConvert.DeserializeObject<GoogleApiResponse>(data.Message.ToString());

            SaveLog(data.Message.ToString(), formattedData.next_page_token, url);

            while (!string.IsNullOrEmpty(formattedData.next_page_token))
            {
                url += $"&pagetoken={formattedData.next_page_token}";

                data = await FetchGooglePlaceInfoAsync(url);

                formattedData = JsonConvert.DeserializeObject<GoogleApiResponse>(data.Message.ToString());

                SaveLog(data.Message.ToString(), formattedData.next_page_token, url);
                Thread.Sleep(500);
            }

            return true;
        }


        private void SaveLog(string data, string nextPageToken, string url)
        {


            _googleApiLogsRepository.Add(new Data.Entities.GoogleApiLogs
            {

                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
            });

            _unitOfWork.Commit();
        }

        //void SyncStoreGoogle()
        //{
        //    try
        //    {
        //        var storeLogs = _googleApiLogsRepository.FindAll().ToList();

        //        List<string> cats = new List<string>();

        //        foreach (var item in storeLogs)
        //        {
        //            var formattedData = JsonConvert.DeserializeObject<GoogleApiResponse>(item.DataContent);

        //            foreach (var stor in formattedData.results)
        //            {
        //                if (_googleMapGISRepository.FindAll().Any(d => d.StoreId.Equals(stor.place_id)))
        //                {
        //                    continue;
        //                }

        //                var obj = new Data.Entities.GoogleMapGIS
        //                {
        //                    DateCreated = DateTime.Now,
        //                    DateModified = DateTime.Now,

        //                    StoreAddress = $"{stor.vicinity} {stor.plus_code?.compound_code}",
        //                    StoreId = stor.place_id,
        //                    StoreName = stor.name,
        //                    Lat = stor.geometry.location.lat,
        //                    Lng = stor.geometry.location.lng
        //                };

        //                if (stor.photos!=null && stor.photos.Count>0)
        //                {
        //                    obj.ImgUrl = stor.photos.FirstOrDefault().photo_reference;
        //                }

        //                _googleMapGISRepository.Add(obj);


        //                foreach (var t in stor.types)
        //                {
        //                    _googleMapGISCategoryMappingsRepository.Add(new Data.Entities.GoogleMapGISCategoryMappings
        //                    {
        //                        StoreCategoryId = t,
        //                        StoreId = stor.place_id
        //                    });

        //                }
        //                _unitOfWork.Commit();
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {

        //        throw;
        //    }

        //}


        public List<GoogleMapGISNearby> GetNeabyStoreByCurrentPosition(string lat, string lng, decimal radius = 1) // radius = km
        {
            var stores = _googleMapGISRepository.GetNeabyStoreByCurrentPosition(lat, lng, radius);

            if (stores.Count == 0)
            {
                LogEmptyStore(lat, lng, radius, "Not found store nearby");
            }

            foreach (var item in stores)
            {
                CalculateStoreDistance(item, lat, lng);
            }

            return stores;
        }

        public GoogleMapGISNearby GetActiveStoreByCurrentPosition(string lat, string lng)
        {

            var store = _googleMapGISRepository.GetCurrentActiveStoreByCurrentPosition(lat, lng, 0.05m);

            if (store == null)return null;


            _logger.LogInformation(JsonConvert.SerializeObject(store));

            _logger.LogInformation($"GetActiveStoreByCurrentPosition - {lat} - {lng}");

            int limitDistance = 20;

            if (_configuration.GetValue<int>("DistanceLimit") > 0)
                limitDistance = _configuration.GetValue<int>("DistanceLimit");

            _logger.LogInformation($"GetActiveStoreByCurrentPosition Prepare Parse Distance - {lat} - {lng} - {store.Lat} - {store.Lng}");

            _logger.LogInformation($"GetActiveStoreByCurrentPosition After Parse Distance - {double.Parse(lat, CultureInfo.InvariantCulture)} - {double.Parse(lng, CultureInfo.InvariantCulture)} - {double.Parse(store.Lat, CultureInfo.InvariantCulture)} - {double.Parse(store.Lng, CultureInfo.InvariantCulture)}");

            var distance = GetDistanceOf(double.Parse(lat, CultureInfo.InvariantCulture),
                double.Parse(lng, CultureInfo.InvariantCulture),
                double.Parse(store.Lat, CultureInfo.InvariantCulture),
                double.Parse(store.Lng, CultureInfo.InvariantCulture));

            _logger.LogInformation($"GetActiveStoreByCurrentPosition - {lat} - {lng} store - {store.StoreName} - distance {distance} - limit distance {limitDistance}");

            if (IsInsideStore(double.Parse(lat, CultureInfo.InvariantCulture),
                double.Parse(lng, CultureInfo.InvariantCulture),
                double.Parse(store.Lat, CultureInfo.InvariantCulture),
                double.Parse(store.Lng, CultureInfo.InvariantCulture)))
                return store;


            return null;
        }

        private void CalculateStoreDistance(GoogleMapGISNearby storeInfo, string lat, string lng)
        {
            var distance = GetDistanceOf(double.Parse(lat, CultureInfo.InvariantCulture),
                double.Parse(lng, CultureInfo.InvariantCulture),
                double.Parse(storeInfo.Lat, CultureInfo.InvariantCulture),
                double.Parse(storeInfo.Lng, CultureInfo.InvariantCulture));

            storeInfo.Distance = Math.Round(distance / 1000,2);

        }

        public bool IsInsideStore(double lat1, double lon1, double lat2, double lon2)
        {
            var distance = GetDistanceOf(lat1, lon1, lat2, lon2);

            int limitDistance = 20;

            if (_configuration.GetValue<int>("DistanceLimit") > 0)
                limitDistance = _configuration.GetValue<int>("DistanceLimit");

            return distance <= limitDistance;
        }



        private void LogEmptyStore(string lat, string lng, decimal radius = 1, string remarks = "")
        {
            _googleApiLogsRepository.Add(new Data.Entities.GoogleApiLogs
            {
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                Lat = lat,
                Lng = lng,
                Radius = radius,
                Remarks = remarks
            });
        }

        public double GetDistanceOf(double lat1, double lon1, double lat2, double lon2)
        {
            const double MetersPerDegree = 111325 ;
            double distanceLatitude = Math.Abs(lat1 - lat2) * MetersPerDegree;
            double distanceLongitude = Math.Abs(lon1 - lon2) * MetersPerDegree;
            return Math.Sqrt((distanceLatitude * distanceLatitude) + (distanceLongitude * distanceLongitude));
        }

        public double GetDistanceOf(string lat1, string lon1, string lat2, string lon2)
        {
            var latS = double.Parse(lat1, CultureInfo.InvariantCulture);
            var lngS = double.Parse(lon1, CultureInfo.InvariantCulture);

            var latD = double.Parse(lat2, CultureInfo.InvariantCulture);
            var lngD = double.Parse(lon2, CultureInfo.InvariantCulture);

            const double MetersPerDegree = 111325;
            double distanceLatitude = Math.Abs(latS - latD) * MetersPerDegree;
            double distanceLongitude = Math.Abs(lngS - lngD) * MetersPerDegree;
            return Math.Sqrt((distanceLatitude * distanceLatitude) + (distanceLongitude * distanceLongitude));
        }

        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
    }
}
