using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace WebSite.Test.Common.Facebook
{
    public static class Facebookutils
    {
        public class DataHolder<T>
        {
            public List<T> data { get; set; }
        }


        public static string DEV_APP_ACCESSTOKEN = "180085418789276|XXRb7U189ljKwt-InHPlbyA9b1c";
        public static string DEV_APP_ID = "180085418789276";

        public static void TestUserDelete(string userID)
        {
            var requestUrl = String.Format("https://graph.facebook.com/{0}?method=delete&access_token={1}", userID, DEV_APP_ACCESSTOKEN);
            var request = WebRequest.Create(requestUrl);
            using (request.GetResponse()) { }
        }

        public static void TestEventAdd(string accessToken)
        {
            var requestUrl = String.Format("https://graph.facebook.com/me/events?method=post&access_token={0}&name={1}&start_time={2}&end_time={3}&location={4}&description={5}&location_id={6}&privacy_type={7}",
                accessToken,
                "Test Event 1",
                DateTime.Now.AddDays(2).ToString("yyyy-MM-ddThh:mm"),
                DateTime.Now.AddDays(3).ToString("yyyy-MM-ddThh:mm"),
                "ya mums",
                "this is a test event yo",
                "185187880329",
                "OPEN");

            var request = WebRequest.Create(requestUrl);
            using (request.GetResponse()){}
          
        }

        public static void TestUserAdd(string userName, string permissions)
        {
            var requestUrl = String.Format("https://graph.facebook.com/{0}/accounts/test-users?installed=true&name={1}&locale=en_US&permissions={2}&method=post&access_token={3}",
                Uri.EscapeDataString(DEV_APP_ID),
                Uri.EscapeDataString(userName),
                Uri.EscapeDataString(permissions),
                Uri.EscapeDataString(DEV_APP_ACCESSTOKEN)
            );

            var request = WebRequest.Create(requestUrl);
            using (request.GetResponse()) {}
        }

        public static FacebookTestUser TestUserGet()
        {
            var requestUrl = String.Format("https://graph.facebook.com/{0}/accounts/test-users?access_token={1}", Uri.EscapeDataString(DEV_APP_ID), Uri.EscapeDataString(DEV_APP_ACCESSTOKEN));
            var request = WebRequest.Create(requestUrl);

            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var serializer = new JavaScriptSerializer();
                    var reader = new StreamReader(responseStream);
                    var json = reader.ReadToEnd();

                    var testUserList = serializer.Deserialize<DataHolder<FacebookTestUser>>(json);
                    return testUserList.data.First();
                }
            }
        }
    }
}
