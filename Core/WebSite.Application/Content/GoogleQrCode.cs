using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Website.Application.Content
{
    internal static class GoogleQrCode
    {
        public static Image QrCodeImg(string data, int size = 80, int margin = 4,
                                      QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            var url = QrCodeUrl(data, size, margin, errorCorrectionLevel);
            try
            {
                var wc = new WebClient();
                using (var stream = wc.OpenRead(url))
                {
                    return Image.FromStream(stream);
                }              
            }
            catch (Exception e)
            {
                Trace.TraceError("Qr code error {0} : {1}", e.Message, e.StackTrace);
                return null;
            }       
        }

        public static String QrCodeUrl(string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");
            if (margin < 0)
                throw new ArgumentOutOfRangeException("margin", margin, "Must be greater than or equal to zero.");
            if (!Enum.IsDefined(typeof(QrCodeErrorCorrectionLevel), errorCorrectionLevel))
                throw new InvalidEnumArgumentException("errorCorrectionLevel", (int)errorCorrectionLevel, typeof(QrCodeErrorCorrectionLevel));

            var ret = String.Format("http://chart.apis.google.com/chart?cht=qr&chld={2}|{3}&chs={0}x{0}&chl={1}", size, HttpUtility.UrlEncode(data), errorCorrectionLevel.ToString()[0], margin);
            return ret;
        }
    }

    public class GoogleQrCodeService : QrCodeServiceInterface
    {
        public Image QrCodeImg(string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            return GoogleQrCode.QrCodeImg(data, size, margin, errorCorrectionLevel);
        }
    }


}
