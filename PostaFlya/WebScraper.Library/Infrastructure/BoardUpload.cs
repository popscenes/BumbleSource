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
using PostaFlya.Models.Flier;
using WebScraper.Library.Model;

namespace WebScraper.Library.Infrastructure
{
    public class BoardUpload
    {
        private readonly string _authcookie;
        private readonly BoardCreateEditModel _model;
        private readonly String _server;
        private readonly String _flyerPost;
        private readonly String _imagePost;

        public BoardUpload(string authcookie, string browserId, BoardCreateEditModel model, string server)
        {
            _authcookie = authcookie;
            _model = model;
            _server = server;
            _flyerPost = string.Format("/api/Browser/{0}/MyBoards", browserId);
        }

        public async Task<bool> Request()
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
                            req.Method = HttpMethod.Post;
                            req.RequestUri = new Uri(_server + _flyerPost);
                            
                            Trace.TraceInformation(JsonConvert.SerializeObject(_model));

                            //_model.Id = "fuckity";

                            req.Content = new ObjectContent<BoardCreateEditModel>(_model, new JsonMediaTypeFormatter());
                            using (var res = await client.SendAsync(req))
                            {
                                if (!res.IsSuccessStatusCode)
                                {
                                    Trace.TraceError("Error " + res.StatusCode + " " + await res.Content.ReadAsStringAsync());
                                    return false;
                                }
                                
                                there Trace.TraceInformation("Created " + res.Headers.Location);
                                return true;
                                
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error " + e);
                return false;
            }
        }
    }
}
