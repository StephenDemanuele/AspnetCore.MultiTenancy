using System;
using SampleUsage.Services.Contracts;

namespace SampleUsage.Services
{
	public class BarService : IBarService
	{
		public BarService()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string Get() => $"Bar service: {Id}";
	}
}
