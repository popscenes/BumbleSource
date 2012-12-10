using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Domain.Payment
{

    public static class EntityFeatureChargesInterfaceExtension
    {
        public static void CopyFieldsFrom(this EntityFeatureChargesInterface target,
                                          EntityFeatureChargesInterface source)
        {
            target.Features = source.Features != null ? new HashSet<EntityFeatureCharge>(source.Features) : null;            
        }

        public static void MergeChargesForAggregateMemberEntity<MemberEntityType>(
            this EntityFeatureChargesInterface aggregateRootEntity, MemberEntityType aggregateMemberEntity)
            where MemberEntityType : EntityInterface, EntityFeatureChargesInterface
        {
            if (aggregateRootEntity.Features == null || aggregateRootEntity.Features.Count == 0)
                return;
            foreach (var charge in 
                aggregateRootEntity.Features
                .Select(chargeFeature => 
                    chargeFeature.GetChargeForAggregateMemberEntity(aggregateMemberEntity))
                    .Where(charge => charge != null &&  
                        (aggregateMemberEntity.Features == null || !aggregateMemberEntity.Features.Contains(charge))))
            {
                if(aggregateMemberEntity.Features == null)
                    aggregateMemberEntity.Features = new HashSet<EntityFeatureCharge>();
                aggregateMemberEntity.Features.Add(charge);
            }
        }

        public static bool EnableOrDisablePaidFeaturesBasedOnState<EntityType>(this EntityType entity)
            where EntityType : EntityInterface, EntityFeatureChargesInterface
        {
            if (entity.Features == null) return false;
            return entity.Features.Aggregate(false, 
                (current, chargeFeature) => chargeFeature.EnableOrDisableFeaturesBasedOnState(entity) || current);
        }

        public static bool ChargeForState<EntityType>(this EntityType entity, GenericRepositoryInterface repository, GenericQueryServiceInterface queryService)
            where EntityType : class, EntityInterface, EntityFeatureChargesInterface
        {
            if (entity.Features == null) return false;
            return entity.Features.Aggregate(false, 
                (current, chargeFeature) => chargeFeature.ChargeForState(entity, repository, queryService) || current);
        }

        public static void MergeUpdateFeatureCharges<EntityType>(this EntityType target, HashSet<EntityFeatureCharge> source)
            where EntityType : class, EntityInterface, EntityFeatureChargesInterface
        {

            if (target.Features == null) return;
            foreach (var chargeFeature in target.Features)
            {
                var src = source.FirstOrDefault(f => f.Equals(chargeFeature));          
                if(src != null)
                    chargeFeature.UpdateCost(src);
            }

            target.EnableOrDisablePaidFeaturesBasedOnState();
        }
    }

    public interface EntityFeatureChargesInterface
    {
        HashSet<EntityFeatureCharge> Features { get; set; }
    }

    public class EntityFeatureCharge
    {
        public string BehaviourTypeString { get; set; }
        public string Description { get; set; }
        public string CurrentStateMessage { get; set; }
        public int Cost { get; set; }
        public int Paid { get; set; }
        public bool IsPaid {
            get { return OutstandingBalance <= 0; }
        }
        public int OutstandingBalance
        {
            get { return Cost - Paid; }
        }
        //not sure if we need extra storage here like a hash table to pass to the behaviour

        public void UpdateCost(EntityFeatureCharge source)
        {
            Cost = source.Cost;
        }

        public bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityType entity) where EntityType : EntityInterface
        {
            return Behvaiour.EnableOrDisableFeaturesBasedOnState(this, entity);
        }

        public bool ChargeForState<EntityType>(EntityType entity, GenericRepositoryInterface repository, GenericQueryServiceInterface queryService) where EntityType : EntityInterface
        {
            Behvaiour.ChargeForState<EntityType>(this, entity, repository, queryService);
            return EnableOrDisableFeaturesBasedOnState(entity);
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(MemberEntityType entity) where MemberEntityType : EntityInterface
        {
            return Behvaiour.GetChargeForAggregateMemberEntity(this, entity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityFeatureCharge) obj);
        }

        protected bool Equals(EntityFeatureCharge other)
        {
            return string.Equals(BehaviourTypeString, other.BehaviourTypeString);
        }

        public override int GetHashCode()
        {
            return (BehaviourTypeString != null ? BehaviourTypeString.GetHashCode() : 0);
        }

        private EntityFeatureChargeBehaviourInterface _behvaiour = null;
        private EntityFeatureChargeBehaviourInterface Behvaiour
        {
            get { return _behvaiour ?? (_behvaiour = CreateBehaviourByName()); }
        }

        private EntityFeatureChargeBehaviourInterface CreateBehaviourByName()
        {
            var type = Type.GetType(BehaviourTypeString);
            if (type == null)
                return null;
            var ret = Activator.CreateInstance(type);
            return ret as EntityFeatureChargeBehaviourInterface;
        }

    }

    public interface EntityFeatureChargeBehaviourInterface
    {
        bool EnableOrDisableFeaturesBasedOnState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity) where EntityType : EntityInterface;
        EntityFeatureCharge GetChargeForAggregateMemberEntity<MemberEntityType>(EntityFeatureCharge entityFeatureCharge, MemberEntityType entity) where MemberEntityType : EntityInterface;
        bool ChargeForState<EntityType>(EntityFeatureCharge entityFeatureCharge, EntityType entity, GenericRepositoryInterface repository, GenericQueryServiceInterface queryService) where EntityType : EntityInterface;
    }
}