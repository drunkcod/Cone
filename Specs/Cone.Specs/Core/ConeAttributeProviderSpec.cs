using System.Linq;

namespace Cone.Core
{
	[Describe(typeof(ConeAttributeProvider))]
    public class ConeAttributeProviderSpec
    {
        public void supports_attribute_lookup_by_basetype() {
            IConeAttributeProvider provider = new ConeAttributeProvider(new[] { new PendingAttribute() });
            Check.That(() => provider.GetCustomAttributes(typeof(IPendingAttribute)).Any());
        }
    }
}
