using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using Website.Application.Util;

namespace Website.Application.Intergrations
{

    public class DataHolder<T>
    {
        public List<T> data { get; set; }
    }

    public class FacebookGraph
    {
        private string _userAccessToken;

        public FacebookGraph(string userAccessToken)
        {
            _userAccessToken = userAccessToken;
        }

        private class JsonIdHolder
        {
            public String id { get; set; }
        }

        

        public FaceBookUser GetUser()
        {
            var requestUrl = String.Format("https://graph.facebook.com/me?access_token={0}&fields=id,name,first_name,last_name,middle_name,gender,email", Uri.EscapeDataString(_userAccessToken));
            var fbUser = GetAndSerialize<FaceBookUser>(requestUrl);
            return fbUser;
        }

        public String PublishStatus(String link, String message, String pictureLink)
        {
            var requestUrl = String.Format("https://graph.facebook.com/me/feed?method={0}&link={1}&picture={2}&message={3}&access_token={4}", "POST",
                Uri.EscapeDataString(link), Uri.EscapeDataString(pictureLink), Uri.EscapeDataString(message), Uri.EscapeDataString(_userAccessToken));
            var idHolder = GetAndSerialize<JsonIdHolder>(requestUrl);
            return idHolder.id;
        }

        public List<FaceBookEvent> UserEventsGet()
        {
            var requestUrl = String.Format("https://graph.facebook.com/me/events?access_token={0}&fields=description,name,end_time,venue,privacy,location,id", Uri.EscapeDataString(_userAccessToken));
            var events = GetAndSerialize<DataHolder<FaceBookEvent>>(requestUrl);
            events.data.ForEach(_ => _.picture = GetEventPictureUrl(_.id));
            return events.data;
        }

        protected string GetEventPictureUrl(string eventId)
        {
            return String.Format("https://graph.facebook.com/{0}/picture?type=large", eventId);
        }

        public List<FacebookPermissionSet> GetUserPermission()
        {
            var requestUrl = String.Format("https://graph.facebook.com/me/permissions?access_token={0}", Uri.EscapeDataString(_userAccessToken));
            var perms = GetAndSerialize<DataHolder<FacebookPermissionSet>>(requestUrl);
            return perms.data;
        }

        protected T GetAndSerialize<T>(string requestUrl)
        {
            
            var request = WebRequest.Create(requestUrl);
            return request.GetFromJson<T>();
//            T fbEntity;
//
//            using (var response = request.GetResponse())
//            {
//                using (var responseStream = response.GetResponseStream())
//                {
//
//                    var reader = new StreamReader(responseStream);
//                    var json = reader.ReadToEnd();
//                    fbEntity = serializer.Deserialize<T>(json);
//                }
//            }
//
//            return fbEntity;
        }

//        protected T GetAndSerialize<T>(string requestUrl)
//        {
//            var serializer = new JavaScriptSerializer();
//            return GetAndSerialize<T>(requestUrl);
//        }
    }
}
