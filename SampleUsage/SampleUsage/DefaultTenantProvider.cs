using AspnetCore.ServiceCollection.MultiTenancy;

namespace SampleUsage
{
	public class DefaultTenantProvider : ITenantProvider
	{
		public ITenant Get(int tenantId) => new Tenant(tenantId);
	}
}
