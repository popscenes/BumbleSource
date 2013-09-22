using System.Data.SqlClient;
using PostaFlya.DataRepository.Binding;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Messaging;

namespace PostaFlya.DataRepository.Search.Command
{
    public class ReindexFlierCommandHandler : MessageHandlerInterface<ReindexFlierCommand>
    {
        private readonly SqlConnection _connection;

        public ReindexFlierCommandHandler([SqlSearchConnectionString]string searchDbConnectionString)
        {
            _connection = new SqlConnection(searchDbConnectionString);
        }

        public void Handle(ReindexFlierCommand command)
        {
            var test = command.UpdateOrDelete 
                ? SqlExecute.InsertOrUpdate(command.SearchRecord, _connection)
                : SqlExecute.Delete(command.SearchRecord, _connection);
        }
    }
}