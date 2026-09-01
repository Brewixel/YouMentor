using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Persistence.Entities;

namespace NotificationService.Persistence.Configuration;

public class HistoryConfiguration : IEntityTypeConfiguration<History>
{
	public void Configure(EntityTypeBuilder<History> builder)
	{
		builder.Property(h => h.Source).HasMaxLength(50);
		builder.Property(h => h.Channel).HasMaxLength(50);
	}
}
