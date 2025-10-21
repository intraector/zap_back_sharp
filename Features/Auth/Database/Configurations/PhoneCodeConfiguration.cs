using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Features.Auth.Database.Configurations
{
	public class PhoneCodeConfiguration : IEntityTypeConfiguration<PhoneCode>
	{
		public void Configure(EntityTypeBuilder<PhoneCode> b)
		{
			b.ToTable("phone_codes");
			b.HasKey(x => x.Id);
			b.Property(x => x.Id).HasColumnName("id");
			b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
			b.Property(x => x.Code).HasColumnName("code").HasMaxLength(6);
			b.Property(x => x.CreatedAt).HasColumnName("created_at");
		}
	}
}
