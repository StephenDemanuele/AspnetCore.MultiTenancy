using System;

namespace AspnetCore.ServiceCollection.MultiTenancy.UnitTests.Stubs.Services
{
	internal interface IFooService
	{
		Guid Id { get; }

		string Get();
	}

	internal class DefaultFooService : IFooService
	{
		public DefaultFooService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Default foo service: {Id}";
	}

	internal class SpecialFooService : IFooService
	{
		public SpecialFooService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Special foo service: {Id}";
	}
}
