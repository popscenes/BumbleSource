using System;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public static class PaymentTransactionInterfaceExtensions
    {
        public static void CopyFieldsFrom(this PaymentTransactionInterface target, PaymentTransactionInterface source)
        {
            target.Amount = source.Amount;
            target.FriendlyId = source.FriendlyId;
            target.Id = source.Id;
            target.Message = source.Message;
            target.PayerId = source.PayerId;
            target.PaymentEntityId = source.PaymentEntityId;
            target.Status = source.Status;
            target.TransactionId = source.TransactionId;
            target.Type = source.Type;
            target.Time = source.Time;
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
        }
    }

    public interface PaymentTransactionInterface : EntityInterface, BrowserIdInterface, AggregateInterface
    {
        String PayerId { get; set; }

        String TransactionId { get; set; }

        PaymentType Type { get; set; }

        String PaymentEntityId { get; set; }

        double Amount{ get; set; }

        PaymentTransactionStatus Status { get; set; }

        string Message{ get; set; }

        DateTimeOffset Time { get; set; }

    }

    public class PaymentTransaction : PaymentTransactionInterface
    {

        public string PayerId { get; set; }

        public string TransactionId { get; set; }

        public PaymentType Type { get; set; }

        public string PaymentEntityId { get; set; }

        public double Amount { get; set; }

        public PaymentTransactionStatus Status { get; set; }

        public string Message { get; set; }
        public DateTimeOffset Time { get; set; }

        public int Version { get; set; }

        public Type PrimaryInterface {
            get { return typeof(PaymentTransactionInterface); }
        }

        public string Id { get; set; }

        public string FriendlyId { get; set; }

        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
        public string BrowserId { get; set; }

        
    }
}