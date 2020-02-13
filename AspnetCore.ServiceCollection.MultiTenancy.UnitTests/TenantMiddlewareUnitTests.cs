using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AspnetCore.ServiceCollection.MultiTenancy.Builder;
using AspnetCore.ServiceCollection.MultiTenancy.Extensions;
using AspnetCore.ServiceCollection.MultiTenancy.Middleware;
using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs.Services;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests
{
	public class TenantMiddlewareUnitTests
	{
		private readonly IList<ITenant> _tenants;

		public TenantMiddlewareUnitTests()
		{
			_tenants = new List<ITenant>
			{
				new MockTenant(1),
				new MockTenant(2)
			};
		}

		[Fact]
		public async Task InvokeCall_succeeds()
		{
			var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			var mockHttpRequest = new Mock<HttpRequest>();
			mockHttpRequest.Setup(_ => _.Headers)
				.Returns(new HeaderDictionary { { "tenantId", "1" } });
			mockHttpContextAccessor.Setup(x => x.HttpContext.Request)
				.Returns(mockHttpRequest.Object);

			//setup serviceCollection
			var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

			foreach (var tenant in _tenants)
			{
				if (tenant.Id == 1)
					serviceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);
				else
					serviceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Scoped);

				//this singleton is tenant scoped.
				//there will be a singleton for each tenant
				serviceCollection.AddTenantScoped(tenant, typeof(IBarService), typeof(BarService), ServiceLifetime.Singleton);
			}

			serviceCollection.AddSingleton(_ => mockHttpContextAccessor);
			serviceCollection.AddSingleton<ITenantProvider, MockTenantProvider>();
			serviceCollection.AddSingleton<ITenantResolutionStrategy, HeaderTenantResolutionStrategy>();
			serviceCollection.AddSingleton<IBazService, BazService>();
			serviceCollection.AddSingleton(mockHttpContextAccessor.Object);

			mockHttpContextAccessor.Setup(x => x.HttpContext.RequestServices)
				.Returns(serviceCollection.BuildServiceProvider());

			string foo = null;
			var sut = new TenantMiddleware(next: (innerHttpContext) =>
			{
				//do something in next request delegate
				foo = "executed";
				return Task.CompletedTask;
			},
			new ContainerBuilder(_tenants, serviceCollection));

			await sut.Invoke(mockHttpContextAccessor.Object.HttpContext);
			Assert.Equal("executed", foo);
		}

		[Fact]
		public async Task TenantSpecificServices_ShouldBeInjected()
		{
			//setup IHttpContextAccessor
			var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			var mockHttpRequest = new Mock<HttpRequest>();
			mockHttpRequest.Setup(_ => _.Headers)
				.Returns(new HeaderDictionary { { "tenantId", "2" } });
			mockHttpContextAccessor.Setup(x => x.HttpContext.Request)
				.Returns(mockHttpRequest.Object);

			//setup serviceCollection
			var masterServiceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

			foreach (var tenant in _tenants)
			{
				if (tenant.Id == 1)
					masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Scoped);
				else
					masterServiceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);

				//this singleton is tenant scoped.
				//there will be a singleton for each tenant
				masterServiceCollection.AddTenantScoped(tenant, typeof(IBarService), typeof(BarService), ServiceLifetime.Singleton);
			}

			masterServiceCollection.AddSingleton<ITenantProvider, MockTenantProvider>();
			masterServiceCollection.AddSingleton<ITenantResolutionStrategy, HeaderTenantResolutionStrategy>();
			masterServiceCollection.AddSingleton<IBazService, BazService>();
			masterServiceCollection.AddSingleton(mockHttpContextAccessor.Object);

			mockHttpContextAccessor.Setup(x => x.HttpContext.RequestServices)
				.Returns(masterServiceCollection.BuildServiceProvider());

			var sut = new TenantMiddleware(next: (innerHttpContext) =>
			{
				var fooService = innerHttpContext.RequestServices.GetService<IFooService>();
				Assert.NotNull(fooService);
				Assert.IsType<DefaultFooService>(fooService);

				return Task.CompletedTask;
			},
			new ContainerBuilder(_tenants, masterServiceCollection));

			await sut.Invoke(mockHttpContextAccessor.Object.HttpContext);
		}
	}
}
