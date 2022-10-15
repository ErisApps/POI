using System;
using POI.Core.Services;

namespace POI.Azure
{
	internal class Constants : ConstantsCore
	{
		public override string Name { get; }
		public override Version Version { get; }

		public Constants()
		{
			Name = "POI Online";
			Version = typeof(Constants).Assembly.GetName().Version!;
		}
	}
}