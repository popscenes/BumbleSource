namespace PostaFlya.Application.Domain.ExternalSource
{
    public interface FlierImportServiceInterface
    {
        FlierImporterInterface GetImporter(string providerName);
    }
}
