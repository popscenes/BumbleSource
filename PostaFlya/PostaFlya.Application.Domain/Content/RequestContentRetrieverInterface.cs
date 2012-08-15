namespace PostaFlya.Application.Domain.Content
{
    public interface RequestContentRetrieverInterface
    {
        //var image = WebImage.GetImageFromRequest();
        PostaFlya.Domain.Content.Content GetContent();
        string GetLastError();
    }
}