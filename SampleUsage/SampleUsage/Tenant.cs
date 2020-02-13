using AspnetCore.ServiceCollection.MultiTenancy;

namespace SampleUsage
{
	public class Tenant : ITenant
	{
		public Tenant(int id) => Id = id;

		public int Id { get; }
	}
}
