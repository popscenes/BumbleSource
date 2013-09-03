using System;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Website.Application.Domain.Content
{
    public class WebImageRequestContentRetriever : RequestContentRetrieverInterface
    {
        private String _lastError = "";
        private readonly HttpContextBase _contextBase;
        public WebImageRequestContentRetriever(HttpContextBase contextBase)
        {
            _contextBase = contextBase;
        }

        #region Implementation of RequestContentRetrieverInterface

        public Website.Domain.Content.Content GetContent()
        {
            var ret = new Website.Domain.Content.Content { Type = Website.Domain.Content.Content.ContentType.Image };

            if (_contextBase.Request == null || _contextBase.Request.Files == null || _contextBase.Request.Files.Count == 0
                || _contextBase.Request.Files[0] == null)
            {
                _lastError = "No Image File Uploaded";
                return ret;
            }

            if(_contextBase.Request.Files[0].ContentLength > 3000000)
            {
                _lastError = "Image File Too Large";
                return ret;
            }

            try
            {
//                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(_contextBase.Request.Files[0].InputStream))
                {
                    ret.Data = binaryReader.ReadBytes(_contextBase.Request.Files[0].ContentLength);
                    ret.OriginalFileName = _contextBase.Request.Files[0].FileName;
                    return ret;                  
                }

//                using(var img = Image.FromStream(_contextBase.Request.Files[0].InputStream))
//                {
//                    ret.Data = img.GetBytes();
//                    return ret;
//                }
            }
            catch (Exception e)
            {
                _lastError = "Failed to read Image Data";
                Trace.TraceError("WebImageRequestContentRetriever Error: {0}, Stack {1}", e.Message, e.StackTrace);
            }

            return ret;
        }

        public string GetLastError()
        {
            return _lastError;
        }

        #endregion
    }
}