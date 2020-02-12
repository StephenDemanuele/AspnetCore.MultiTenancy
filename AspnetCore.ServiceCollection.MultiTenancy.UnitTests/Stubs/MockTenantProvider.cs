namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs
{
	internal class MockTenantProvider : ITenantProvider
	{
		public ITenant Get(int tenantId)
		{
			return new MockTenant(tenantId);
		}
	}
}
