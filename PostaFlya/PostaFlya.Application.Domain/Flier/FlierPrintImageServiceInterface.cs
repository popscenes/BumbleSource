using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace PostaFlya.Application.Domain.Flier
{
    public interface FlierPrintImageServiceInterface
    {
        Image GetPrintImageForFlier(string flierId);
    }
}
