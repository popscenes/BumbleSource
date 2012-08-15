using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WebSite.Azure.Common.Tests.Sql
{
    public class SqlOpenClose : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly SqlCommand _command;

        public SqlOpenClose(SqlConnection connection)
        {
            _connection = connection;
            _command = connection.CreateCommand();
            _connection.Open();
        }

        public SqlCommand Cmd { get { return _command; } }

        public void Dispose()
        {
            _connection.Close();
            _command.Dispose();
        }
    }
}
