using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Website.Application.Content
{
    internal static class ZXingQrCode
    {
        public static Image QrCodeImg(string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            var options = new EncodingOptions()
            {
                Height = size,
                Width = size,
                Margin = margin
            };
            options.Hints.Add(EncodeHintType.ERROR_CORRECTION, FromQrCodeErrorCorrectionLevel(errorCorrectionLevel));
            var writer = new BarcodeWriter()
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = options,
                    Encoder = new MultiFormatWriter(){}
                };
            return writer.Write(data);
        }

        private static ErrorCorrectionLevel FromQrCodeErrorCorrectionLevel(QrCodeErrorCorrectionLevel errorCorrectionLevel)
        {
            switch (errorCorrectionLevel)
            {
                case QrCodeErrorCorrectionLevel.High:
                    return ErrorCorrectionLevel.H;
                case QrCodeErrorCorrectionLevel.Medium:
                    return ErrorCorrectionLevel.M;
                case QrCodeErrorCorrectionLevel.Low:
                    return ErrorCorrectionLevel.L;
                case QrCodeErrorCorrectionLevel.QuiteGood:
                    return ErrorCorrectionLevel.Q;
            }
            return ErrorCorrectionLevel.L;
        }
    }

    public class ZXingQrCodeService : QrCodeServiceInterface
    {
        public Image QrCodeImg(string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            return ZXingQrCode.QrCodeImg(data, size, margin, errorCorrectionLevel);
        }
    }
}
