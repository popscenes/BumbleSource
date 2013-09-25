using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading;
using NLog;
using Website.Infrastructure.Execution;

namespace Website.Azure.Common.Sql.Infrastructure
{

    public class AdoExecutionContext : IDisposable
    {
        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }

        public static string IdFor(ExecutionEnvironment env)
        {
            return "AdoExecutionContext" + env.Id;
        }

        public void Dispose()
        {
            if (Transaction != null)
                Transaction.Dispose();

            if (Connection != null)
                Connection.Dispose();
        }
    }

    public static class AdoActionExtensions
    {

        public static void ExecuteInEnvironment(this Action<IDbConnection> action, ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            Action sqlAct = () =>
            {
                using (var conn = GetConnectionForEnvironment(env, defaultFactory))
                {
                    action(conn);
                }

            };

            sqlAct.ExecuteSqlWithRetries();
        }

        public static void ExecuteInContext(this Action<AdoExecutionContext> action, AdoExecutionContext context)
        {
            Action sqlAct = () => action(context);
            sqlAct.ExecuteSqlWithRetries();
        }

        public static TRet ExecuteInEnvironment<TRet>(this Func<IDbConnection, TRet> action, ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            var ret = default(TRet);
            Action sqlAct = () =>
            {
                using (var conn = GetConnectionForEnvironment(env, defaultFactory))
                {
                    ret = action(conn);
                }

            };

            sqlAct.ExecuteSqlWithRetries();
            return ret;
        }

        public static IDbConnection GetConnectionForEnvironment(this ExecutionEnvironment env, Func<IDbConnection> defaultFactory = null)
        {
            if (env == null && defaultFactory == null) return null;

            var connection = env == null ? defaultFactory() : new SqlConnection(env.ConnectionInfo);
            if (connection == null) return null;
            connection.Open();
            return connection;
        }
    }

    public static class SqlExecute
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(SqlExecute).Name);

        public static bool ExecuteSqlWithRetries(this Action tryact)
        {
            return ExecuteSqlActionWithRetries(tryact);
        }

        public static bool ExecuteSqlActionWithRetries(Action tryact)
        {
            var retry = 0;
            do
            {
                try
                {
                    tryact();
                    retry = 0;
                }
                catch (SqlException ex)
                {
                    retry = HandleSqlException(ex, retry);
                    
                }

            } while (retry > 0);


            return retry == 0;
        }

        private static readonly HashSet<int> RetryErrors = new HashSet<int>()
                                                                         {
                                                                            10054,// Connection reset by peer
                                                                            10053, // Software caused connection abort
                                                                            1205, // Deadlock
                                                                         };
        private const int SqlRetryMax = 3;
        private const int RetryDelay = 10;
        private static int HandleSqlException(SqlException ex, int retryNumber = 0)
        {

            if (RetryErrors.Contains(ex.Number) && retryNumber < SqlRetryMax)
            {
                Thread.Sleep(RetryDelay);
                return retryNumber + 1;
            }
                

            var errorMessages = new StringBuilder("SQL Error ");
            if (retryNumber == SqlRetryMax)
                errorMessages.Append("giving up after " + SqlRetryMax + " retries");
            errorMessages.Append("\n");

            for (int i = 0; i < ex.Errors.Count; i++)
            {
                errorMessages.Append("Index #" + i + "\n" +
                    "Message: " + ex.Errors[i].Message + "\n" +
                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                    "Source: " + ex.Errors[i].Source + "\n" +
                    "Procedure: " + ex.Errors[i].Procedure + "\n");
            }

            errorMessages.Append("Local Stack: \n");
            var trace = new StackTrace();
            errorMessages.Append(trace.ToString());

            Logger.ErrorException(errorMessages.ToString(), ex);
            throw ex;
        }
    }
}
