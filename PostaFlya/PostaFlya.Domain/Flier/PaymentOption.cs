namespace PostaFlya.Domain.Flier
{

    public enum PaymentOptionType
    {
        ContactDetails
    }

    public enum PaymentOptionStatus
    {
        PaymentPending,
        PaymentAccepted,
        PaymentCanceled
    }

    public class PaymentOption
    {
        public PaymentOptionStatus Status { get; set; }

        public PaymentOptionType Type { get; set; }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }
    }
}