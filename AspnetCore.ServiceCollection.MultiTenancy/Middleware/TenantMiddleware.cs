using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using AspnetCore.ServiceCollection.MultiTenancy.Builder;
using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;

namespace AspnetCore.ServiceCollection.MultiTenancy.Middleware
{
	internal class TenantMiddleware
	{
		private readonly RequestDelegate _next;

		private ConcurrentDictionary<int, IServiceProvider> Containers = new ConcurrentDictionary<int, IServiceProvider>();

		public TenantMiddleware(RequestDelegate next, ContainerBuilder containerBuilder)
		{
			_next = next;
			Containers = containerBuilder.Build();
		}

		public async Task Invoke(HttpContext context)
		{
			var tenantResolutionStrategy = context.RequestServices.GetService(typeof(ITenantResolutionStrategy)) as ITenantResolutionStrategy;
			var tenant = await tenantResolutionStrategy.GetTenantAsync();

			if (!Containers.ContainsKey(tenant.Id))
			{
				throw new InvalidOperationException($"No configuration for tenant {tenant.Id}");
			}

			using (var scope = Containers[tenant.Id].CreateScope())
			{
				context.RequestServices = scope.ServiceProvider;

				if (_next != null)
					await _next(context);
			}
		}
	}
}
