using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.GoogleApi
{
    public class GoogleApiResponse
    {
        public string next_page_token { get; set; }

        public List<GoogleApiDataModel> results { get;set; } = new List<GoogleApiDataModel>();
    }

    public class GoogleApiDataModel
    {
        public string name { get;set;}

        public string place_id { get;set;}

        public string vicinity { get;set;}

        public List<string> types { get;set;}

        public geometry geometry { get;set;}

        public List<photos> photos { get;set;}

        public plus_code plus_code { get;set;}


    }

    public class plus_code
    {
        public string compound_code { get;set;}
    }

    public class photos
    {
        public int height { get;set;}

        public string photo_reference { get;set;}
    }

    public class geometry
    {
        public location location { get;set;}
    }

    public class location
    {
        public string lat { get;set;}

        public string lng { get;set;}


    }
}
