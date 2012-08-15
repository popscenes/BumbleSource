using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.TableStorage
{
    public interface PropertyGroupTableSerializerInterface
    {
        void LoadProperties(PropertyGroupCollection propertyGroup, ExtendableTableEntry tableEntry);
        void MergeProperties(ExtendableTableEntry tableEntry, PropertyGroupCollection propertyGroup);
    }
}
