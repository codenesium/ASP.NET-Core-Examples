using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkExample.Models
{
    public partial class IOTContext : DbContext
    {
        public IOTContext()
        {
        }

        public IOTContext(DbContextOptions<IOTContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Device> Device { get; set; }
        public virtual DbSet<DeviceAction> DeviceAction { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=localhost;Persist Security Info=False;User ID=test;Password=Passw0rd;Initial Catalog=IOT;MultipleActiveResultSets=True;Connection Timeout=300;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasIndex(e => e.PublicId)
                    .HasName("IX_Device")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(90)
                    .IsUnicode(false);

                entity.Property(e => e.PublicId).HasColumnName("publicId");
            });

            modelBuilder.Entity<DeviceAction>(entity =>
            {
                entity.HasIndex(e => e.DeviceId)
                    .HasName("IX_DeviceAction_DeviceId");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeviceId).HasColumnName("deviceId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(90)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.DeviceAction)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeviceAction_Device");
            });
        }
    }
}
