using System;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public static class CreditTransactionInterfaceExtensions
    {
        public static void CopyFieldsFrom(this CreditTransactionInterface target, CreditTransactionInterface source)
        {
            target.Credits = source.Credits;
            target.CreditTransactionType = source.CreditTransactionType;
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }
    public interface CreditTransactionInterface : EntityInterface, AggregateInterface, BrowserIdInterface
    {
        String CreditTransactionType { get; set; }
        decimal Credits { get; set; }
    }
    public class CreditTransaction : CreditTransactionInterface
    {
        public int Version { get; set; }

        public Type PrimaryInterface { get; set; }

        public string Id { get; set; }
        public string FriendlyId { get; set; }

        public string AggregateId { get; set; }

        public string AggregateTypeTag { get; set; }

        public String CreditTransactionType { get; set; }
        public decimal Credits { get; set; }

        public string BrowserId { get; set; }
    }
}