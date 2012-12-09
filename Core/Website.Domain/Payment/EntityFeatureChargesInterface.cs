using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;

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
            foreach (var charge in 
                aggregateRootEntity.Features
                .Select(chargeFeature => 
                    chargeFeature.GetChargeForAggregateMemberEntity(aggregateMemberEntity))
                    .Where(charge => charge != null && !aggregateMemberEntity.Features.Contains(charge)))
            {
                aggregateMemberEntity.Features.Add(charge);
            }
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
        public bool IsPaid { get; set; }
        //not sure if we need extra storage here like a hash table to pass to the behaviour

        public void EnableOrDisableFeaturesBasedOnAvailableCredit(
            EntityInterface entity, ChargableEntityInterface chargableEntity)
        {
            Behvaiour.EnableOrDisableFeaturesBasedOnAvailableCredit(this, entity, chargableEntity);
        }

        public EntityFeatureCharge GetChargeForAggregateMemberEntity(EntityInterface entity)
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
        void EnableOrDisableFeaturesBasedOnAvailableCredit(EntityFeatureCharge entityFeatureCharge, EntityInterface entity, ChargableEntityInterface chargableEntity);
        EntityFeatureCharge GetChargeForAggregateMemberEntity(EntityFeatureCharge entityFeatureCharge, EntityInterface entity);
    }
}