using System.Linq;
using PostaFlya.Domain.Flier;
using Website.Domain.Browser;

namespace PostaFlya.Application.Domain.ExternalSource
{
    public interface FlierImporterInterface
    {
        bool CanImport(BrowserInterface browser);
        IQueryable<FlierInterface> ImportFliers(BrowserInterface browser);
    }
}
