namespace Website.Infrastructure.Query
{

    public interface QueryHandlerInterface<in QueryType, out ReturnType>  
        where QueryType : QueryInterface
    {
        ReturnType Query(QueryType argument);
    }
}