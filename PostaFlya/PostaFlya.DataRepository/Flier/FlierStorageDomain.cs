using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Application.Extension.Expression;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Util;


namespace PostaFlya.DataRepository.Flier
{
    internal class FlierTableEntry : ExtendableTableEntry
        , StorageTableEntryInterface<FlierInterface>
        , HasPropertyGroupCollectionInterface
    {
        public string Id { get; set; }
        public int Version { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public double LocationLatitude { get; set; }
        public double LocationLongitude { get; set; }
        public string LocationDescription { get; set; }
        public string Status { get; set; }
        public string FlierBehaviour { get; set; }

        public Guid? Image { get; set; }
        public string BrowserId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string WebsiteTags { get; set; }
        public byte[] ImageList { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }


        public void Update(FlierInterface source)
        {
            Id = source.Id;
            Version = source.Version;
            Title = source.Title;
            Description = source.Description;
            Tags = source.Tags.ToString();

            Image = source.Image;
            BrowserId = source.BrowserId;
            EffectiveDate = source.EffectiveDate;
            CreateDate = source.CreateDate;

            LocationLatitude = source.Location.Latitude;
            LocationLongitude = source.Location.Longitude;
            LocationDescription = source.Location.Description;
            Status = source.Status.ToString();
            FlierBehaviour = source.FlierBehaviour.ToString();
            ExternalSource = source.ExternalSource;
            ExternalId = source.ExternalId;

            ImageList = SerializeUtil.ToByteArray(source.ImageList);
            

            _propertyGroupCollection = source.ExtendedProperties != null ? new PropertyGroupCollection(source.ExtendedProperties) : null;

#if DEBUG
            UpdateEntity(source);
#endif
        }

        public void UpdateEntity(FlierInterface target)
        {
            target.Title = Title;
            target.Description = Description;
            target.Tags = Tags == null ? new Tags() : new Tags(Tags);
            target.Location = new Location(LocationLongitude, LocationLatitude);
            target.Location.Description = LocationDescription;
            target.Image = Image;
            target.BrowserId = BrowserId;
            target.EffectiveDate = EffectiveDate;
            
            if(CreateDate.HasValue)
                target.CreateDate = CreateDate.Value;
            
            target.Id = Id;
            target.Version = Version;
            target.ImageList = SerializeUtil.FromByteArray<List<FlierImage>>(ImageList);

            var status = FlierStatus.Pending;
            Enum.TryParse(Status, true, out status);
            target.Status = status;

            var behaviour = Domain.Behaviour.FlierBehaviour.Default;
            Enum.TryParse(FlierBehaviour, true, out behaviour);
            target.FlierBehaviour = behaviour;

            target.ExtendedProperties = _propertyGroupCollection != null ? new PropertyGroupCollection(_propertyGroupCollection) : null;
            target.ExternalSource = ExternalSource;
            target.ExternalId = ExternalId;
        }

        private PropertyGroupCollection _propertyGroupCollection = new PropertyGroupCollection();
        public PropertyGroupCollection GetPropertyGroupCollection()
        {
            return _propertyGroupCollection;
        }
    }

    internal class FlierStorageDomain : StorageDomainEntityBase<FlierStorageDomain, FlierTableEntry, FlierInterface, Domain.Flier.Flier>
    {
        public static TableNameAndPartitionProvider<FlierInterface> 
            TableNamesAndPartition = new TableNameAndPartitionProvider<FlierInterface>()
                                    {
                                        {typeof(FlierTableEntry), IdPartition, "flierbyid", f => f.Id, f=>f.Id}, 
                                        {typeof(FlierTableEntry), BrowserPartition, "flierbyid", f => f.BrowserId, f=>f.Id}, 
//                                        {typeof(FlierTableEntry), LocationPartition, "flierbyloc", f => GetLocationPartitionKey(f.Location), f=>f.Id},
                                        
//                                        {typeof(FlierSearchEntry), CreatedDateSearchPartition, "flierbycreated", f => GetCoarseLocationPartitionKey(f.Location), f=> GetIdByCreated(f)},
//                                        {typeof(FlierSearchEntry), EffectiveDateSearchPartition, "flierbyeffective", f => GetCoarseLocationPartitionKey(f.Location), f=> GetIdByEffectiveDate(f)},
//                                        {typeof(FlierSearchEntry), PopularitySearchPartition, "flierbypopular", f => GetCoarseLocationPartitionKey(f.Location), f=> GetIdByPopular(f)}
                                    };

        public const int BrowserPartition = 1;
//        public const int LocationPartition = 2;

//        protected readonly ClonedTableEntry<FlierSearchEntry, FlierInterface> SearchTable;
//        public const int CreatedDateSearchPartition = 21;
//        public const int EffectiveDateSearchPartition = 22;
//        public const int PopularitySearchPartition = 23;


        internal FlierStorageDomain(AzureTableContext tableContext)
            : base(TableNamesAndPartition, tableContext)
        {
 //           SearchTable = new ClonedTableEntry<FlierSearchEntry, FlierInterface>(TableNamesAndPartition);
        }

        internal FlierStorageDomain(FlierInterface flier, AzureTableContext tableContext)
            : this(tableContext)
        {
            this.DomainEntity.CopyFieldsFrom(flier);
            ClonedTable.CreateDefaultEntries();
//            SearchTable.CreateDefaultEntries();
        }

        public FlierStorageDomain() : base(TableNamesAndPartition)
        {
//            SearchTable = new ClonedTableEntry<FlierSearchEntry, FlierInterface>(TableNamesAndPartition);   
        }

        public ExtendableTableEntry GetTableStorage()
        {
            return ClonedTable.GetStorageTableEntries().FirstOrDefault(e => e != null) as ExtendableTableEntry;
        }

//        public void LoadSearchPartitions()
//        {
//            var searchEntry = TableContext.PerformQuery<FlierSearchEntry>(
//                bc => bc.PartitionKey == GetCoarseLocationPartitionKey(DomainEntity.Location)
//                    && bc.RowKey == GetIdByCreated(DomainEntity), CreatedDateSearchPartition)
//                .SingleOrDefault();
//            if(searchEntry != null)
//                SearchTable.SetPartitionEntity(CreatedDateSearchPartition, searchEntry);
//        }

//        public override IEnumerable<StorageTableEntryInterface> GetTableEntries()
//        {
//
//            var ret = new List<StorageTableEntryInterface>(base.GetTableEntries());
//            SearchTable.PopulatePartitionClones<Domain.Flier.Flier>(DomainEntity, TableContext);
//            ret.AddRange(SearchTable.GetStorageTableEntries());         
//            return ret;
//        }

        public static IQueryable<FlierInterface> GetByBrowserId(string browserId, AzureTableContext tableContext)
        {
            var tableEntity =
                tableContext.PerformQuery<FlierTableEntry>(te => te.PartitionKey == browserId, BrowserPartition)
                    .AsEnumerable();

            return tableEntity
            .Select(ts => ts.CreateEntityCopy<Domain.Flier.Flier, FlierInterface>())
            .AsQueryable();
        }

        //what we'll do is provide enough field width to introduce extra
        //partitions when needed and we'll start with partitions of  111m? 0.001
        //and allow up to 1m 0.00001 http://en.wikipedia.org/wiki/Decimal_degrees
//        internal static string GetLocationPartitionKey(Location loc)
//        {
//            //make positive and adjust to 0.001 precision
//            //in future if we want to introduce smaller partitions
//            //adjust the below to floor at a finer precision and the multiplier
//            //below that accordingly
//            //00000000-36000000 long
//            //00000000-18000000 lat
//            var longitudeAdjusted = (int)Math.Floor((loc.Longitude + 180.0) * 1000);
//            var latitudeAdjusted = (int)Math.Floor((loc.Latitude + 90.0) * 1000);
//            //take to 0.00001
//            longitudeAdjusted *= 100;
//            latitudeAdjusted *= 100;
//            return longitudeAdjusted.ToString("D8") + ":" + latitudeAdjusted.ToString("D8");
//        }

//        public static string[] GetSortPartitionsForBoundingBox(BoundingBox boundingBox)
//        {
//            IList<string> ret = new List<string>();
//            var latStart = (int)Math.Floor(boundingBox.Min.Latitude + 90.0);
//            var longStart = (int)Math.Floor(boundingBox.Min.Longitude + 180.0);
//
//            var latEnd = (int)Math.Floor(boundingBox.Max.Latitude + 90.0);
//            var longEnd = (int)Math.Floor(boundingBox.Max.Longitude + 180.0);
//
//            for (int latt = latStart; latt <= latEnd; latt++)
//            {
//                for (int longi = longStart; longi <= longEnd; longi++)
//                {
//                    ret.Add(longi.ToString("D3") + ":" + latt.ToString("D3"));
//                }
//            }
//            return ret.ToArray();
//        }

//        internal static string GetCoarseLocationPartitionKey(Location loc)
//        {           
//            var longitudeAdjusted = (int)Math.Floor(loc.Longitude + 180.0);
//            var latitudeAdjusted = (int)Math.Floor(loc.Latitude + 90.0);
//            return longitudeAdjusted.ToString("D3") + ":" + latitudeAdjusted.ToString("D3");
//        }
//
//        internal static string GetCoarseLocationPartitionKey(int lattitude, int longitude)
//        {
//            var longitudeAdjusted = lattitude + 180;
//            var latitudeAdjusted = longitude + 90;
//            return longitudeAdjusted.ToString("D3") + ":" + latitudeAdjusted.ToString("D3");
//        }

//        public static string GetIdByCreated(FlierInterface flier)
//        {
//            return flier.CreateDate.Ticks.ToString("D20") + '[' + flier.Id + ']';
//        }
//
//        public static string GetIdByEffectiveDate(FlierInterface flier)
//        {
//            return flier.EffectiveDate.Ticks.ToString("D20") + '[' + flier.Id + ']';
//        }
//
//        public static string GetIdByPopular(FlierInterface flier)
//        {
//            return (1000000000 - (flier.NumberOfLikes + flier.NumberOfComments)).ToString("D10") + '[' + flier.Id +']';
//        }

//        public static IList<string> FindFliersByLocationTagsAndBoundingBox(Location location
//            , Tags tags, BoundingBox boundingBox, int take, AzureTableContext tableContext)
//        {
//            var partitionKeyMin = GetLocationPartitionKey(boundingBox.Min);
//            var partitionKeyMax = GetLocationPartitionKey(boundingBox.Max);
////            var subPartitionKeyMin = partitionKeyMin.Substring(9);
////            var subPartitionKeyMax = partitionKeyMax.Substring(9);
//
//            var watch = new Stopwatch();
//            watch.Start();
//
//            Expression<Func<FlierTableEntry, bool>> query =
//                (fliers) => fliers.PartitionKey.CompareTo(partitionKeyMin) >= 0
//                            && fliers.PartitionKey.CompareTo(partitionKeyMax) <= 0
//
//                            // one day table storage will support this
//                            //                          && fliers.PartitionKey.Substring(9).CompareTo(subPartitionKeyMin) >= 0
//                            //                          && fliers.PartitionKey.Substring(9).CompareTo(subPartitionKeyMax) <= 0
//                            //when it does can just remove the below checks as partition will be close enough
//                            && fliers.LocationLongitude >= boundingBox.Min.Longitude
//                            && fliers.LocationLongitude <= boundingBox.Max.Longitude
//                            && fliers.LocationLatitude >= boundingBox.Min.Latitude
//                            && fliers.LocationLatitude <= boundingBox.Max.Latitude;
//
////            Expression<Func<FlierTableEntry, MinType>> select =
////                ft => new MinType() { Id = ft.Id, ImageId = ft.Image, FlierBehaviour = ft.FlierBehaviour};
//
//            var tableEntity = tableContext.PerformQuery(query, LocationPartition, take).AsEnumerable();
//
//            var time = watch.ElapsedMilliseconds;
//            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", time, tableEntity.Count());
//            watch.Restart();
//            var ret= tableEntity
//                .Select(ts => ts.CreateEntityCopy<Domain.Flier.Flier, FlierInterface>())
//                //.Select(ts => new Domain.Flier.Flier() { Id = ts.Id, Image = ts.ImageId, FlierBehaviour = (FlierBehaviour)Enum.Parse(typeof(FlierBehaviour), ts.FlierBehaviour) })
//                .Where(_ => tags == null || !tags.Any() || _.Tags.IsSupersetOf(tags)).ToList()
//                .AsQueryable();
//            time = watch.ElapsedMilliseconds;
//            Trace.TraceInformation("FindFliers transform time: {0}, numfliers {1}", time, tableEntity.Count());
//            return ret.Select(f => f.Id).ToList();
//        }

//        public static IList<string> FindSortedFliersByLocationTagsAndBoundingBox    (Location location
//            , Tags tags, BoundingBox boundingBox, int take, FlierSortOrder sortOrder, 
//            FlierInterface endOfPageFlier, AzureTableContext tableContext)
//        {
//            var watch = new Stopwatch();
//            watch.Start();
//
//            var time = watch.ElapsedMilliseconds;
//            var sortPartitionsForBoundingBox = GetSortPartitionsForBoundingBox(boundingBox);
//            var queries = new Expression<Func<FlierSearchEntry, bool>>[sortPartitionsForBoundingBox.Length];
//            for (int i = 0; i < sortPartitionsForBoundingBox.Length; i++)
//            {
//                var partitionKey = sortPartitionsForBoundingBox[i];
//                Expression<Func<FlierSearchEntry, bool>> query = (entry) => entry.PartitionKey == partitionKey;
//
//                if (endOfPageFlier != null)
//                    query = query.AndAlsoSameParam(entry => entry.RowKey.CompareTo(GetSortRowKey(endOfPageFlier, sortOrder)) > 0);
//
//                Expression<Func<FlierSearchEntry, bool>> locationQuery = (entry) =>
//                            entry.LocationLongitude >= boundingBox.Min.Longitude
//                            && entry.LocationLongitude <= boundingBox.Max.Longitude
//                            && entry.LocationLatitude >= boundingBox.Min.Latitude
//                            && entry.LocationLatitude <= boundingBox.Max.Latitude;
//
//                query = query.AndAlsoSameParam(locationQuery);
//
//                query = query.AndAlsoSameParam(GetTagsFilter(tags));
//                queries[i] = query;
//            }
//
//            Trace.TraceInformation("FindFliers expression buildTime: {0}", time);
//            watch.Restart();
//            
//            var retEntities = tableContext.PerformParallelQueries(queries, GetSortPartition(sortOrder), take);
//
//            time = watch.ElapsedMilliseconds;
//            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", time, retEntities.Count());
//            watch.Restart();
//
//            var ret = retEntities.OrderBy(GetSorter(sortOrder)).AsEnumerable();
//            if (take > 0)
//                ret = ret.Take(take);
//            time = watch.ElapsedMilliseconds;
//            Trace.TraceInformation("FindFliers transform time: {0}, numfliers {1}", time, ret.Count());
//            return ret.Select(f => f.Id).ToList();
//        }

//        private static string GetSortRowKey(FlierInterface endOfPageFlier, FlierSortOrder sortOrder)
//        {
//            switch (sortOrder)
//            {
//                case FlierSortOrder.CreatedDate:
//                    return GetIdByCreated(endOfPageFlier);
//                case FlierSortOrder.EffectiveDate:
//                    return GetIdByEffectiveDate(endOfPageFlier);
//                case FlierSortOrder.Popularity:
//                    return GetIdByPopular(endOfPageFlier);
//            }
//            return GetIdByCreated(endOfPageFlier);
//        }

//        private static int GetSortPartition(FlierSortOrder sortOrder)
//        {
//            switch (sortOrder)
//            {
//                case FlierSortOrder.CreatedDate:
//                    return CreatedDateSearchPartition;
//                case FlierSortOrder.EffectiveDate:
//                    return EffectiveDateSearchPartition;
//                case FlierSortOrder.Popularity:
//                    return PopularitySearchPartition;
//            }
//            return CreatedDateSearchPartition;
//        }

//        private static Expression<Func<FlierSearchEntry, object>> GetSorter(FlierSortOrder sortOrder)
//        {
//            switch (sortOrder)
//            {
//                case  FlierSortOrder.CreatedDate:
//                    return entry => entry.CreateDate;
//                case FlierSortOrder.Popularity:
//                    throw new NotSupportedException();
//            }
//            return entry => entry.CreateDate;
//        }

//        private static Expression<Func<FlierSearchEntry, bool>> GetTagsFilter(Tags tags)
//        {
//            Expression<Func<FlierSearchEntry, bool>> ret = null;
//            foreach (var tag in tags)
//            {
//                if (ret == null)
//                    ret = FlierSearchEntry.GetFilterForTag(tag);
//                else
//                {
//                    ret = ret.AndAlsoSameParam(FlierSearchEntry.GetFilterForTag(tag));
//                }
//            }
//            return ret;
//        }


    }
}