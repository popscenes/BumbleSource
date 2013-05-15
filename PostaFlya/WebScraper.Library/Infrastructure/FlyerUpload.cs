using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Models.Flier;
using WebScraper.Library.Model;
using Website.Application.Extension.Content;
using Website.Infrastructure.Configuration;

namespace WebScraper.Library.Infrastructure
{
    public class FlyerUpload
    {
        private readonly string _authcookie;
        private readonly ImportedFlyerScraperModel _model;
        private readonly String _server;
        private readonly String _flyerPost;
        private readonly String _imagePost;
        

        public FlyerUpload(string authcookie, string browserId, ImportedFlyerScraperModel model, string server)
        {
            _authcookie = authcookie;
            _model = model;
            _server = server;
            _flyerPost = string.Format("/api/Browser/{0}/MyFliers", browserId);
            _imagePost = "/Img/Post";
        }


        public async Task<bool> Request()
        {
            if (!await RetrieveImage())
            {
                return false;
            }

            if (!await UploadImage())
            {
                return false;
            }

            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(_server), new Cookie(".ASPXAUTH", _authcookie));
            
                using (var handler = new HttpClientHandler() {CookieContainer = cookieContainer})
                {
                    using (var client = new HttpClient(handler))
                    {
                        using (var req = new HttpRequestMessage())
                        {
                            req.Method = HttpMethod.Post;
                            req.RequestUri = new Uri(_server + _flyerPost);
                            var cont = new FlierCreateModel().MapFrom(_model);
                            req.Content = new ObjectContent<FlierCreateModel>(cont, new JsonMediaTypeFormatter());
                            using (var res = await client.SendAsync(req))
                            {
                                if (!res.IsSuccessStatusCode)
                                {
                                    Trace.TraceError("Error " + res.StatusCode + " " + await res.Content.ReadAsStringAsync());
                                    return false;
                                }
                                else
                                {
                                    Trace.TraceInformation("Created " + res.Headers.Location);
                                    return true;
                                }


                                //await res.Content.ReadAsAsync<Rootobject>();
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


        private async Task<bool> UploadImage()
        {
            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(_server), new Cookie(".ASPXAUTH", _authcookie));

                using (var handler = new HttpClientHandler() {CookieContainer = cookieContainer})
                {
                    using (var client = new HttpClient(handler))
                    {
                        using (var req = new HttpRequestMessage())
                        {
                            req.Method = HttpMethod.Post;
                            req.RequestUri = new Uri(_server + _imagePost + "?Anonymous=true");


                            using (var content = new MultipartContent())
                            {
                                content.Add(new ByteArrayContent(_model.Image.GetBytes()));
                                using (var res = await client.SendAsync(req))
                                {
                                    if (!res.IsSuccessStatusCode)
                                    {
                                        Trace.TraceError("Error " + res.StatusCode + " " + await res.Content.ReadAsStringAsync());
                                        return false;
                                    }
                                    else
                                    {

                                        var loc = res.Headers.Location.ToString();
                                        var imageId = loc.Substring(loc.LastIndexOf('/') + 1);
                                        _model.ImageUrl = imageId;
                                        Trace.TraceInformation("Created " + res.Headers.Location);
                                        return true;
                                    }

                                }
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

        private async Task<bool> RetrieveImage()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var clientResponse = await client.GetByteArrayAsync(_model.ImageUrl);
                    _model.Image = clientResponse.GetImage();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to retrieve image " + _model.ImageUrl + "\n" + e);
                return false;
            }
            return true;
        }
    }
}
