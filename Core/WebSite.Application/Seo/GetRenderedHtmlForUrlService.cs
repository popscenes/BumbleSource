using System;
using System.Diagnostics;
using System.Text;

namespace Website.Application.Seo
{
    public interface GetRenderedHtmlForUrlServiceInterface
    {
        string GetHtml(string url, bool waitforbodystatus = false, int maxwaitmillis = 0);
    }

    public class GetRenderedHtmlForUrlService : GetRenderedHtmlForUrlServiceInterface
    {

        public string GetHtml(string url, bool waitforbodystatus = false, int maxwaitmillis = 0)
        {
            var outputBuilder = new StringBuilder();

            var processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = "Seo\\echo-html.js " + url + (waitforbodystatus ? " waitforstatus" : "");
            processStartInfo.FileName = "Seo\\phantomjs.exe";

            var process = new Process();
            process.StartInfo = processStartInfo;
            // enable raising events because Process does not raise events by default
            process.EnableRaisingEvents = true;
            // attach the event handler for OutputDataReceived before starting the process
            process.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    outputBuilder.Append(e.Data);
//                    if (e.Data.ToLower().Trim().EndsWith("</html>"))
//                        process.Kill();//phantomjs hangs on windows 7 update nvidea drivers 
                }
            );
            // start the process
            // then begin asynchronously reading the output
            // then wait for the process to exit
            // then cancel asynchronously reading the output
            process.Start();
            process.BeginOutputReadLine();
            if (maxwaitmillis > 0)
                process.WaitForExit(maxwaitmillis);
            else
                process.WaitForExit();

            process.CancelOutputRead();
            try
            {
                process.Kill();//phantomjs hangs on windows 7 for some reason
            }
            catch(Exception){}

            // use the output
            return outputBuilder.ToString();
        }
    }
}
