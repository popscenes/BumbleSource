using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Flier;

namespace PostaFlya.Application.Domain.ExternalSource
{
    public interface FlierImporterInterface
    {
        bool CanImport(BrowserInterface browser);
        IQueryable<FlierInterface> ImportFliers(BrowserInterface browser);
    }
}
