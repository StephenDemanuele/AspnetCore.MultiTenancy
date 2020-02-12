using System;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs.Services
{
	internal interface IBarService
	{
		Guid Id { get; }

		string Get();
	}

	internal class BarService : IBarService
	{
		public BarService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Bar service: {Id}";
	}

}
