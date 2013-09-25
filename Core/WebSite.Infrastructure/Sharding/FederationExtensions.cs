using System;
using System.Linq;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Types;

namespace Website.Infrastructure.Sharding
{
    public class FederationInstance
    {
        public override string ToString()
        {
            return FederationName + " " + DistributionName + "=" + FedVal;
        }

        public string FederationName { get; set; }
        public string DistributionName { get; set; }
        public object FedVal { get; set; }
    }

    public static class FederationExtensions
    {
        private static bool? _federationDisabled;
        public static bool FederationDisabled
        {
            get
            {
                if (_federationDisabled.HasValue)
                    return _federationDisabled.Value;

                bool ret;
                bool.TryParse(Config.Instance.GetSetting("DisableFederation"),
                out ret);
                _federationDisabled = ret;
                return ret;
            }
        }

        public static FederationInstance GetFedInstance(this object fedobject)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedobject.GetType(), typeof(FederationColumnAttribute));
            if (prop == null || FederationDisabled)
                return null;

            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationColumnAttribute)) as FederationColumnAttribute;

            var fedVal = prop.GetValue(fedobject, null);

            return new FederationInstance()
                {
                    FederationName = fedAtt.FederationName,
                    DistributionName = fedAtt.DistributionName,
                    FedVal = fedVal
                };
        }

        public static bool SetFedVal(this object fedObject, object fedVal)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedObject.GetType(), typeof(FederationColumnAttribute));
            if (prop == null || FederationExtensions.FederationDisabled)
                return false;

            prop.SetValue(fedObject, SerializeUtil.ConvertVal(fedVal, prop.PropertyType), null);
            return true;
        }

        public static FederationInstance GetFedInfo(this Type fedTyp)
        {
            var prop = SerializeUtil.GetPropertyWithAttribute(fedTyp, typeof(FederationColumnAttribute));
            if (prop == null || FederationExtensions.FederationDisabled)
                return null;

            var fedAtt = prop.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationColumnAttribute)) as FederationColumnAttribute;

            return new FederationInstance()
                {
                    FederationName = fedAtt.FederationName,
                    DistributionName = fedAtt.DistributionName,
                    FedVal = null,
                };
        }
    }
}