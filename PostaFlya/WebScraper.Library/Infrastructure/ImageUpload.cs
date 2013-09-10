using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Library.Model;
using Website.Application.Extension.Content;

namespace WebScraper.Library.Infrastructure
{
    public class ImageUpload
    {
        private readonly string _authcookie;
        private readonly String _server;
        private readonly String _imageName;
        private readonly String _imagePost;
        private readonly Image _image;


        public ImageUpload(string authcookie, string server, Image image, String fileName)
        {
            _authcookie = authcookie;
            _server = server;
            _imagePost = "/Img/Post";
            _image = image;
            _imageName = fileName;
        }

        public async Task<String> UploadImage()
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
                            req.RequestUri = new Uri(_server + _imagePost + "?Anonymous=true&KeepFileImapeType=true");


                            using (var content = new MultipartFormDataContent())
                            {
                                var fileContent = new ByteArrayContent(_image.GetBytes());
                                content.Add(fileContent, @"""file""", _imageName);
                                req.Content = content;
                                using (var res = await client.SendAsync(req))
                                {
                                    if (!res.IsSuccessStatusCode)
                                    {
                                        Trace.TraceError("Image upload failed Error " + res.StatusCode + " " + await res.Content.ReadAsStringAsync());
                                        return "";
                                    }
                                    var loc = res.Headers.Location.ToString();
                                    var imageId = loc.Substring(loc.LastIndexOf('/') + 1).Replace(Path.GetExtension(_imageName), "");
                                    Trace.TraceInformation("Image Uploaded " + res.Headers.Location);
                                    return imageId;
                                }
                            }

                        }
                    }
                }

            }
            catch (Exception e)
            {
                Trace.TraceError("Image upload failed Error " + e);
                return "";
            }

        }
    }
}
