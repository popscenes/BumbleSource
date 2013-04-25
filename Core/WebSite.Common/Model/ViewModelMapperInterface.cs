namespace Website.Common.Model
{
    public interface ViewModelMapperInterface<ViewModelType, in SourceType>
    {
        ViewModelType ToViewModel(ViewModelType target, SourceType source);
    }
}