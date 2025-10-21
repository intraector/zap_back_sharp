using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Zaps.Database.Configurations
{
	public class ZapEntityConfiguration : IEntityTypeConfiguration<ZapEntity>
	{
		public void Configure(EntityTypeBuilder<ZapEntity> b)
		{
			b.ToTable("zaps");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.BrandId).HasColumnName("brand_id");
			b.Property(x => x.ModelId).HasColumnName("model_id");
			b.Property(x => x.BodyId).HasColumnName("body_id");
			b.Property(x => x.GenerationId).HasColumnName("generation_id");
			b.Property(x => x.ModificationId).HasColumnName("modification_id");
			b.Property(x => x.RegionId).HasColumnName("region_id");
			b.Property(x => x.RegionName).HasColumnName("regionName");
			b.Property(x => x.Description).HasColumnName("desc").HasMaxLength(1000);
		}
	}

	public class SpareEntityConfiguration : IEntityTypeConfiguration<SpareEntity>
	{
		public void Configure(EntityTypeBuilder<SpareEntity> b)
		{
			b.ToTable("spares");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.ZapId).HasColumnName("brand_id");
			b.Property(x => x.Description).HasColumnName("desc").HasMaxLength(1000);
			b.Property(x => x.Status).HasColumnName("status");
			b.Property(x => x.Photos).HasColumnName("photos");
			b.HasOne(x => x.Zap).WithMany(z => z.Spares).HasForeignKey(x => x.ZapId);
		}
	}
}
