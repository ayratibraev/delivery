﻿using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;

internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("orders");

        entityTypeBuilder.HasKey(entity => entity.Id);

        entityTypeBuilder
           .Property(entity => entity.Id)
           .ValueGeneratedNever()
           .HasColumnName("id")
           .IsRequired();

        entityTypeBuilder
           .Property(entity => entity.CourierId)
           .HasColumnName("courier_id")
           .IsRequired(false);

        entityTypeBuilder
           .HasOne(entity => entity.Status)
           .WithMany()
           .IsRequired()
           .HasForeignKey("status_id");

        entityTypeBuilder
           .OwnsOne(entity => entity.Location, l =>
            {
                l.Property(x => x.X).HasColumnName("location_x").IsRequired();
                l.Property(y => y.Y).HasColumnName("location_y").IsRequired();
                l.WithOwner();
            });
    }
}