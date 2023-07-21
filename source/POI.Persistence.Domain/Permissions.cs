namespace POI.Persistence.Domain
{
	[Flags]
	public enum Permissions
	{
		None = 0,
		LinkApproval = 1 << 0,
		ForceLink = 1 << 1
	}
}