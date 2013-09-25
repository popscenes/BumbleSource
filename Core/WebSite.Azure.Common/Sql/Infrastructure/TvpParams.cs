using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public static class DynamicParameters
    {
        public static Action<SqlCommand> AddBoolParam(string param, bool value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.Bit);
                p.Direction = ParameterDirection.Input;
                p.Value = value;
            };
        }

        public static Action<SqlCommand> AddBoolParam(string param, bool? value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.Bit);
                p.Direction = ParameterDirection.Input;
                p.Value = value;
                p.IsNullable = true;
            };
        }

        public static Action<SqlCommand> AddIntParam(string param, int value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.Int);
                p.Direction = ParameterDirection.Input;
                p.Value = value;
            };
        }

        public static Action<SqlCommand> AddIntParam(string param, int? value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.Int);
                p.Direction = ParameterDirection.Input;
                p.IsNullable = true;
                p.Value = value;
            };
        }

        public static Action<SqlCommand> AddStringParam(string param, string value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.NVarChar);
                p.Direction = ParameterDirection.Input;
                p.IsNullable = true;
                p.Value = value;
            };
        }

        public static Action<SqlCommand> AddDateTimeParam(string param, DateTime value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.DateTime2);
                p.Direction = ParameterDirection.Input;
                p.Value = value;
            };
        }

        public static Action<SqlCommand> AddDateTimeParam(string param, DateTime? value)
        {
            return command =>
            {
                var p = command.Parameters.Add("@" + param, SqlDbType.DateTime2);
                p.Direction = ParameterDirection.Input;
                p.IsNullable = true;
                p.Value = value;
            };
        }
    }

    public class IntTvpParam : Dapper.SqlMapper.IDynamicParameters
    {
        private readonly IEnumerable<int> _numbers;
        private readonly string _parameter;
        private readonly Action<SqlCommand> _commandExtraConfig;

        public IntTvpParam(IEnumerable<int> numbers, string parameter, Action<SqlCommand> commandExtraConfig)
        {
            this._numbers = numbers;
            _parameter = parameter;
            _commandExtraConfig = commandExtraConfig;
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var sqlCommand = (SqlCommand) command;
            sqlCommand.CommandType = CommandType.StoredProcedure;

            if(!_numbers.Any())
            {
                // Add the table parameter.
                var p = sqlCommand.Parameters.Add("@" + _parameter, SqlDbType.Structured);
                p.Direction = ParameterDirection.Input;
                p.TypeName = "int_list_type";
                p.IsNullable = true;
                p.Value = null;
            }
            else
            {
                var number_list = new List<Microsoft.SqlServer.Server.SqlDataRecord>();

                // Create an SqlMetaData object that describes our table type.
                Microsoft.SqlServer.Server.SqlMetaData[] tvp_definition =
                {
                    new Microsoft.SqlServer.Server.SqlMetaData("n", SqlDbType.Int)
                };

                foreach (int n in _numbers)
                {
                    // Create a new record, using the metadata array above.
                    var rec = new Microsoft.SqlServer.Server.SqlDataRecord(tvp_definition);
                    rec.SetInt32(0, n); // Set the value.
                    number_list.Add(rec); // Add it to the list.
                }

                // Add the table parameter.
                var p = sqlCommand.Parameters.Add("@" + _parameter, SqlDbType.Structured);
                p.Direction = ParameterDirection.Input;
                p.TypeName = "int_list_type";
                p.Value = number_list;
            }



            if (_commandExtraConfig != null)
                _commandExtraConfig(sqlCommand);

        }
    }

    public class StringTvpParam : Dapper.SqlMapper.IDynamicParameters
    {
        readonly IEnumerable<string> _strings;
        private readonly string _parameter;
        private readonly Action<SqlCommand> _commandExtraConfig;

        public StringTvpParam(IEnumerable<string> strings, string parameter, Action<SqlCommand> commandExtraConfig)
        {
            this._strings = strings;
            _parameter = parameter;
            _commandExtraConfig = commandExtraConfig;
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var sqlCommand = (SqlCommand)command;
            sqlCommand.CommandType = CommandType.StoredProcedure;

            if (!_strings.Any())
            {
                // Add the table parameter.
                var p = sqlCommand.Parameters.Add("@" + _parameter, SqlDbType.Structured);
                p.Direction = ParameterDirection.Input;
                p.TypeName = "string_list_type";
                p.IsNullable = true;
                p.Value = null;
            }
            else
            {
                var string_list = new List<Microsoft.SqlServer.Server.SqlDataRecord>();

                // Create an SqlMetaData object that describes our table type.
                Microsoft.SqlServer.Server.SqlMetaData[] tvp_definition = { new Microsoft.SqlServer.Server.SqlMetaData("s", SqlDbType.NVarChar, -1) };

                foreach (string s in _strings)
                {
                    // Create a new record, using the metadata array above.
                    var rec = new Microsoft.SqlServer.Server.SqlDataRecord(tvp_definition);
                    rec.SetString(0, s);    // Set the value.
                    string_list.Add(rec);      // Add it to the list.
                }

                // Add the table parameter.
                var p = sqlCommand.Parameters.Add("@" + _parameter, SqlDbType.Structured);
                p.Direction = ParameterDirection.Input;
                p.TypeName = "string_list_type";
                p.Value = string_list; 
            }



            if (_commandExtraConfig != null)
                _commandExtraConfig(sqlCommand);

        }


    }
}