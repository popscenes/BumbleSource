using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Website.Application.Google.Places
{


    namespace Details
    {
        public class PlaceDetailsRequest
        {
            private const string Url = @"http://maps.googleapis.com/maps/api/place/details/json?sensor=false&reference=";
            private readonly string _reference;

            public PlaceDetailsRequest(string reference)
            {
                _reference = reference;
            }

            public async Task<Rootobject> Request()
            {
                using (var client = new HttpClient())
                {
                    using (var req = new HttpRequestMessage())
                    {
                        req.Method = HttpMethod.Get;
                        req.RequestUri = GetUri();
                        using (var res = await client.SendAsync(req))
                        {
                            return await res.Content.ReadAsAsync<Rootobject>();
                        }
                    }
                }
            }

            private Uri GetUri()
            {
                var uri = Url + _reference;
                uri = uri.AddApiKey(GoogleApiUtil.PlacesKey);
                return new Uri(uri);
            }
        }

        #region PlaceDetails

        public class Rootobject
        {
            public object[] html_attributions { get; set; }
            public Result result { get; set; }
            public string status { get; set; }
        }

        public class Result
        {
            public Address_Components[] address_components { get; set; }
            public string formatted_address { get; set; }
            public string formatted_phone_number { get; set; }
            public Geometry geometry { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public string international_phone_number { get; set; }
            public string name { get; set; }
            public Opening_Hours opening_hours { get; set; }
            public string reference { get; set; }
            public Review[] reviews { get; set; }
            public string[] types { get; set; }
            public string url { get; set; }
            public int utc_offset { get; set; }
            public string vicinity { get; set; }
            public string website { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
        }

        public class Location
        {
            public float lat { get; set; }
            public float lng { get; set; }
        }

        public class Opening_Hours
        {
            public bool open_now { get; set; }
            public Period[] periods { get; set; }
        }

        public class Period
        {
            public Close close { get; set; }
            public Open open { get; set; }
        }

        public class Close
        {
            public int day { get; set; }
            public string time { get; set; }
        }

        public class Open
        {
            public int day { get; set; }
            public string time { get; set; }
        }

        public class Address_Components
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public string[] types { get; set; }
        }

        public class Review
        {
            public Aspect[] aspects { get; set; }
            public string author_name { get; set; }
            public string author_url { get; set; }
            public string text { get; set; }
            public int time { get; set; }
        }

        public class Aspect
        {
            public int rating { get; set; }
            public string type { get; set; }
        }
        #endregion

    }


    namespace PlaceSearch
    {

        public class PlaceSearchRequest
        {
            private const string Url = @"http://maps.googleapis.com/maps/api/place/textsearch/json?sensor=false&query=";
            private readonly string _searchTerm;

            public PlaceSearchRequest(string searchTerm)
            {
                _searchTerm = searchTerm;
            }

            public async Task<Rootobject> Request()
            {
                using (var client = new HttpClient())
                {
                    using (var req = new HttpRequestMessage())
                    {
                        req.Method = HttpMethod.Get;
                        req.RequestUri = GetUri();
                        using (var res = await client.SendAsync(req))
                        {
                            return await res.Content.ReadAsAsync<Rootobject>();
                        }
                    }
                }
            }

            private Uri GetUri()
            {
                var uri = Url + _searchTerm;
                uri = uri.AddApiKey(GoogleApiUtil.PlacesKey);
                return new Uri(uri);
            }
        }
        #region PlaceSearch
        public class Rootobject
        {
            public string[] html_attributions { get; set; }
            public string next_page_token { get; set; }
            public Result[] results { get; set; }
            public string status { get; set; }
        }

        public class Result
        {
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public Opening_Hours opening_hours { get; set; }
            public Photo[] photos { get; set; }
            public float rating { get; set; }
            public string reference { get; set; }
            public string[] types { get; set; }
            public int price_level { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
        }

        public class Location
        {
            public float lat { get; set; }
            public float lng { get; set; }
        }

        public class Opening_Hours
        {
            public bool open_now { get; set; }
        }

        public class Photo
        {
            public int height { get; set; }
            public string[] html_attributions { get; set; }
            public string photo_reference { get; set; }
            public int width { get; set; }
        }

        #endregion
    }
}
