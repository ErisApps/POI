using System.Reflection;
using System.Runtime.Loader;
using DryIoc;
using POI.DiscordDotNet.Module.Base;

namespace POI.DiscordDotNet.Core.Loader
{
    internal class HostAssemblyLoadContext : AssemblyLoadContext, IUnloadable
    {
	    private readonly WeakReference _depContainerWeakRef;
	    private readonly AssemblyDependencyResolver _dependencyResolver;

	    private WeakReference<IModuleEntrypoint>? _moduleEntrypointWeakRef;

	    public HostAssemblyLoadContext(IContainer container, string modulePath) : base(name: "HostAssemblyLoadContext", isCollectible: true)
	    {
		    _depContainerWeakRef = new WeakReference(container);
            _dependencyResolver = new AssemblyDependencyResolver(modulePath);
        }

        public void InitAndTrackEntrypoint(IModuleEntrypoint moduleEntrypoint)
        {
            if (moduleEntrypoint == null)
            {
                throw new ArgumentNullException(nameof(moduleEntrypoint), "Entrypoint should not be null, this would result in faulty unloading");
            }

            _moduleEntrypointWeakRef = new WeakReference<IModuleEntrypoint>(moduleEntrypoint);

            moduleEntrypoint.Initialize((_depContainerWeakRef.Target as IContainer)!);
        }

        public void RequestUnload()
        {
            if (_moduleEntrypointWeakRef == null)
            {
                Console.WriteLine("ModuleEntrypoint not set, can't unload properly.");
                return;
            }

            if (_moduleEntrypointWeakRef.TryGetTarget(out var moduleInterface))
            {
                moduleInterface.Unload((_depContainerWeakRef.Target as IContainer)!);
            }

            Unload();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                Console.WriteLine($"Loading assembly {assemblyPath} into the HostAssemblyLoadContext");
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}