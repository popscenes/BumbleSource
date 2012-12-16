using System.Drawing;

namespace Website.Application.Content
{
    public interface QrCodeServiceInterface
    {
        Image QrCodeImg(string data, int size = 80, int margin = 4,
                        QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low);
    }

    public enum QrCodeErrorCorrectionLevel
    {
        /// <summary>Recovers from up to 7% erroneous data.</summary>
        Low,
        /// <summary>Recovers from up to 15% erroneous data.</summary>
        Medium,
        /// <summary>Recovers from up to 25% erroneous data.</summary>
        QuiteGood,
        /// <summary>Recovers from up to 30% erroneous data.</summary>
        High
    }
}