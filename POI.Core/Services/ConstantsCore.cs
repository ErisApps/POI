using System;
using POI.Core.Services.Interfaces;

namespace POI.Core.Services
{
	public abstract class ConstantsCore : IConstantsCore
	{
		public abstract string Name { get; }
		public abstract Version Version { get; }
	}
}