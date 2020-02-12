using System;
using SampleUsage.Services.Contracts;

namespace SampleUsage.Services
{
	public class SpecialFooService : IFooService
	{
		public SpecialFooService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Special foo service: {Id}";
	}
}
