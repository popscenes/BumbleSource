using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Application.Content;
using BlobProperties = WebSite.Application.Content.BlobProperties;

namespace WebSite.Application.Azure.Content
{
    public class AzureCloudBlobStorage : BlobStorageInterface
    {
        private readonly CloudBlobContainer _blobContainer;

        public AzureCloudBlobStorage(CloudBlobContainer blobContainer)
        {
            _blobContainer = blobContainer;
        }

        #region Implementation of BlobStorageInterface

        public byte[] GetBlob(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            Func<byte[]> getBlob = () => _blobContainer.GetBlobReference(id).DownloadByteArray();
            return RetryQuery(getBlob);
        }

        public bool GetToStream(string id, Stream s)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            Func<bool> getBlob = () => { _blobContainer.GetBlobReference(id).DownloadToStream(s);
                                           return true;
            };
            return RetryQuery(getBlob);
        }

        public Uri GetBlobUri(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _blobContainer.GetBlobReference(id).Uri;
        }

        public bool Exists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            Func<bool> getBlob = () =>
            {
                _blobContainer.GetBlobReference(id).FetchAttributes();
                return true;
            };
            return RetryQuery(getBlob);
        }

        public BlobProperties GetBlobProperties(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            Func<BlobProperties> getProperties = 
                () =>
                    {
                        var blob = _blobContainer.GetBlobReference(id);
                        blob.FetchAttributes();
                        return new BlobProperties()
                                    {
                                        MetaData = new NameValueCollection(blob.Metadata),
                                        ContentTyp = blob.Properties.ContentType
                                    };
                    };

            return RetryQuery(getProperties);
        }

        public bool SetBlob(string id, byte[] bytes, BlobProperties properties = null)
        {
            Func<bool> setBlob = 
                () =>
                    {
                        var blob = _blobContainer.GetBlobReference(id);
                        UpdateFromProperties(blob, properties);
                        blob.UploadByteArray(bytes); return true;
                    };
            return RetryQuery(setBlob);
        }

        public bool SetBlobProperties(string id, BlobProperties properties)
        {
            Func<bool> setBlob =
                () =>
                    {
                        var blob = _blobContainer.GetBlobReference(id);
                        UpdateFromProperties(blob, properties, true);
                        return true;
                    };
            return RetryQuery(setBlob); 
        }

        public bool DeleteBlob(string id)
        {
            Func<bool> deleteBlob = () => _blobContainer.GetBlobReference(id).DeleteIfExists();
            return RetryQuery(deleteBlob);
        }

        public int BlobCount
        {
            get
            {
                Func<int> blobCount = () => _blobContainer.ListBlobs().Count();
                return RetryQuery(blobCount);
            }
        }

        #endregion

        public bool CreateIfNotExists()
        {
            Func<bool> create = () => _blobContainer.CreateIfNotExist();
            return RetryQuery(create);
        }

        public bool Exists()
        {
            Func<bool> exists = () =>
            {
                _blobContainer.FetchAttributes();
                return true;
            };
            return RetryQuery(exists);
        }

        public bool Delete()
        {
            Func<bool> delete = () =>
            {
                _blobContainer.Delete();
                return true;
            };
            return RetryQuery(delete);
        }

        private static void UpdateFromProperties(CloudBlob blob, BlobProperties properties, bool update = false)
        {
            if (properties == null) return;
            
            if (properties.ContentTyp != null)
            {
                blob.Properties.ContentType = properties.ContentTyp;
                if(update)
                    blob.SetProperties();
            }
                    
            if (properties.MetaData != null)
            {
                blob.Metadata.Add(properties.MetaData);
                if (update)
                    blob.SetMetadata();
            }
        }

        private readonly static HashSet<HttpStatusCode> GracefulFailCodes = new HashSet<HttpStatusCode>()
                                                                       {
                                                                           HttpStatusCode.NotFound,
                                                                           //HttpStatusCode.BadRequest
                                                                       };

        public static ReturnType RetryQuery<ReturnType>(Func<ReturnType> action)
        {
            //see msdn doc for StorageClientException 
            var canRetry = true;
            while (canRetry) 
            {
                try
                {
                    return action();
                }
                catch (StorageClientException ex)
                {
                    if (ex.ExtendedErrorInformation == null || !ex.ExtendedErrorInformation.ErrorCode.Equals(
                        StorageErrorCodeStrings.InternalError.ToString(CultureInfo.InvariantCulture)))
                    {
                        canRetry = false;

                        if(!GracefulFailCodes.Contains(ex.StatusCode))
                            Trace.TraceError("AzureStorageClient Error: {0}, Stack {1}", ex.Message, ex.StackTrace);
                    }
                    else 
                    {
                        // Wait before retrying the operation
                        //Trace
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
            //Trace
            return default(ReturnType);
        }
    }
}
