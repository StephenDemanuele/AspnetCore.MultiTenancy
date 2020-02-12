using System.Threading.Tasks;

namespace AspnetCore.ServiceCollection.MultiTenancy.TenantResolution
{
	public interface ITenantResolutionStrategy
	{
		Task<ITenant> GetTenantAsync();
	}
}
