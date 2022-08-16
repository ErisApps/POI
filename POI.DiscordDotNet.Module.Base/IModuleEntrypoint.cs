using DryIoc;

namespace POI.DiscordDotNet.Module.Base
{
	public interface IModuleEntrypoint
	{
		string Name { get; }
		string Description { get; }

		void Initialize(IContainer container);
		void Unload(IContainer container);
	}
}