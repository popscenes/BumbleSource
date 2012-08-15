using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebSite.Common.ActionResult
{

    public class WriteToStreamFileResult : FileResult
    {

        /// <summary>
        /// Gets the Action That will be used to write to the stream
        /// </summary>
        /// 
        /// <returns>
        /// The file stream.
        /// </returns>
        public Action<Stream> WriteAction { get; set; }

        /// <summary>
        /// Initializes a new instance of the WriteToStreamFileResult class.
        /// </summary>
        /// <param name="writeAction">the Action That will be used to write to the output stream</param><param name="contentTyp">The content type to use for the response.</param><exception cref="T:System.ArgumentNullException">The <paramref name="fileStream"/> parameter is null.</exception>
        public WriteToStreamFileResult(Action<Stream> writeAction, string contentTyp)
            : base(contentTyp)
        {
            if (writeAction == null)
                throw new ArgumentNullException("WriteAction");
            this.WriteAction = writeAction;
        }

        /// <summary>
        /// Uses the action to write the file to the response.
        /// </summary>
        /// <param name="response">The response.</param>
        protected override void WriteFile(HttpResponseBase response)
        {
            WriteAction(response.OutputStream);
        }
    }
}
