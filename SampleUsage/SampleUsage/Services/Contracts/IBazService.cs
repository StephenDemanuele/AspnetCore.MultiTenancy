using System;

namespace SampleUsage.Services.Contracts
{
	public interface IBazService
	{
		Guid Id { get; }

		string Get();
	}
}
