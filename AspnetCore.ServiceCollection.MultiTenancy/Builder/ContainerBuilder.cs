using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AspnetCore.ServiceCollection.MultiTenancy.Builder
{
	internal class ContainerBuilder
	{
		private readonly IEnumerable<ITenant> _tenants;

		private readonly IServiceCollection _masterServiceCollection;

		public ContainerBuilder(IEnumerable<ITenant> tenants, IServiceCollection masterServiceCollection)
		{
			_tenants = tenants;
			_masterServiceCollection = masterServiceCollection;
		}

		public ConcurrentDictionary<int, IServiceProvider> Build()
		{
			var containerDictionary = new ConcurrentDictionary<int, IServiceProvider>();
			using (var scope = _masterServiceCollection.BuildServiceProvider().CreateScope())
			{
				foreach (var tenant in _tenants)
				{
					//get registrations set for the specific tenant
					var tenantScopedServiceDescriptors = _masterServiceCollection
						.Where(x => x is TenantScopedServiceDescriptor scopedServiceDescriptor && scopedServiceDescriptor.Tenant.Id == tenant.Id)
						.Select(x => (TenantScopedServiceDescriptor)x);

					var tenantServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
					foreach (var tenantServiceDescriptor in tenantScopedServiceDescriptors)
					{
						tenantServiceCollection.Add(tenantServiceDescriptor);
					}

					//get registrations which are not tenant scoped
					var generalScopeServiceDescriptors = _masterServiceCollection
						.Where(x => x is ServiceDescriptor)
						.ToList();
					foreach (var serviceDescriptor in generalScopeServiceDescriptors)
					{
						if (serviceDescriptor is TenantScopedServiceDescriptor || serviceDescriptor is IServiceCollection || serviceDescriptor.ServiceType == typeof(ContainerBuilder))
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

					containerDictionary.TryAdd(tenant.Id, tenantServiceCollection.BuildServiceProvider());
				}
			}

			return containerDictionary;
		}
	}
}
