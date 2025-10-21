using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Auth.Database.Configurations
{
	public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> b)
		{
			b.ToTable("refresh_tokens");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.UserId).HasColumnName("user_id");
			b.Property(x => x.Token).HasColumnName("token").HasMaxLength(1000);
			b.Property(x => x.CreatedAt).HasColumnName("created_at");
		}
	}
}
