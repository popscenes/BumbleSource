using System;
using System.Data.SqlClient;

namespace WebSite.Azure.Common.Sql
{
    public class SqlOpenClose : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly SqlCommand _command;
        private readonly SqlTransaction _transaction = null;

        public SqlOpenClose(SqlConnection connection)
        {
            _connection = connection;           
            _connection.Open();
            _command = connection.CreateCommand();
//            if (singleTransaction)
//            {
//                _transaction = _connection.BeginTransaction();
//                _command.Transaction = _transaction;
//            }
        }

        public SqlCommand Cmd { get { return _command; } }

        public void Dispose()
        {
            if (_transaction != null)
                _transaction.Commit();

            _connection.Close();
            _command.Dispose();
        }
    }
}
