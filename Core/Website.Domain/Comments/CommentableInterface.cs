namespace Website.Domain.Comments
{
    public interface CommentableInterface
    {
        int NumberOfComments { get; set; }
    }

    public static class CommentableInterfaceExtensions
    {
        public static void CopyFieldsFrom(this CommentableInterface target, CommentableInterface source)
        {
            target.NumberOfComments = source.NumberOfComments;
        }
    }
}
