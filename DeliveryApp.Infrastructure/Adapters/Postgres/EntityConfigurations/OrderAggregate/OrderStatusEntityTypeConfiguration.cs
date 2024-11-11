using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;

internal class OrderStatusEntityTypeConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("order_statuses");

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