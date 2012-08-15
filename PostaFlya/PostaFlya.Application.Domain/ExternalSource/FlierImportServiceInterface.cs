using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Application.Domain.ExternalSource
{
    public interface FlierImportServiceInterface
    {
        FlierImporterInterface GetImporter(string providerName);
    }
}
