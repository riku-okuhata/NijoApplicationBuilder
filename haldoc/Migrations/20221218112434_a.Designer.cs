﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using haldoc;

namespace haldoc.Migrations
{
    [DbContext(typeof(DynamicDbContext))]
    [Migration("20221218112434_a")]
    partial class a
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.17");

            modelBuilder.Entity("haldoc.AutoGenerated.Entities.取引先", b =>
                {
                    b.Property<string>("企業ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("企業名")
                        .HasColumnType("TEXT");

                    b.Property<int?>("重要度")
                        .HasColumnType("INTEGER");

                    b.HasKey("企業ID");

                    b.ToTable("取引先");
                });

            modelBuilder.Entity("haldoc.AutoGenerated.Entities.取引先支店", b =>
                {
                    b.Property<string>("会社__企業ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("支店ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("支店名")
                        .HasColumnType("TEXT");

                    b.HasKey("会社__企業ID", "支店ID");

                    b.ToTable("取引先支店");
                });

            modelBuilder.Entity("haldoc.AutoGenerated.Entities.営業所", b =>
                {
                    b.Property<string>("営業所ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("営業所名")
                        .HasColumnType("TEXT");

                    b.HasKey("営業所ID");

                    b.ToTable("営業所");
                });

            modelBuilder.Entity("haldoc.AutoGenerated.Entities.担当者", b =>
                {
                    b.Property<string>("ユーザーID")
                        .HasColumnType("TEXT");

                    b.Property<string>("所属__営業所ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("氏名")
                        .HasColumnType("TEXT");

                    b.HasKey("ユーザーID");

                    b.ToTable("担当者");
                });

            modelBuilder.Entity("haldoc.AutoGenerated.Entities.請求情報", b =>
                {
                    b.Property<string>("取引先__企業ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("住所__丁番地")
                        .HasColumnType("TEXT");

                    b.Property<string>("住所__市町村")
                        .HasColumnType("TEXT");

                    b.Property<string>("住所__郵便番号")
                        .HasColumnType("TEXT");

                    b.Property<string>("住所__都道府県")
                        .HasColumnType("TEXT");

                    b.Property<string>("宛名")
                        .HasColumnType("TEXT");

                    b.HasKey("取引先__企業ID");

                    b.ToTable("請求情報");
                });
#pragma warning restore 612, 618
        }
    }
}
