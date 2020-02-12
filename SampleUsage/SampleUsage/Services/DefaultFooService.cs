using System;
using SampleUsage.Services.Contracts;

namespace SampleUsage.Services
{
	internal class DefaultFooService : IFooService
	{
		public DefaultFooService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Default foo service: {Id}";
	}
}
