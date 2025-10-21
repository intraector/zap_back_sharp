using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Dict.Region.Database
{
	public class RegionConfiguration : IEntityTypeConfiguration<RegionEntity>
	{
		public void Configure(EntityTypeBuilder<RegionEntity> b)
		{
			b.ToTable("region"); // mapped to existing table name 'region'
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
			b.Property(x => x.Enabled).HasColumnName("enabled");
		}
	}
}
