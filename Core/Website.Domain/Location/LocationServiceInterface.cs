namespace Website.Domain.Location
{
    public interface LocationServiceInterface
    {
        bool IsWithinDefaultDistance(Location location);
        bool IsWithinBoundingBox(BoundingBox boundingBox, Location location);
        BoundingBox GetDefaultBox(Location location);
        bool IsWithinDistance(Location location, int distance);
        BoundingBox GetBoundingBox(Location location, double distance);
    }
}