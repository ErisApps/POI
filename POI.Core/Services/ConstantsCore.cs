using System;
using POI.Core.Services.Interfaces;

namespace POI.Core.Services
{
	public abstract class ConstantsCore : IConstantsCore
	{
		public const string API_KEY_HEADER_NAME = "X-API-Key";

		public abstract string Name { get; }
		public abstract Version Version { get; }
	}
}