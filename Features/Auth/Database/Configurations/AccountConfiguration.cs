using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Auth.Database.Configurations
{
	public class AccountConfiguration : IEntityTypeConfiguration<Account>
	{
		public void Configure(EntityTypeBuilder<Account> b)
		{
			b.ToTable("accounts");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired(false);
			b.Property(x => x.Status).HasColumnName("status");
			b.Property(x => x.CreatedAt).HasColumnName("created_at");
			b.Property(x => x.Roles).HasColumnName("roles").HasColumnType("integer[]");
		}
	}
}
