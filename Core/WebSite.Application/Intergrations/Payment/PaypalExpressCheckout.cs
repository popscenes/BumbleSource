using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Website.Application.Intergrations.Payment
{
    public class PaypalExpressCheckout
    {
        public String ApiEndpoint { get; set; }
        public String Url { get; set; }
        public String Name { get; set; }
        public String Password { get; set; }
        public String Signiture { get; set; }
        public String Version { get; set; }
        public String CallbackUrl { get; set; }
        public String CancelUrl { get; set; }


        public NameValueCollection SetExpressCheckout(String paymentAmount)
        {
            String arguments =
                "NOSHIPPING=1&ALLOWNOTE=0&HDRIMG=http://www.eatnow.com.au/img/eatnow-logo-takeaway-home-delivery1.png&PAYMENTREQUEST_0_PAYMENTACTION=Sale&PAYMENTREQUEST_0_CURRENCYCODE=AUD&PAYMENTREQUEST_0_DESC=Eat Now Food Order&PAYMENTREQUEST_0_ITEMAMT=" +
                paymentAmount + "&PAYMENTREQUEST_0_AMT=" + paymentAmount + "&RETURNURL=" + CallbackUrl + "&CANCELURL=" +
                CancelUrl;
            return MakeCallToPaypal("SetExpressCheckout", arguments);
        }

        public String GetPayPalOrderUrl(String Token)
        {
            return Url + Token;
        }


        public NameValueCollection GetExpressCheckoutDetails(String token)
        {
            String arguments = "TOKEN=" + token + "&METHOD=GetExpressCheckoutDetails";
            return MakeCallToPaypal("GetExpressCheckoutDetails", arguments);
        }


        public NameValueCollection DoExpressCheckoutPayment(String token, String payerId, String paymentAmount)
        {
            String arguments =
                "PAYMENTREQUEST_0_PAYMENTACTION=Sale&PAYMENTREQUEST_0_CURRENCYCODE=AUD&PAYMENTREQUEST_0_DESC=Eat Now Food Order&PAYMENTREQUEST_0_ITEMAMT=" +
                paymentAmount + "&PAYMENTREQUEST_0_AMT=" + paymentAmount + "&TOKEN=" + token + "&PAYERID=" + payerId;
            return MakeCallToPaypal("DoExpressCheckoutPayment", arguments);
        }


        public NameValueCollection RefundTransaction(String transactionId)
        {
            String arguments = "TRANSACTIONID=" + transactionId;
            return MakeCallToPaypal("RefundTransaction", arguments);
        }


        public bool IsValidResult(Dictionary<String, String> result)
        {
            if (result == null)
            {
                return false;
            }

            var strAck = result["ACK"];
            return strAck != null && strAck.Equals("Success", StringComparison.CurrentCultureIgnoreCase);
        }

        public NameValueCollection  MakeCallToPaypal(String methodName, String nvpStr)
        {

            String agent = "Mozilla/4.0";
            Dictionary<String, String> nvp = null;

            //deformatNVP( nvpStr );	
            String encodedData = "METHOD=" + methodName + "&VERSION=" + Version + "&PWD=" + Password + "&USER=" + Name +
                                 "&SIGNATURE=" + Signiture + "&" + nvpStr;

            var request = WebRequest.Create(Url + encodedData);
            ((HttpWebRequest) request).UserAgent = agent;

            var response = request.GetResponse();
            var dataStream = response.GetResponseStream();
            if (dataStream == null)
            {
                return null;
            }

            var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();

            var paypalResponse =  HttpUtility.ParseQueryString(responseFromServer);
            return paypalResponse;

        }
    }
}
