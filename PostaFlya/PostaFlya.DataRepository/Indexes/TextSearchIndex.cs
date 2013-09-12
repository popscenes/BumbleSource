using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;

namespace PostaFlya.DataRepository.Indexes
{
    public class TextSearchIndex : Index<EntityInterface>
    {
        public override string IndexName
        {
            get { return "T"; }
        }
    }
}