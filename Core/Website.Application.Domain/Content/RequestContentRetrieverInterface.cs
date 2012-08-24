namespace Website.Application.Domain.Content
{
    public interface RequestContentRetrieverInterface
    {
        //var image = WebImage.GetImageFromRequest();
        Website.Domain.Content.Content GetContent();
        string GetLastError();
    }
}