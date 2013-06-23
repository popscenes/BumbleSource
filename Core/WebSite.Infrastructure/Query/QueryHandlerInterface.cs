namespace Website.Infrastructure.Query
{
    public interface QueryHandlerInterface
    {
        
    }
    public interface QueryHandlerInterface<in QueryType, out ReturnType> : QueryHandlerInterface 
        where QueryType : QueryInterface
    {
        ReturnType Query(QueryType argument);
    }
}