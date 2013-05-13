using System;
using System.Collections.Generic;

namespace Website.Application.Google.Payment
{
    public class DigitalGoodsOrder
    {
        public DigitalGoodsOrder()
        {
            UserData = new Dictionary<string, string>();
        }

        public String OrderId { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public GoogleWalletDigitalGoods.CurrencyCode Currency { get; set; }

        public Dictionary<String, String> UserData { get; set; }

        public double Price { get; set; }
    }
}