using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace PostaFlya.Application.Domain.Flier
{
    public interface FlierPrintImageServiceInterface
    {
        Image GetPrintImageForFlierWithTearOffs(string flierId);
        Image GetQrCodeImageForFlier(string flierId);
        Image GetPrintImageForFlier(string flierId, FlierPrintImageServiceQrLocation qrLocation);   
    }

    public enum FlierPrintImageServiceQrLocation
    {
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight
    }
    
}
