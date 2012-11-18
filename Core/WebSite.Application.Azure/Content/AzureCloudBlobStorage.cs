using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Website.Application.Content;

namespace Website.Application.Azure.Content
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


            Func<byte[]> getBlob = () =>
                {
                    var ms = new MemoryStream();
                    _blobContainer.GetBlockBlobReference(id).DownloadToStream(ms);
                    return ms.ToArray();
                };

                //.DownloadByteArray();
            return RetryQuery(getBlob);
        }

        public bool GetToStream(string id, Stream s)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            Func<bool> getBlob = () =>
            {
                _blobContainer.GetBlockBlobReference(id).DownloadToStream(s);
                                           return true;
            };
            return RetryQuery(getBlob);
        }

        public Uri GetBlobUri(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _blobContainer.GetBlockBlobReference(id).Uri;
        }

        public bool Exists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            Func<bool> getBlob = () => _blobContainer.GetBlockBlobReference(id).Exists();
            return RetryQuery(getBlob);
        }

        public Application.Content.BlobProperties GetBlobProperties(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            Func<Application.Content.BlobProperties> getProperties = 
                () =>
                    {
                        var blob = _blobContainer.GetBlockBlobReference(id);
                        blob.FetchAttributes();
                        return new Application.Content.BlobProperties()
                                    {
                                        MetaData = new Dictionary<string, string>(blob.Metadata),
                                        ContentTyp = blob.Properties.ContentType
                                    };
                    };

            return RetryQuery(getProperties);
        }

        public bool SetBlob(string id, byte[] bytes, Application.Content.BlobProperties properties = null)
        {
            Func<bool> setBlob = 
                () =>
                    {
                        //var blob = _blobContainer.GetBlobReferenceFromServer(id);
                        var blob = _blobContainer.GetBlockBlobReference(id);
                        UpdateFromProperties(blob, properties);
                        var ms = new MemoryStream(bytes);
                        blob.UploadFromStream(ms);
                        return true;
                    };
            return RetryQuery(setBlob);
        }

        public bool SetBlobProperties(string id, Application.Content.BlobProperties properties)
        {
            Func<bool> setBlob =
                () =>
                    {
                        var blob = _blobContainer.GetBlockBlobReference(id);
                        UpdateFromProperties(blob, properties, true);
                        return true;
                    };
            return RetryQuery(setBlob); 
        }

        public bool DeleteBlob(string id)
        {
            Func<bool> deleteBlob = () =>         
                _blobContainer.GetBlockBlobReference(id).DeleteIfExists();
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
            Func<bool> create = () => _blobContainer.CreateIfNotExists();
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

        private static void UpdateFromProperties(ICloudBlob blob, Application.Content.BlobProperties properties, bool update = false)
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
                foreach (var metaData in properties.MetaData)
                {
                    blob.Metadata.Remove(metaData.Key);
                    blob.Metadata.Add(metaData.Key, metaData.Value);
                }
                

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
                //catch (StorageClientException ex)
                catch (StorageException ex)
                {
                    var reqInfo = ex.RequestInformation;
                    
                    if (reqInfo.ExtendedErrorInformation == null || !reqInfo.ExtendedErrorInformation.ErrorCode.Equals(
                        StorageErrorCodeStrings.InternalError.ToString(CultureInfo.InvariantCulture)))
                    {
                        canRetry = false;

                        if (!GracefulFailCodes.Contains((HttpStatusCode) reqInfo.HttpStatusCode))
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
