using System;
using SampleUsage.Services.Contracts;

namespace SampleUsage.Services
{
	public class BazService : IBazService
	{
		public Guid Id { get; }

		public BazService()
		{
			Id = Guid.NewGuid();
		}

		public string Get() => $"Baz service: {Id}";
	}
}
