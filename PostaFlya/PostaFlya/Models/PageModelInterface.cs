namespace PostaFlya.Models
{
    public static class PageModelInterfaceExtensions
    {

    }

    public interface PageModelInterface
    {
        string PageId { get; set; }
        string ActiveNav { get; set; }
    }

}