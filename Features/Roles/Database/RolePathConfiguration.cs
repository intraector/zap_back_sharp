using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Roles.Database
{
	public class RolePathConfiguration : IEntityTypeConfiguration<RolePathEntity>
	{
		public void Configure(EntityTypeBuilder<RolePathEntity> b)
		{
			b.ToTable("roles");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.Path).HasColumnName("path").HasMaxLength(255);
			b.Property(x => x.Role).HasColumnName("role");
			b.Property(x => x.Label).HasColumnName("label").HasMaxLength(127);
		}
	}
}
