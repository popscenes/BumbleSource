using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Util;

namespace Website.Application.SiteMap
{
    public class SiteMapIndexBuilder : IDisposable
    {
        private const string XmlLine = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
        private readonly string _siteUrl;
        private readonly string _siteMapFileFormat;
        private readonly TempFileStorageInterface _tempFileStorage;
        private readonly BlobStorageInterface _applicationStorage;
        private int _siteMapNumber = 0;

        private StreamWriter _currentMap;
        private int _currentCount;
        private const int MaxEntries = 50000;


        public SiteMapIndexBuilder(string siteUrl, string siteMapFileFormat, TempFileStorageInterface tempFileStorage, [ApplicationStorage]BlobStorageInterface applicationStorage)
        {
            _siteUrl = siteUrl;
            _siteMapFileFormat = siteMapFileFormat;
            _tempFileStorage = tempFileStorage;
            _applicationStorage = applicationStorage;
        }


        public void AddPath(string path)
        {
            CheckCurrentMap();
            var locElement = new XElement("loc", _siteUrl + path);
            var urlElement = new XElement("url", locElement);

            _currentMap.WriteLine(urlElement.ToString());
            _currentCount++;
        }

        private void CheckCurrentMap(bool forceClose = false)
        {
            if (_currentCount == MaxEntries || (_currentMap != null && forceClose))
            {
                _currentMap.WriteLine(@"</urlset>"); 
                _currentMap.Flush();
                _currentMap.Dispose();
                _currentMap = null;
            }

            if (_currentMap != null || forceClose) return;

            var siteFile = string.Format(_siteMapFileFormat, ++_siteMapNumber);
            _currentMap = new StreamWriter(OpenTruncate(siteFile));
            _currentMap.WriteLine(XmlLine);
            _currentMap.WriteLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");
            _currentCount = 0;
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_tempFileStorage.GetTempPath(), fileName);
        }

        private FileStream OpenTruncate(string fileName)
        {
            var file = GetFilePath(fileName);
            return File.Exists(file)
                       ? File.Open(file, FileMode.Truncate, FileAccess.Write)
                       : File.Open(file, FileMode.CreateNew, FileAccess.Write);
        }

        public void Dispose()
        {
            CheckCurrentMap(true);

            var locElement = new XElement("loc");
            var lastModified = new XElement("lastmod", DateTimeOffset.Now);
            var siteElement = new XElement("sitemap", locElement, lastModified);

            var indexFileName = string.Format(_siteMapFileFormat, "");
            using (var indexFile = new StreamWriter(OpenTruncate(indexFileName)))
            {
                indexFile.WriteLine(XmlLine);
                indexFile.WriteLine(@"<sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

                for (var i = 1; i <= _siteMapNumber; i++)
                {
                    var siteFileName = string.Format(_siteMapFileFormat, i);
                    if (!WriteFileToBlobStorage(siteFileName)) continue;
                    locElement.SetValue(_siteUrl + "/" + siteFileName);
                    indexFile.WriteLine(siteElement.ToString());
                }

                indexFile.WriteLine(@"</sitemapindex>");
                indexFile.Flush();
                indexFile.Dispose();
            }

            WriteFileToBlobStorage(indexFileName);
        }

        private bool WriteFileToBlobStorage(string fileName)
        {
            try
            {
                using (var fileStream = File.Open(GetFilePath(fileName), FileMode.Open))
                {
                    _applicationStorage.SetBlobFromStream(fileName, fileStream);
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Failed to open generated sitemap {0} : {1}", fileName, e.Message);
                return false;
            }
            return true;
        }
    }
}