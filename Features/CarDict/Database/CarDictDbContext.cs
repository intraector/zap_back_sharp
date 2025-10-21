using App.Data.Entities.CarDict;
using Microsoft.EntityFrameworkCore;

namespace App.Data
{
	public class CarDictDbContext : DbContext
	{
		public CarDictDbContext(DbContextOptions<CarDictDbContext> options) : base(options) { }

		public DbSet<BrandEntity> Brands { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<BrandEntity>(b =>
			{
				b.ToTable("car_mark");
				b.HasKey(x => x.Id);
				b.Property(x => x.Id).HasColumnName("id_car_mark");
				b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
			});
		}
	}
}
