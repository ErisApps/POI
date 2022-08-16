using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DryIoc;
using POI.DiscordDotNet.Module.Base;

namespace POI.DiscordDotNet.Core.Loader
{
	public class ModuleLoader
	{
		private readonly IContainer _container;
		private readonly Dictionary<string, WeakReference> _alcWeakRefDict;

		public ModuleLoader(IContainer container)
		{
			_container = container;
			_alcWeakRefDict = new Dictionary<string, WeakReference>();
		}

		public uint LoadedModuleCount => (uint) _alcWeakRefDict.Count;

		public void Load(string fullPath)
		{
			if (!File.Exists(fullPath))
			{
				// File not found
				return;
			}

			if (_alcWeakRefDict.ContainsKey(fullPath))
			{
				// Module already loaded
				return;
			}

			Console.WriteLine($"Starting loading of \"{fullPath}\"");

			if (LoadInternal(fullPath, out var alcWeakRef))
			{
				_alcWeakRefDict.Add(fullPath, alcWeakRef);
			}

			_ = LoadedModuleCount;
		}

		public void Unload(string fullPath)
		{
			if (UnloadInternal(fullPath, out var unloadableWeakRef))
			{
				// Poll and run GC until the AssemblyLoadContext is unloaded.
				// You don't need to do that unless you want to know when the context
				// got unloaded. You can just leave it to the regular GC.
				for (var i = 0; unloadableWeakRef.IsAlive && (i < 10); i++)
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}

				Console.WriteLine($"Unload success: {!unloadableWeakRef.IsAlive}");

				_ = LoadedModuleCount;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private bool LoadInternal(string assemblyPath, [NotNullWhen(true)] out WeakReference? unloadableWeakRef)
		{
			try
			{
				var alc = new HostAssemblyLoadContext(_container, assemblyPath);

				unloadableWeakRef = new WeakReference(alc);

				var assembly = alc.LoadFromAssemblyPath(assemblyPath);

				var rawPluginType = assembly
					.GetTypes()
					.Single(t => t.IsAssignableTo(typeof(IModuleEntrypoint)));

				var plugin = (IModuleEntrypoint) Activator.CreateInstance(rawPluginType)!;

				alc.InitAndTrackEntrypoint(plugin);

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				unloadableWeakRef = null;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private bool UnloadInternal(string fullPath, [NotNullWhen(true)] out WeakReference? alcWeakRef)
		{
			if (_alcWeakRefDict.Remove(fullPath, out alcWeakRef) && alcWeakRef.Target is IUnloadable unloadable)
			{
				unloadable.RequestUnload();
				return true;
			}

			return false;
		}
	}
}