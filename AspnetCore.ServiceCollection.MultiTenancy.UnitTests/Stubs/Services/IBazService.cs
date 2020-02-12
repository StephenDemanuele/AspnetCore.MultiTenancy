using System;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs.Services
{
	internal interface IBazService
	{
		Guid Id { get; }

		string Get();
	}

	internal class BazService : IBazService
	{
		public Guid Id { get; }

		public BazService()
		{
			Id = Guid.NewGuid();
		}

		public string Get() => $"Baz service: {Id}";
	}
}
