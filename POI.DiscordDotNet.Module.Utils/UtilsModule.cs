using System.Diagnostics;
using DryIoc;
using POI.DiscordDotNet.Module.Base;
using POI.DiscordDotNet.Module.Utils.Services;
using POI.DiscordDotNet.Module.Utils.Services.Interfaces;

namespace POI.DiscordDotNet.Module.Utils
{
	public class UtilsModule : IModuleEntrypoint
	{
		private IContainer? _utilsModuleScopedContainer;

		public string Name => nameof(UtilsModule);
		public string Description => $"Hii from the lovely {nameof(UtilsModule)} ^^";

		public void Initialize(IContainer container)
		{
			_utilsModuleScopedContainer = container.CreateChild();
			_utilsModuleScopedContainer.Register<ITestService, TestService>(Reuse.Singleton);

			Debugger.Break();
		}

		public void Unload(IContainer container)
		{
			_utilsModuleScopedContainer?.Dispose();
			_utilsModuleScopedContainer = null;

			Debugger.Break();
		}
	}
}