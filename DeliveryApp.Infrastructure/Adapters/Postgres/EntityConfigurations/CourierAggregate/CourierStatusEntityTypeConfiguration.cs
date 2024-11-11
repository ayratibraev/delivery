using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;

internal class CourierStatusEntityTypeConfiguration : IEntityTypeConfiguration<CourierStatus>
{
    public void Configure(EntityTypeBuilder<CourierStatus> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("courier_statuses");

        entityTypeBuilder.HasKey(entity => entity.Id);

        entityTypeBuilder
           .Property(entity => entity.Id)
           .ValueGeneratedNever()
           .HasColumnName("id")
           .IsRequired();

        entityTypeBuilder
           .Property(entity => entity.Name)
           .HasColumnName("name")
           .IsRequired();
    }
}