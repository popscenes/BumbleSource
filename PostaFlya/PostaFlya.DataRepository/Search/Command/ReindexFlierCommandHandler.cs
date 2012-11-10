using System.Data.SqlClient;
using PostaFlya.DataRepository.Binding;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Command;

namespace PostaFlya.DataRepository.Search.Command
{
    public class ReindexFlierCommandHandler : CommandHandlerInterface<ReindexFlierCommand>
    {
        private readonly SqlConnection _connection;

        public ReindexFlierCommandHandler([SqlSearchConnectionString]string searchDbConnectionString)
        {
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public object Handle(ReindexFlierCommand command)
        {
            return command.UpdateOrDelete 
                ? SqlExecute.InsertOrUpdate(command.SearchRecord, _connection)
                : SqlExecute.Delete(command.SearchRecord, _connection);
        }
    }
}