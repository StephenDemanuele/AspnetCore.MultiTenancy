using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;

namespace AspnetCore.ServiceCollection.MultiTenancy.Middleware
{
	public class TenantMiddleware
	{
		private readonly RequestDelegate next;

		private ConcurrentDictionary<int, IServiceProvider> Containers;

		public TenantMiddleware(
			RequestDelegate next, 
			IEnumerable<ITenant> tenants, 
			IServiceCollection masterServiceCollection)
		{
			this.next = next;
			Containers = new ConcurrentDictionary<int, IServiceProvider>();

			using (var scope = masterServiceCollection.BuildServiceProvider().CreateScope())
			{
				foreach (var tenant in tenants)
				{
					var tenantServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
					//get registrations set for the specific tenant
					var tenantScopedServiceDescriptors = masterServiceCollection
						.Where(x => x is TenantScopedServiceDescriptor scopedServiceDescriptor && scopedServiceDescriptor.Tenant.Id == tenant.Id)
						.Select(x => (TenantScopedServiceDescriptor)x);

					foreach (var tenantServiceDescriptor in tenantScopedServiceDescriptors)
					{
						tenantServiceCollection.Add(tenantServiceDescriptor);
					}

					//get registrations which are not tenant scoped
					var generalScopeServiceDescriptors = masterServiceCollection
						.Where(x => x is ServiceDescriptor).ToList();

					foreach (var serviceDescriptor in generalScopeServiceDescriptors)
					{
						if (serviceDescriptor is TenantScopedServiceDescriptor || serviceDescriptor is IServiceCollection)
							continue;

						try
						{
							var requiredService = scope.ServiceProvider.GetRequiredService(serviceDescriptor.ServiceType);
							tenantServiceCollection.Add(new ServiceDescriptor(serviceDescriptor.ServiceType, requiredService));
						}
						catch
						{
							tenantServiceCollection.Add(serviceDescriptor);
						}
					}

					Containers.TryAdd(tenant.Id, tenantServiceCollection.BuildServiceProvider());
				}
			}
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

				if (next != null)
					await next(context);
			}
		}
	}
}
