using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Internal;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Util;

namespace PostaFlya.DataRepository.Browser
{
    internal class BrowserCredentialsTableEntry : TableServiceEntity
        , StorageTableEntryInterface<BrowserIdentityProviderCredentialBase>
    {
        public string BrowserId { get; set; }
        public string IdentityProvider { get; set; }
        public string UserIdentifier { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int PartitionClone { get; set; }
        public bool KeyChanged { get; set; }
        public string AccessToken { get; set; }
        public string AccessPermissions { get; set; }
        public DateTime AccessExpires { get; set; }



        public void Update(BrowserIdentityProviderCredentialBase source)
        {
            BrowserId = source.Id;
            IdentityProvider = source.IdentityProvider;
            UserIdentifier = source.UserIdentifier;
            Name = source.Name;
            Email = source.Email;

            if (source.AccessToken != null)
            {
                AccessExpires = source.AccessToken.Expires;
                AccessToken = source.AccessToken.Token;
                AccessPermissions = source.AccessToken.Permissions;
            }
            
#if DEBUG
            UpdateEntity(source);
#endif
        }

        public void UpdateEntity(BrowserIdentityProviderCredentialBase target)
        {
            target.Id = BrowserId;
            target.IdentityProvider = IdentityProvider;
            target.UserIdentifier = UserIdentifier;
            target.Name = Name;
            target.Email = Email;
            target.AccessToken = new AccessToken() 
            {
                Expires = AccessExpires,
                Permissions = AccessPermissions,
                Token = AccessToken
            };
        }
    }

    internal class BrowserTableEntry : TableServiceEntity
        , StorageTableEntryInterface<BrowserInterface>
    {
        
        public BrowserTableEntry()
        {
            DefaultLocationLatitude = -200;
            DefaultLocationLongitude = -200;
            AddressLatitude = -200;
            AddressLongitude = -200;
        }

        public string Id { get; set; }
        public int Version { get; set; }

        public string Handle { get; set; }
        public string EmailAddress { get; set; }
        public int? Distance { get; set; }
        public string Tags { get; set; }
        public string Roles { get; set; }

        public double DefaultLocationLatitude { get; set; }
        public double DefaultLocationLongitude { get; set; }
        public string DefaultLocationDescription { get; set; }

        public double AddressLatitude { get; set; }
        public double AddressLongitude { get; set; }
        public string AddressDescription { get; set; }
        public int? AddressPublic { get; set; }

        public byte[] SavedTags { get; set; }
        public byte[] SavedLocations { get; set; }

        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public string AvatarImageId { get; set; }

        public int PartitionClone { get; set; }
        public bool KeyChanged { get; set; }

        public void Update(BrowserInterface source)
        {
            Id = source.Id;
            Version = source.Version;
            Handle = source.Handle;
            EmailAddress = source.EmailAddress;
            Distance = source.Distance;
            Tags = source.Tags.ToString();
            Roles = source.Roles.ToString();

            if (source.DefaultLocation != null)
            {
                DefaultLocationLatitude = source.DefaultLocation.Latitude;
                DefaultLocationLongitude = source.DefaultLocation.Longitude;
                DefaultLocationDescription = source.DefaultLocation.Description;
            }

            SavedTags = SerializeUtil.ToByteArray(source.SavedTags);
            SavedLocations = SerializeUtil.ToByteArray(source.SavedLocations);

            FirstName = source.FirstName;
            MiddleNames = source.MiddleNames;
            Surname = source.Surname;
            AvatarImageId = source.AvatarImageId;

            if (source.Address != null)
            {
                AddressLatitude = source.Address.Latitude;
                AddressLongitude = source.Address.Longitude;
                AddressDescription = source.Address.Description;
            }

            AddressPublic = source.AddressPublic ? 1 : 0;

#if DEBUG
            UpdateEntity(source);
#endif
        }

        public void UpdateEntity(BrowserInterface target)
        {
            target.Id = Id;
            target.Version = Version;
            target.Handle = Handle;
            target.EmailAddress = EmailAddress;
            target.Distance = Distance;
            target.Tags = Tags == null ? new Tags() : new Tags(Tags);
            target.DefaultLocation = new Location(DefaultLocationLongitude, DefaultLocationLatitude, DefaultLocationDescription);
            target.SavedTags = SerializeUtil.FromByteArray<List<Tags>>(SavedTags);
            target.SavedLocations = SerializeUtil.FromByteArray<Locations>(SavedLocations);
            target.Roles = Roles == null ? new Roles() : new Roles(Roles);
            target.FirstName = FirstName;
            target.MiddleNames = MiddleNames;
            target.Surname = Surname;
            target.AvatarImageId = AvatarImageId;
            target.Address = new Location(AddressLongitude, AddressLatitude, AddressDescription);
            target.AddressPublic = AddressPublic.HasValue && AddressPublic.Value != 0;
        }
    }

    [Serializable]
    internal class BrowserIdentityProviderCredentialBase : IdentityProviderCredential, EntityInterface
    {
        public string Id { get; set; }
        public int Version { get; set; }
        public PropertyGroupCollection ExtendedProperties { get; set; }

        public Type PrimaryInterface
        {
            get { return typeof (BrowserIdentityProviderCredentialBase); }
        }      
    }

    internal class BrowserIdentityProviderCredential : BrowserIdentityProviderCredentialBase
           , StorageDomainEntityInterface
    {
        public static TableNameAndPartitionProvider<BrowserIdentityProviderCredentialBase>
            TableNamesAndPartition = new TableNameAndPartitionProvider<BrowserIdentityProviderCredentialBase>()
                                              {
                                                  {typeof(BrowserCredentialsTableEntry), IdPartition, "browserCredential", i => i.Id, i => i.GetHash()},
                                                  {typeof(BrowserCredentialsTableEntry), HashPartition, "browserCredential", i => i.GetHash(), i => i.Id},    
                                              };

        private readonly ClonedTableEntry<BrowserCredentialsTableEntry, BrowserIdentityProviderCredentialBase> _browserCreds
            = new ClonedTableEntry<BrowserCredentialsTableEntry, BrowserIdentityProviderCredentialBase>(TableNamesAndPartition);
        public const int IdPartition = 0;
        public const int HashPartition = 1;

        private readonly AzureTableContext _tableContext;

        protected internal BrowserIdentityProviderCredential(AzureTableContext tableContext)
        {
            _tableContext = tableContext;
        }

        protected internal BrowserIdentityProviderCredential(string browserId, IdentityProviderCredential source, AzureTableContext tableContext)
            : this(tableContext)
        {
            Id = browserId;
            IdentityProvider = source.IdentityProvider;
            UserIdentifier = source.UserIdentifier;
            Name = source.Name;
            Email = source.Email;
            if (source.AccessToken != null)
            {
                this.AccessToken = new AccessToken()
                {
                    Expires = source.AccessToken.Expires,
                    Token = source.AccessToken.Token,  
                    Permissions = source.AccessToken.Permissions

                };
            }

            _browserCreds.CreateDefaultEntries();
        }

        public void LoadByPartition(BrowserCredentialsTableEntry table, int partition)
        {
            _browserCreds.SetPartitionEntity(partition, table);
            table.UpdateEntity(this);
        }

        public IEnumerable<StorageTableEntryInterface> GetTableEntries()
        {
            _browserCreds.PopulatePartitionClones<BrowserIdentityProviderCredentialBase>(this, _tableContext);
            return _browserCreds.GetStorageTableEntries();
        }

        
    }


    internal class BrowserStorageDomain :
        StorageDomainEntityBase<BrowserStorageDomain, BrowserTableEntry, BrowserInterface, Domain.Browser.Browser>
    {
        public const int HandlePartition = 1;
        public static TableNameAndPartitionProvider<BrowserInterface>
            TableNamesAndPartition = new TableNameAndPartitionProvider<BrowserInterface>()
                                                         {
                                                           {typeof(BrowserTableEntry), IdPartition, "browser", i => i.Id, i => i.Id}, 
                                                           {typeof(BrowserTableEntry), HandlePartition, "browser", i => i.Handle, i => i.Id},
                                                         };
        



        protected internal BrowserStorageDomain(AzureTableContext tableContext)
            : base(TableNamesAndPartition, tableContext)
        {

        }

        protected internal BrowserStorageDomain(BrowserInterface source, AzureTableContext tableContext)
            : this(tableContext)
        {
            this.DomainEntity.CopyFieldsFrom(source);
            ClonedTable.CreateDefaultEntries();
            this.DomainEntity.ExternalCredentials = new HashSet<IdentityProviderCredential>();
            foreach (var cred in source.ExternalCredentials)
            {
                this.DomainEntity.ExternalCredentials.Add(new BrowserIdentityProviderCredential(this.DomainEntity.Id, cred, TableContext));
            }
        }

        public BrowserStorageDomain() : base(TableNamesAndPartition)
        {
        }

        public override void LoadByPartition(BrowserTableEntry table, int partition)
        {
            base.LoadByPartition(table, partition);
            LoadCredentials();
        }

        private void LoadCredentials()
        {         
            var browserPartition = BrowserCredentialsTableEntities(this.DomainEntity.Id, TableContext);

            this.DomainEntity.ExternalCredentials = new HashSet<IdentityProviderCredential>();
            foreach (var browserCredentialsTableEntity in browserPartition)
            {
                var credential = new BrowserIdentityProviderCredential(TableContext);
                credential.LoadByPartition(browserCredentialsTableEntity, BrowserIdentityProviderCredential.IdPartition);
                this.DomainEntity.ExternalCredentials.Add(credential);
            }
        }

        private void SyncCredentials()
        {
            for (var i = 0; i < this.DomainEntity.ExternalCredentials.Count; i++)
            {
                var externalCred = this.DomainEntity.ExternalCredentials.ElementAt(i);
                if ((externalCred is BrowserIdentityProviderCredential)) continue;
                this.DomainEntity.ExternalCredentials.Remove(externalCred);
                this.DomainEntity.ExternalCredentials.Add(
                    new BrowserIdentityProviderCredential(this.DomainEntity.Id, externalCred, TableContext));
            }
        }

        public override IEnumerable<StorageTableEntryInterface> GetTableEntries()
        {
            var ret = new List<StorageTableEntryInterface>(base.GetTableEntries());
            SyncCredentials();
            foreach (BrowserIdentityProviderCredential externalCred in this.DomainEntity.ExternalCredentials)
            {
                ret.AddRange(externalCred.GetTableEntries());
            }
            return ret;
        }

        public static BrowserInterface FindById(string id, AzureTableContext tableContext)
        {
            var ret = StorageDomainEntityBase<
                BrowserStorageDomain, BrowserTableEntry, BrowserInterface, Domain.Browser.Browser
                >.FindById(id, tableContext);

            return ret == null ? null : GetCredentials(ret, tableContext);
        }

        private static BrowserInterface GetCredentials(BrowserInterface ret, AzureTableContext tableContext)
        {
            var credentialsTableEntities = BrowserCredentialsTableEntities(ret.Id, tableContext);

            ret.ExternalCredentials = new HashSet<IdentityProviderCredential>();
            foreach (var credential in credentialsTableEntities
                .Select(browserCredentialsTableEntity =>
                    browserCredentialsTableEntity.CreateEntityCopy<BrowserIdentityProviderCredentialBase, BrowserIdentityProviderCredentialBase>()))
            {
                ret.ExternalCredentials.Add(credential);
            }

            return ret;
        }

        private static IEnumerable<BrowserCredentialsTableEntry> BrowserCredentialsTableEntities(string id, AzureTableContext tableContext)
        {
            var browserPartition = tableContext.PerformQuery<BrowserCredentialsTableEntry>(bc => bc.PartitionKey == id)
                .AsEnumerable();
            return browserPartition;
        }

        public static BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential, AzureTableContext tableContext)
        {
            var tableEntity = tableContext.PerformQuery<BrowserCredentialsTableEntry>(
                bc => bc.PartitionKey == credential.GetHash()
                && bc.IdentityProvider == credential.IdentityProvider
                && bc.UserIdentifier == credential.UserIdentifier
                )
                .SingleOrDefault();

            return tableEntity != null ? FindById(tableEntity.BrowserId, tableContext) : null;
        }

        public static BrowserInterface FindByHandle(string handle, AzureTableContext tableContext)
        {
            var ret = FindById(handle, tableContext, HandlePartition);

            return ret == null ? null : GetCredentials(ret, tableContext);
        }
    }
}