using System;

namespace SampleUsage.Services.Contracts
{
	public interface IFooService
	{
		Guid Id { get; }

		string Get();
	}
}
