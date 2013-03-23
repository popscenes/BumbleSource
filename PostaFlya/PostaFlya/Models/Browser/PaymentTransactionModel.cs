using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Domain.Payment;

namespace PostaFlya.Models.Browser
{
    public class PaymentTrasactionPageModel : PageModelInterface
    {
        public string PageId { get; set; }
        public string ActiveNav { get; set; }
        public List<PaymentTransactionModel> Transactions { get; set; }
    }

    public static class PaymentTransactionExtensions
    {
        public static PaymentTransactionModel ToViewModel(this PaymentTransaction transaction)
        {
            var ret = new PaymentTransactionModel();
            ret.CopyFieldsFrom(transaction);
            return ret;
        }
    }

    public class PaymentTransactionModel : PaymentTransactionFieldsInterface
    {
        public string PayerId { get; set; }
        public string TransactionId { get; set; }
        public PaymentType Type { get; set; }
        public string PaymentEntityId { get; set; }
        public double Amount { get; set; }
        public PaymentTransactionStatus Status { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}