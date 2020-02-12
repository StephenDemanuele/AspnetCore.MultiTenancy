namespace AspnetCore.ServiceCollection.MultiTenancy
{
	public interface ITenantProvider
	{
		ITenant Get(int tenantId);
	}
}
