using AspnetCore.ServiceCollection.MultiTenancy.TenantResolution;
using AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests
{
	public class Tests
	{
		private HeaderTenantResolutionStrategy _sut;
		private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
		private ITenantProvider _tenantProvider ;

		[SetUp]
		public void Setup()
		{
			_tenantProvider = new MockTenantProvider();
			_mockHttpContextAccessor =  new Mock<IHttpContextAccessor>();
			_sut = new HeaderTenantResolutionStrategy(_mockHttpContextAccessor.Object, _tenantProvider);
		}

		[Theory()]
		public async Task Test1()
		{
			var context = new DefaultHttpContext();
			context.Request.Headers["tenantId"] = "1";

			_mockHttpContextAccessor.Setup(x => x.HttpContext)
				.Returns(context);

			var tenant = await _sut.GetTenantAsync();
			Assert.That(tenant.Id == 1);
		}
	}
}