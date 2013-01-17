using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostaFlya.Application.Domain.Flier
{
    public static class QrSource
    {
        public static string GetDescFromParam(string param)
        {
            if (string.IsNullOrWhiteSpace(param))
                return null;

            if (param.Equals("1"))
                return "QrCodeSrcCodeOnly";
            if (param.Equals("2"))
                return "QrCodeSrcOnFlierWithTearOffs";
            if (param.Equals("3"))
                return "QrCodeSrcTearOff";
            if (param.Equals("4"))
                return "QrCodeSrcOnFlierWithoutTearOffs";
            return null;
        }
        public const string QrCodeSrcCodeOnly = "?q=1";
        public const string QrCodeSrcOnFlierWithTearOffs = "?q=2";
        public const string QrCodeSrcTearOff = "?q=3";
        public const string QrCodeSrcOnFlierWithoutTearOffs = "?q=4";
    }
}
