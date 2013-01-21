namespace PostaFlya.Domain.Flier.Analytic
{
    public enum FlierAnalyticSourceAction
    {
        Unknown,
        QrCodeSrcCodeOnly,
        QrCodeSrcOnFlierWithTearOffs,
        QrCodeSrcTearOff,
        QrCodeSrcOnFlierWithoutTearOffs,
        TinyUrl,
        LocationTrack,
        TinyUrlByApi,
        IdByApi,
        IdByBulletin
    }
}