using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests
{
	public class HeaderTenantResolutionStrategyUnitTests
	{
		private HeaderTenantResolutionStrategy _sut;
		private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
		private ITenantProvider _tenantProvider ;


		public HeaderTenantResolutionStrategyUnitTests()
		{
			_tenantProvider = new MockTenantProvider();
			_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			_sut = new HeaderTenantResolutionStrategy(_mockHttpContextAccessor.Object, _tenantProvider);
		}

		[Theory]
		[InlineData("1", 1)]
		[InlineData("2", 2)]
		public async Task ShouldResolve_ToCorrectTenant(string tenantId, int expectedTenantId)
		{
			var context = new DefaultHttpContext();
			context.Request.Headers["tenantId"] = tenantId;

			_mockHttpContextAccessor.Setup(x => x.HttpContext)
				.Returns(context);

			var tenant = await _sut.GetTenantAsync();
			Assert.True(tenant.Id == expectedTenantId);
		}

		[Fact]
		public async Task MissingTenantIdHeader_shouldThrowException()
		{
			var context = new DefaultHttpContext();

			_mockHttpContextAccessor.Setup(x => x.HttpContext)
				.Returns(context);

			var exception = await Record.ExceptionAsync(() => _sut.GetTenantAsync());
			Assert.IsType<InvalidOperationException>(exception);
		}

		[Fact]
		public async Task InvalidTenantId_shouldThrowException()
		{
			var context = new DefaultHttpContext();
			context.Request.Headers["tenantId"] = "foo";

			_mockHttpContextAccessor.Setup(x => x.HttpContext)
				.Returns(context);

			var exception = await Record.ExceptionAsync(() => _sut.GetTenantAsync());
			Assert.IsType<InvalidOperationException>(exception);
		}
	}
}