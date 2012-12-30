using System;

namespace Website.Domain.Claims
{
    public interface ClaimableInterface
    {
        int NumberOfClaims { get; set; }
    }

    public static class ClaimableInterfaceExtensions
    {
        public static void CopyFieldsFrom(this ClaimableInterface target, ClaimableInterface source)
        {
            target.NumberOfClaims = source.NumberOfClaims;
        }
    }
}