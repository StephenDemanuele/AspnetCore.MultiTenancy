using AspnetCore.ServiceCollection.MultiTenancy.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AspnetCore.ServiceCollection.MultiTenancy.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static void UseTenantMiddleware(this IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseMiddleware<TenantMiddleware>();
		}
	}
}
