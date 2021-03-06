﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XFactory.SqdcWatcher.DataAccess.Migrations
{
    [DbContext(typeof(SqdcDbContext))]
    [Migration("20200426035602_AddStockHistory")]
    partial class AddStockHistory
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("SqdcWatcher.Domain.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Brand")
                        .HasColumnType("TEXT");

                    b.Property<string>("CannabisType")
                        .HasColumnType("TEXT");

                    b.Property<string>("LevelTwoCategory")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProducerName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Quality")
                        .HasColumnType("TEXT");

                    b.Property<string>("Strain")
                        .HasColumnType("TEXT");

                    b.Property<string>("TerpeneDetailed")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("SqdcWatcher.Domain.ProductVariant", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<double>("DisplayPrice")
                        .HasColumnType("REAL");

                    b.Property<double>("GramEquivalent")
                        .HasColumnType("REAL");

                    b.Property<bool>("InStock")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastInStockTimestamp")
                        .HasColumnType("TEXT");

                    b.Property<double>("ListPrice")
                        .HasColumnType("REAL");

                    b.Property<double>("PricePerGram")
                        .HasColumnType("REAL");

                    b.Property<string>("ProductId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductVariant");
                });

            modelBuilder.Entity("SqdcWatcher.Domain.SpecificationAttribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ProductVariantId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PropertyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ProductVariantId");

                    b.ToTable("SpecificationAttribute");
                });

            modelBuilder.Entity("SqdcWatcher.Domain.StockHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Event")
                        .HasColumnType("TEXT");

                    b.Property<long>("ProductVariantId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StockHistory");
                });

            modelBuilder.Entity("SqdcWatcher.Domain.ProductVariant", b =>
                {
                    b.HasOne("SqdcWatcher.Domain.Product", "Product")
                        .WithMany("Variants")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SqdcWatcher.Domain.SpecificationAttribute", b =>
                {
                    b.HasOne("SqdcWatcher.Domain.ProductVariant", "ProductVariant")
                        .WithMany("Specifications")
                        .HasForeignKey("ProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
