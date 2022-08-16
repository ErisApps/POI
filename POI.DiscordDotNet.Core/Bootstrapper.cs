using System.Diagnostics;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using POI.DiscordDotNet.Core.Loader;

namespace POI.DiscordDotNet.Core
{
	internal static class Bootstrapper
	{
		public static void Main(string[] args)
		{
			var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());
			container.Register<ModuleLoader>(Reuse.Singleton);

			var moduleLoader = container.Resolve<ModuleLoader>();

			var utilsModulePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\POI.DiscordDotNet.Module.Utils\\bin\\Debug\\net6.0\\POI.DiscordDotNet.Module.Utils.dll"));
			moduleLoader.Load(utilsModulePath);

			moduleLoader.Unload(utilsModulePath);

			var test = container.WithoutCache();

			var serviceProvider = container.BuildServiceProvider();

			moduleLoader.Load(utilsModulePath);

			moduleLoader.Unload(utilsModulePath);

			Debugger.Break();
		}
	}
}