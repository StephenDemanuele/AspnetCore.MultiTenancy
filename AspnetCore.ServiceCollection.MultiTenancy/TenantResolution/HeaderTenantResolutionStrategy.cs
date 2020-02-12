using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AspnetCore.ServiceCollection.MultiTenancy.UnitTests")]
namespace AspnetCore.ServiceCollection.MultiTenancy.TenantResolution
{
	internal class HeaderTenantResolutionStrategy : ITenantResolutionStrategy
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		private readonly ITenantProvider _tenantProvider;

		private const string _tenantIdHeaderKey = "tenantId";

		public HeaderTenantResolutionStrategy(
			IHttpContextAccessor httpContextAccessor, 
			ITenantProvider tenantProvider)
		{
			_httpContextAccessor = httpContextAccessor;
			_tenantProvider = tenantProvider;
		}

		public Task<ITenant> GetTenantAsync()
		{
			var request = _httpContextAccessor.HttpContext.Request;
			if (!request.Headers.ContainsKey(_tenantIdHeaderKey))
				throw new InvalidOperationException("Missing tenant id");

			var tenantHeaderValue = request.Headers[_tenantIdHeaderKey].FirstOrDefault();
			if (!int.TryParse(tenantHeaderValue, out int tenantId))
				throw new InvalidOperationException($"Invalid tenant id: {tenantHeaderValue}");

			return Task.FromResult(_tenantProvider.Get(tenantId));
		}
	}
}
