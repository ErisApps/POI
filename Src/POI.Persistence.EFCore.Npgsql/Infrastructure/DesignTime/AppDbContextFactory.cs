using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POI.Persistence.EFCore.Npgsql.Infrastructure.DesignTime;

internal class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseNpgsql(args[0], o => o.UseNodaTime());

		return new AppDbContext(optionsBuilder.Options);
	}
}