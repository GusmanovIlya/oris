using EfCoreHomework.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace EfCoreHomework.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

        modelBuilder.Entity("EfCoreHomework.Models.Category", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Categories");

            b.HasData(
                new
                {
                    Id = 1,
                    Name = "Ноутбуки"
                },
                new
                {
                    Id = 2,
                    Name = "Смартфоны"
                },
                new
                {
                    Id = 3,
                    Name = "Аксессуары"
                });
        });

        modelBuilder.Entity("EfCoreHomework.Models.Product", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER");

            b.Property<int>("CategoryId")
                .HasColumnType("INTEGER");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("TEXT");

            b.Property<decimal>("Price")
                .HasColumnType("decimal(10,2)");

            b.HasKey("Id");

            b.HasIndex("CategoryId");

            b.ToTable("Products");

            b.HasData(
                new
                {
                    Id = 1,
                    CategoryId = 1,
                    Name = "Lenovo IdeaPad 3",
                    Price = 55000m
                },
                new
                {
                    Id = 2,
                    CategoryId = 1,
                    Name = "Apple MacBook Air",
                    Price = 120000m
                },
                new
                {
                    Id = 3,
                    CategoryId = 2,
                    Name = "Samsung Galaxy S23",
                    Price = 85000m
                },
                new
                {
                    Id = 4,
                    CategoryId = 2,
                    Name = "iPhone 14",
                    Price = 95000m
                },
                new
                {
                    Id = 5,
                    CategoryId = 3,
                    Name = "Logitech Mouse",
                    Price = 2500m
                },
                new
                {
                    Id = 6,
                    CategoryId = 3,
                    Name = "USB-C Cable",
                    Price = 900m
                });
        });

        modelBuilder.Entity("EfCoreHomework.Models.Product", b =>
        {
            b.HasOne("EfCoreHomework.Models.Category", "Category")
                .WithMany("Products")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Category");
        });

        modelBuilder.Entity("EfCoreHomework.Models.Category", b =>
        {
            b.Navigation("Products");
        });
#pragma warning restore 612, 618
    }
}
