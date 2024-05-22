using System;
using Microsoft.EntityFrameworkCore;
using quest_web.Models;
using EntityFramework.Exceptions.SqlServer;

namespace quest_web.Contexts
{
    public class APIDbContext : DbContext
    {
        internal object User;
        internal object Address;
        internal object Shop;
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Address>()
                .Property<int?>("user_id");
            builder.Entity<Address>()
                .HasOne(p => p.user)
                .WithMany(b => b.Addresses)
                .HasForeignKey("user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.Entity<ShopAddress>()
                .Property<int?>("shop_id");
            builder.Entity<ShopAddress>()
                .HasOne(s => s.shop)
                .WithMany(t => t.ShopAddresses)
                .HasForeignKey("shop_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.Entity<HandiHelper>()
                .HasMany(left => left.Shops)
                .WithMany(right => right.HandiHelpers)
                .UsingEntity(join => join.ToTable("shop_helpers"));

            builder.Entity<Mark>()
                .Property<int?>("user_id");
            builder.Entity<Mark>()
                .HasOne(p => p.user)
                .WithMany(b => b.Marks)
                .HasForeignKey("user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Mark>()
                .Property<int?>("shop_id");
            builder.Entity<Mark>()
                .HasOne(p => p.shop)
                .WithMany(b => b.Marks)
                .HasForeignKey("shop_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Review>()
                .Property<int?>("user_id");
            builder.Entity<Review>()
                .HasOne(p => p.user)
                .WithMany(b => b.Reviews)
                .HasForeignKey("user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Review>()
                .Property<int?>("shop_id");
            builder.Entity<Review>()
                .HasOne(p => p.shop)
                .WithMany(b => b.Reviews)
                .HasForeignKey("shop_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Commentary>()
                .Property<int?>("user_id");
            builder.Entity<Commentary>()
                .HasOne(p => p.user)
                .WithMany(b => b.Commentaries)
                .HasForeignKey("user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Commentary>()
                .Property<int?>("shop_id");
            builder.Entity<Commentary>()
                .HasOne(p => p.shop)
                .WithMany(b => b.Commentaries)
                .HasForeignKey("shop_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopAddress> ShopAddresses { get; set; }
        public DbSet<Mark> Marks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Commentary> Commentaries { get; set; }
    }

}
