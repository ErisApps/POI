using System;
using POI.Core.Services;

namespace POI.Azure
{
	internal class Constants : IConstantsCore
	{
		public string Name { get; }
		public Version Version { get; }

		public Constants()
		{
			Name = "POI Online";
			Version = typeof(Constants).Assembly.GetName().Version!;
		}
	}
}