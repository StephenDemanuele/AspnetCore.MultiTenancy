using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspnetCore.ServiceCollection.MultiTenancy
{
	internal class TenantScopedServiceDescriptor : ServiceDescriptor
	{
		public ITenant Tenant { get; }

		public TenantScopedServiceDescriptor(ITenant tenant, Type serviceType, Type implementationType, ServiceLifetime serviceLifetime)
			: base(serviceType, implementationType, serviceLifetime)
		{
			Tenant = tenant;
		}

		public TenantScopedServiceDescriptor(ITenant tenant, Type serviceType, object instance)
			: base(serviceType, instance)
		{
			Tenant = tenant;
		}

		public TenantScopedServiceDescriptor(ITenant tenant, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime serviceLifetime)
			: base(serviceType, factory, serviceLifetime)
		{
			Tenant = tenant;
		}
	}
}
