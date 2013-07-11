using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PostaFlya.Models.Board;

namespace WebScraper.Library.Infrastructure
{
    public class BoardsGet
    {
        private readonly string _authcookie;
        private readonly String _server;
        private readonly String _boardGetReq;

        public BoardsGet(string server, string authcookie)
        {
            _authcookie = authcookie;
            _server = server;

            //hardoded this for melb for now
            _boardGetReq = "/api/BoardSearchApi?loc%5BLongitude%5D=144.97169099999996&loc%5BLatitude%5D=-37.81138&distance=25000&count=3000";
        }

        public async Task<List<BoardModel>> Request()
        {
            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(_server), new Cookie(".ASPXAUTH", _authcookie));

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                {
                    using (var client = new HttpClient(handler))
                    {
                        using (var req = new HttpRequestMessage())
                        {
                            req.Method = HttpMethod.Get;
                            req.RequestUri = new Uri(_server + _boardGetReq);

                            using (var res = await client.SendAsync(req))
                            {
                                if (!res.IsSuccessStatusCode)
                                {
                                    Trace.TraceError("Error " + res.StatusCode + " " + await res.Content.ReadAsStringAsync());
                                    return null;
                                }

                                string responseString = await res.Content.ReadAsStringAsync();
                                var ret = JsonConvert.DeserializeObject < List<BoardModel>>(responseString);

                                return ret;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error " + e);
                return null;
            }
        }
    }
}
