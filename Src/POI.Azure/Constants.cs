using System;
using POI.ThirdParty.Core.Services;

namespace POI.Azure
{
	internal class Constants : IConstants
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