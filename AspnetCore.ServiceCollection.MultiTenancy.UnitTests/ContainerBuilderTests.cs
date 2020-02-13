using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AspnetCore.ServiceCollection.MultiTenancy.Builder;
using AspnetCore.ServiceCollection.MultiTenancy.Extensions;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs.Services;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests
{
    public class ContainerBuilderTests
    {
        private readonly IList<ITenant> _tenants;

        public ContainerBuilderTests()
        {
            _tenants = new List<ITenant>
            {
                new MockTenant(1),
                new MockTenant(2)
            };
        }

        [Fact]
        public void TenantTransientLifetime_shouldbeDifferentWithinSameScope()
        {
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            
            var tenant = _tenants[0];
            masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Transient);

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            IFooService tenant1FooService1, tenant1FooService2;
            using (var scope1 = tenantContainers[1].CreateScope())
            {
                tenant1FooService1 = scope1.ServiceProvider.GetService<IFooService>();
                tenant1FooService2 = scope1.ServiceProvider.GetService<IFooService>();
                Assert.IsType<SpecialFooService>(tenant1FooService1);
                Assert.IsType<SpecialFooService>(tenant1FooService2);
                //Same scope, should be different instance
                Assert.NotEqual(tenant1FooService1.Id, tenant1FooService2.Id);
            }
        }

        [Fact]
        public void TenantScopedLifetime_shouldbeSameAs_scoped()
        {
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            foreach (var tenant in _tenants)
            {
                if (tenant.Id == 1)
                    masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Scoped);
                else
                    masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);
            }

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            IFooService tenant1FooService1, tenant1FooService2;
            using (var scope1 = tenantContainers[1].CreateScope())
            {
                tenant1FooService1 = scope1.ServiceProvider.GetService<IFooService>();
                tenant1FooService2 = scope1.ServiceProvider.GetService<IFooService>();
                Assert.IsType<SpecialFooService>(tenant1FooService1);
                Assert.IsType<SpecialFooService>(tenant1FooService2);
                //Same scope, should be same instance
                Assert.Equal(tenant1FooService1.Id, tenant1FooService2.Id);
            }
            using (var scope2 = tenantContainers[1].CreateScope())
            {
                var tenant1FooService3 = scope2.ServiceProvider.GetService<IFooService>();
                //Different scope, should be different instance
                Assert.NotEqual(tenant1FooService1.Id, tenant1FooService3.Id);
            }
        }

        [Fact]
        public void TenantSpecificRegistrations_shouldBeCorrect()
        {
            //setup serviceCollection
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            foreach (var tenant in _tenants)
            {
                if (tenant.Id == 1)
                    masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Scoped);
                else
                    masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);
            }

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            using (var scope1 = tenantContainers[1].CreateScope())
            {
                var tenant1FooService = scope1.ServiceProvider.GetService<IFooService>();
                Assert.IsType<SpecialFooService>(tenant1FooService);
            }

            using (var scope2 = tenantContainers[2].CreateScope())
            {
                var tenant2FooService = scope2.ServiceProvider.GetService<IFooService>();
                Assert.IsType<DefaultFooService>(tenant2FooService);
            }
        }

        [Fact]
        public void TenantScopedSingletonRegistrationsForSameTenant_shouldBeTheSameAcrossScopes()
        {
            //setup serviceCollection
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            foreach (var tenant in _tenants)
            {
                //this singleton is tenant scoped. There will be a singleton for each tenant
                masterServiceCollection.AddTenantScoped(tenant, typeof(IBarService), typeof(BarService), ServiceLifetime.Singleton);
            }

            //masterServiceCollection.AddSingleton<IBazService, BazService>();

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            IBarService barService1, barService2;
            using (var scope1 = tenantContainers[1].CreateScope())
            {
                barService1 = scope1.ServiceProvider.GetService<IBarService>();
            }

            using (var scope2 = tenantContainers[1].CreateScope())
            {
                barService2 = scope2.ServiceProvider.GetService<IBarService>();
            }

            Assert.Equal(barService1.Id, barService2.Id);
        }

        [Fact]
        public void TenantScopedSingletonRegistrationsForDifferentTenant_shouldBeTheSameAcrossScopes()
        {
            //setup serviceCollection
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            foreach (var tenant in _tenants)
            {
                //this singleton is tenant scoped. There will be a singleton for each tenant
                masterServiceCollection.AddTenantScoped(tenant, typeof(IBarService), typeof(BarService), ServiceLifetime.Singleton);
            }

            //masterServiceCollection.AddSingleton<IBazService, BazService>();

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            IBarService tenant1BarService1, tenant2BarService1;
            using (var scope1 = tenantContainers[1].CreateScope())
            {
                tenant1BarService1 = scope1.ServiceProvider.GetService<IBarService>();
            }

            using (var scope2 = tenantContainers[2].CreateScope())
            {
                tenant2BarService1 = scope2.ServiceProvider.GetService<IBarService>();
            }

            Assert.NotEqual(tenant1BarService1.Id, tenant2BarService1.Id);
        }

        [Fact]
        public void ApplicationScopedSingletonRegistrationsForDifferentTenants_shouldBeTheSameAcrossScopes()
        {
            //setup serviceCollection
            var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            masterServiceCollection.AddSingleton<IBazService, BazService>();

            var sut = new ContainerBuilder(_tenants, masterServiceCollection);
            var tenantContainers = sut.Build();

            IBazService tenant1BazService1, tenant2BazService1;
            using (var scope1 = tenantContainers[1].CreateScope())
            {
                tenant1BazService1 = scope1.ServiceProvider.GetService<IBazService>();
            }

            using (var scope2 = tenantContainers[2].CreateScope())
            {
                tenant2BazService1 = scope2.ServiceProvider.GetService<IBazService>();
            }

            Assert.Equal(tenant1BazService1.Id, tenant2BazService1.Id);
        }
    }
}
