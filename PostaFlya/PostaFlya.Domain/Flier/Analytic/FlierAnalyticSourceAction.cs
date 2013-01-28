namespace PostaFlya.Domain.Flier.Analytic
{
    public enum FlierAnalyticSourceAction
    {
        Unknown = 0,
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