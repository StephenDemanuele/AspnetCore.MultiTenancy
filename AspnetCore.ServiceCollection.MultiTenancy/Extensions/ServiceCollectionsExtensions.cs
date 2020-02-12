using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;

namespace AspnetCore.ServiceCollection.MultiTenancy.Extensions
{
	public static class ServiceCollectionsExtensions
	{
		public static void AddMultiTenancy(
			this IServiceCollection serviceCollection, 
			IEnumerable<ITenant> tenants,
			ITenantResolutionStrategy tenantResolutionStrategy = null)
		{
			serviceCollection.AddHttpContextAccessor();
			serviceCollection.AddSingleton(tenants);
			serviceCollection.AddSingleton(serviceCollection);

			if (tenantResolutionStrategy == null)
			{
				serviceCollection.AddScoped<ITenantResolutionStrategy, HeaderTenantResolutionStrategy>();
			}
			else
			{
				serviceCollection.AddScoped((serviceProvider) => tenantResolutionStrategy);
			}
		}

		public static void AddTenantScoped(this IServiceCollection serviceCollection,
			ITenant tenant,
			Type serviceType,
			Type implementationType,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			var tenantScopedServiceDescriptor = new TenantScopedServiceDescriptor(tenant, serviceType, implementationType, serviceLifetime);
			serviceCollection.Add(tenantScopedServiceDescriptor);
		}

		public static void AddTenantScoped<TServiceType, TImplementationType>(this IServiceCollection serviceCollection, 
			ITenant tenant,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			serviceCollection.AddTenantScoped(tenant, typeof(TServiceType), typeof(TImplementationType), serviceLifetime);
		}

		public static void AddTenantScoped(this IServiceCollection serviceCollection,
			ITenant tenant,
			Type serviceType,
			object instance)
		{
			var tenantScopedServiceDescriptor = new TenantScopedServiceDescriptor(tenant, serviceType, instance);
			serviceCollection.Add(tenantScopedServiceDescriptor);
		}

		public static void AddTenantScoped<TServiceType>(this IServiceCollection serviceCollection,
			ITenant tenant,
			object instance)
		{
			serviceCollection.AddTenantScoped(tenant, typeof(TServiceType), instance);
		}

		public static void AddTenantScoped(this IServiceCollection serviceCollection,
			ITenant tenant,
			Type serviceType,
			Func<IServiceProvider, object> factory,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			var tenantScopedServiceDescriptor = new TenantScopedServiceDescriptor(tenant, serviceType, factory, serviceLifetime);
			serviceCollection.Add(tenantScopedServiceDescriptor);
		}
	}
}
