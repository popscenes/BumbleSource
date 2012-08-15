using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WebSite.Azure.Common.Sql
{
    public class SqlConnectionFactory
    {
        private readonly SqlConnection _sourceConnection;
        public SqlConnectionFactory(SqlConnection sourceConnection)
        {
            var connToClone = sourceConnection as ICloneable;
            _sourceConnection = connToClone.Clone() as SqlConnection;
            _sourceConnection.Close();
        }

        public SqlConnection GetConnection()
        {
            var connToClone = _sourceConnection as ICloneable;
            return connToClone.Clone() as SqlConnection;
        }
    }
}
