using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using paycircuit.com.google.iap;

namespace Website.Application.Intergrations.Payment
{
    public class GoogleWalletDigitalGoods 
    {
        public string SellerId { get; set; }
        public string Secret { get; set; }

        public object PaymentUrl { get; set; }

        public enum CurrencyCode
        {
            AUD
        }

        public string GenerateJWT(DigitalGoodsOrder digitalGoodsOrder)
        {
            var HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");
            
            InAppItemObject ClaimObj = new InAppItemObject(digitalGoodsOrder.Name, digitalGoodsOrder.Description, digitalGoodsOrder.Price.ToString(CultureInfo.InvariantCulture),
                digitalGoodsOrder.Currency.ToString(), JsonConvert.SerializeObject(digitalGoodsOrder.UserData), SellerId, 10);
            return JWTHelpers.buildJWT(HeaderObj, ClaimObj, Secret);
        }

        public DigitalGoodsOrder OrderFromJWT(string jwtString)
        {
            InAppItemObject claimObj = ParseStrictJwt(jwtString);
            var digitilGoodsOrder = new DigitalGoodsOrder()
                {
                    UserData = JsonConvert.DeserializeObject<Dictionary<String, String>>(claimObj.request.sellerData),
                    Currency =
                        (CurrencyCode)
                        Enum.Parse(typeof (CurrencyCode), claimObj.request.currencyCode, true),
                    Description = claimObj.request.description,
                    Name = claimObj.request.name,
                    Price = Double.Parse(claimObj.request.price),
                    OrderId = claimObj.response == null ? "":claimObj.response.orderId
                };

            return digitilGoodsOrder;
        }

        private InAppItemObject ParseStrictJwt(string jstring)
        {
            //JWTHeaderObject headerObj = null;
            //InAppItemObject claimObj = null;

            string jwtHeader = string.Empty;
            string jwtClaim = string.Empty;

            if (JWTHelpers.verifyJWT(jstring, Secret, ref jwtHeader, ref jwtClaim))
            {
                //headerObj = JSONHelpers.dataContractJSONToObj(jwtHeader, new JWTHeaderObject()) as JWTHeaderObject;
                InAppItemObject claimObj = null;
                claimObj = JSONHelpers.dataContractJSONToObj(jwtClaim, new InAppItemObject()) as InAppItemObject;
                return claimObj;
            }
            return null;
        }
    }
}