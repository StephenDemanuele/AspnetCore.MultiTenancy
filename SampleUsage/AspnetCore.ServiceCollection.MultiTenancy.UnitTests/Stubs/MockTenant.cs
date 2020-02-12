namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs
{
	internal class MockTenant : ITenant
	{
		public MockTenant(int tenantId)
		{
			Id = tenantId;
		}

		public int Id { get; }
	}
}
