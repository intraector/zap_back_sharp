using App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Account> Accounts { get; set; } = null!;
		public DbSet<PhoneCode> PhoneCodes { get; set; } = null!;
		public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
		public DbSet<ZapEntity> Zaps { get; set; } = null!;
		public DbSet<SpareEntity> Spares { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}
	}
}
