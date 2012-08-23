using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Net;
using System.Threading;
using WebSite.Azure.Common.TableStorage;

namespace WebSite.Azure.Common.DataServices
{
    public static class DataServicesQueryHelper
    {
        private const int RetryNumber = 3;
        private readonly static HashSet<HttpStatusCode> GracefulFailCodes = new HashSet<HttpStatusCode>()
                                                                       {
                                                                           HttpStatusCode.NotFound,
                                                                           //HttpStatusCode.BadRequest
                                                                       };

        private readonly static HashSet<HttpStatusCode> RetryFailCodes = new HashSet<HttpStatusCode>()
                                                                       {
                                                                           HttpStatusCode.RequestTimeout,
                                                                           HttpStatusCode.ServiceUnavailable,
                                                                           HttpStatusCode.GatewayTimeout,
                                                                           HttpStatusCode.PreconditionFailed
                                                                       };

        // using AsTableServiceQuery the CloudTableQuery handles retries etc. 
        //this means the below retrying isn't neccesary for some of the timeout codes
        //However still a common place for exception handling
        public static ReturnType QueryRetry<ReturnType>(Func<ReturnType> action) 
        {
            var tryCount  = 0;
            do 
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    if(!HandleException(e))
                        return default(ReturnType);
                        
                    if (++tryCount == RetryNumber)
                    {
                        var trace = new StackTrace();
                        Trace.TraceError("Data Query retry failed Exception {0} : Exception stack: {1}, CurrentStack {2}:", e.Message, e.StackTrace, trace.ToString());
                        return default(ReturnType); 
                        
                    }
                        
                    BackOff(tryCount - 1);              
                }
                
            }
            while(true);
        }

        private static bool HandleException(Exception e)
        {
            var ie = e.InnerException as DataServiceClientException;

            if (ie == null)
            {
                var trace = new StackTrace();
                Trace.TraceError("Data Query Exception {0} : Exception stack: {1}, CurrentStack {2}:", e.Message, e.StackTrace, trace.ToString());
                //throw
                return false; 
            }

            if (GracefulFailCodes.Contains((HttpStatusCode)ie.StatusCode))
            {
                var trace = new StackTrace();
                Trace.TraceInformation("Data Query Exception Graceful fail {0} : Exception stack: {1}, CurrentStack {2}:", ie.Message, ie.StackTrace, trace.ToString());
                return false;
            }
                

            if (!RetryFailCodes.Contains((HttpStatusCode)ie.StatusCode))
            {
                var trace = new StackTrace();
                Trace.TraceError("Data Query DataServiceClientException {0} Exception stack: {1}, CurrentStack {2}:", ie.Message, e.StackTrace, trace.ToString());
                //throw e;
                return false;
            }

            return true;
        }

        private static bool IsConcurrencyException(Exception e)
        {
            var ie = e.InnerException as DataServiceClientException;
            if (ie == null)
                return false;
            return (HttpStatusCode) ie.StatusCode == HttpStatusCode.PreconditionFailed;
        }

        public static void BackOff(int retries, int defaultBackoffMs = 2000)
        {
            var defaultBackoff = TimeSpan.FromMilliseconds(defaultBackoffMs);
            var backoffMin = TimeSpan.FromMilliseconds(300);
            var backoffMax = TimeSpan.FromSeconds(10);

            var random = new Random();

            double backoff = random.Next(
                (int)(0.8D * defaultBackoff.TotalMilliseconds),
                (int)(1.2D * defaultBackoff.TotalMilliseconds));
            backoff *= (Math.Pow(2, retries) - 1);
            backoff = Math.Min(
                backoffMin.TotalMilliseconds + backoff,
                backoffMax.TotalMilliseconds);

            Thread.Sleep((int) backoff);
        }

        public static bool SaveChangesRetryMutatorsOnConcurrencyException(this TableContextInterface context
    , List<Action> retryActions = null)
        {
            var tryCount = 0;
            do
            {
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    if (!HandleException(e))
                        return false;

                    var defaultBackoffMs = 2000;
                    if (IsConcurrencyException(e))
                    {
                        if (retryActions != null && retryActions.Count > 0)
                            retryActions.ForEach(a => a());
                        else
                            return false;
                        defaultBackoffMs = 500;
                    }


                    if (++tryCount == RetryNumber)
                        return false;
                    BackOff(tryCount - 1, defaultBackoffMs);
                }

            }
            while (true);
        }
    }
}
