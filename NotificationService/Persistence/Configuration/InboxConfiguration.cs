using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Persistence.Entities;

namespace NotificationService.Persistence.Configuration;

public class InboxConfiguration : IEntityTypeConfiguration<Inbox>
{
	public void Configure(EntityTypeBuilder<Inbox> builder)
	{
		builder.HasKey(i => new { i.NotificationId, i.Source });
		builder.Property(i => i.Source).HasMaxLength(50);
	}
}
