using System;

namespace SampleUsage.Services.Contracts
{
	public interface IBarService
	{
		Guid Id { get; }

		string Get();
	}
}
