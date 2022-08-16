namespace POI.DiscordDotNet.Core.Loader
{
    /// <summary>
    /// Interface existing solely for limiting the access to the HostAssemblyLoadContext by normal means
    /// </summary>
    public interface IUnloadable
    {
        void RequestUnload();
    }
}