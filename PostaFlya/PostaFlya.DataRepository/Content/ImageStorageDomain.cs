using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.TaskJob;

namespace PostaFlya.DataRepository.Content
{
    internal class ImageTableEntry : TableServiceEntity
        , StorageTableEntryInterface<ImageInterface>
    {
        public string Id { get; set; }
        public int Version { get; set; }
        public string BrowserId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }


        public void Update(ImageInterface source)
        {            
            Id = source.Id;
            Version = source.Version;

            var loc = source.Location ?? new Location();
            Title = source.Title;
            BrowserId = source.BrowserId;
            Status = source.Status.ToString();
            Longitude = loc.Longitude;
            Latitude = loc.Latitude;

//just ensure we are serializing everything correctly for tests
#if DEBUG
            UpdateEntity(source);
#endif
        }

        public void UpdateEntity(ImageInterface imageStorage)
        {
            imageStorage.Id = Id;
            imageStorage.Version = Version;

            imageStorage.Title = Title;
            imageStorage.BrowserId = BrowserId;

            var status = ImageStatus.Failed;
            Enum.TryParse(Status, true, out status);
            imageStorage.Status = status;

            if (Longitude.HasValue && Latitude.HasValue)
                imageStorage.Location = new Location(Longitude.Value, Latitude.Value);
        }

        public int PartitionClone { get; set; }
        public bool KeyChanged { get; set; }
    }

    internal class ImageStorageDomain : 
        StorageDomainEntityBase<ImageStorageDomain, ImageTableEntry, ImageInterface, Domain.Content.Image>
        
    {
        public static TableNameAndPartitionProvider<ImageInterface>
            TableNamesAndPartition = new TableNameAndPartitionProvider<ImageInterface>()
                                                         {
                                                           {typeof(ImageTableEntry), IdPartition, "image", i => i.Id, i => i.Id},    
                                                           {typeof(ImageTableEntry), BrowserPartition, "image", i => i.BrowserId, i => i.Id}
                                                         };


        public const int BrowserPartition = 1;

        protected internal ImageStorageDomain(AzureTableContext tableContext)
            : base(TableNamesAndPartition, tableContext)
        {
        }

        protected internal ImageStorageDomain(ImageInterface source, AzureTableContext tableContext)
            : base(TableNamesAndPartition, tableContext)
        {
            this.DomainEntity.CopyFieldsFrom(source);
            ClonedTable.CreateDefaultEntries();
        }

        public ImageStorageDomain() : base(TableNamesAndPartition)
        {

        }

        public static IQueryable<ImageInterface> GetByBrowserId(string browserId, AzureTableContext tableContext)
        {
            var imgTableEntity = tableContext.PerformQuery<ImageTableEntry>(te => te.PartitionKey == browserId)
                        .AsEnumerable();

            return imgTableEntity.Select(ts => ts.CreateEntityCopy<Image, ImageInterface>())
                .AsQueryable();
        }

    }
}